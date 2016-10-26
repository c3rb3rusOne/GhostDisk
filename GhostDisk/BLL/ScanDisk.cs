using System;
//using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using GhostDisk.BO;

//! Diag activité à modif
//! ATTENTION out of memory et problèmes de droits sur larges volumes !

namespace GhostDisk.BLL
{
    class ScanDisk
    {
        // Ou public event EventHandler<paramsEvent> updateProgressbar; // paramEvent = objet perso
        public event EventHandler<int> updateProgressbar;
        public event EventHandler<int> initializeProgressbar;

        // Test, pour compter le nb total de fichiers ds un dossier/volume
        private long nbFilesToScan = 0;

        // Nombre de caractères du chemin du dossier (ou lecteur) à sauvegarder
        private ushort BaseFolderPathLenght = 0;

        private Options options = null;

        private int cursor = 1; //? long
        private int refreshInterval = 0;

        public ScanDisk(Options options)
        {
            if (options != null)
                this.options = options;
            //static création de l'objet
        }

        // Initialisation d'une sauvegarde
        public bool Start_ScanDisk(string folderPath, string backupPath, CancellationToken CancellationToken)
        {
            // Test l'existance des dossiers à sauvegarder et de sauvegarde
            if (folderPath == null || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Properties.Resources.ERROR_MAIN_FOLDER_UNEXIST);
                return false;
            }
            if (backupPath == null || !Directory.Exists(backupPath))
            {
                //sizeof disque existe le créer ?
                MessageBox.Show(Properties.Resources.ERROR_BACKUP_FOLDER_UNEXIST); // Le créer ?
                return false;
            }
            
            this.BaseFolderPathLenght = (ushort)folderPath.Length;

            // Initialiser refreshInterval
            //! Beaucoup trop long sur des gros volumes
            this.countNbFilesInDirectoryAndSubdirectory(new DirectoryInfo(folderPath));
            MessageBox.Show("nb files to scan : " + nbFilesToScan.ToString());
            //DirectoryInfo .GetFiles(folderPath, "*.*", SearchOption.AllDirectories).Length - 1;//Directory.GetFiles(folderPath).Length; //! Différents selons les options (remarque ça passe qd même car updater avant continue...)
            
            // Avantage ne rafraîchit pas à ts les fichiers pr de grands nb de fichiers
            /*decimal nbSteps = Math.Round(decimal.Divide(100, nbFilesToScan), 3);
            nbSteps = Math.Round(decimal.Divide(1, nbSteps), 3);
            this.refreshInterval = Convert.ToInt32(Math.Ceiling(nbSteps)); // Ou cast direct ou decimal.toInt32, ou floor -> arrondi inférieur
            ;*/

            // Par fichier si nbFichiers<=100 sinon par pas de +- 2%
            if (nbFilesToScan > 100)
            {
                decimal deuxPrct = Math.Round(decimal.Divide(nbFilesToScan * 2, 100));
                this.refreshInterval = Convert.ToInt32(deuxPrct);
            }
            else
                this.refreshInterval = 1;
           
            // Préparer la progressbar
            this.initializeProgressbar?.Invoke(new object(), (int)nbFilesToScan); 

            bool error = FolderScan_RecursiveMode(folderPath, backupPath, CancellationToken);

            return error;
        }
        
        // Méthode récursive dossier par dossier
        private bool FolderScan_RecursiveMode(string folderPath, string backupPath, CancellationToken CancellationToken)
        {
            // Si l'un des dossier dossier à sauvegarder ou dossier cible est manquant, sortir - Demander création dossier cible ?
            if (folderPath == null || !Directory.Exists(@folderPath))
            {
                MessageBox.Show(Properties.Resources.ERROR_MAIN_FOLDER_UNEXIST);
                return false;
            }

            //!! TEST des DROITS d'accès au dossier
            //DirectorySecurity dirSec = Directory.GetAccessControl(folderPath);

            string[] Filestab = null;
            try
            {
                Filestab = Directory.GetFiles(folderPath);
            }
            catch (UnauthorizedAccessException) { return false; }
            
            // Pour chaques fichiers du dossier principal puis des sous dossier, Create file object (bdd)
            //foreach (string file in Directory.GetFiles(folderPath)) //'System.UnauthorizedAccessException'
            foreach (string file in Filestab) // -> for
            {
                FileInfo fileInfo = null; // sortir ?
                //fonction update si > 50 fich pr 3000 //!
                throwUpdateProgressbar();

                // Test si le fichier source existe toujours
                if (!System.IO.File.Exists(file))
                    continue;

                fileInfo = new FileInfo(file);

                // Exécute le tri selon les filtres
                if (options.UseIncludeExcludeFilter)
                {
                    if (!testIncludeExcludeFilter(fileInfo))
                        continue;
                }

                // Créer le fichier de sauvegarde / Enregistrer liste des fichiers n'ayant pus êtres sauvegardés
                CreateFile(fileInfo, backupPath);
                
                // Vérifier si il n'y a pas eu une annulation de la tâche
                if (CancellationToken.IsCancellationRequested)
                {
                    // ou try/catch return ici
                    CancellationToken.ThrowIfCancellationRequested();
                }
            }

            // Pour chaques sous-dossiers, créer le dossier dans le dossier accueillant la sauvegarde et rappeler la fonction
            foreach (string folderInFolder in Directory.GetDirectories(folderPath))
            {
                //test droits
                CreateFolder(folderInFolder, backupPath);
                FolderScan_RecursiveMode(folderInFolder, backupPath, CancellationToken);
            }

            return true;
        }

        // Renvoi true si le fichier en paramètre est conforme aux filtres d'inclusion/exclusion sinon flase
        private bool testIncludeExcludeFilter(FileInfo fileInfo)
        {
            // Si l'extension du fichier testé est contenue dans la liste d'extensions à filtrer
            if (options.extensionsToFilter.Contains(fileInfo.Extension.ToLower()))
            {
                // Et que l'on est en mode exclusion retourner false
                if (options.includeExcludeFilterType == 2) // 1 -> include, 2 -> exclude
                    return false;
            }
            // Si extension NON contenue dans la liste d'extensions à filtrer et que l'on est en mode inclusion retourner false
            else if (options.includeExcludeFilterType == 1)
                return false;
            
            return true;
        }
        
        // Créer ou copie les fichiers vers le dossier de sauvegarde
        private bool CreateFile(FileInfo OriginalFileInfo, string BackupFolderPath)
        {        
            // DANGER: System.IO.File.Create will overwrite the file if it already exists.

            // Chemin de sauvegarde du fichier (emplacement + nom + extension)
            string BackupPath = BackupFolderPath + @"\" + OriginalFileInfo.FullName.Remove(0, BaseFolderPathLenght); //BackupPath StringBuilder ? garbageCollector ?

            /*********************Exceptions selon type de fichiers (à sortir)****************************************/
            #region Cas spécial - Icônes
            if (OriginalFileInfo.Extension.Equals(".lnk")) // à mettre ds constantes
            {
                try
                {
                    System.IO.File.Copy(OriginalFileInfo.FullName, BackupPath, true);
                }
                catch (PathTooLongException ptle)
                {
                    if (Constantes.DEBUG)
                        //MessageBox.Show("Icon: Nom de fichier trop long " + BackupPath + " - " + ptle.ToString());

                    // Si le chemin du fichier est supérieur à 259: suppression des caractères en trop et ajout de --- à la fin du nom de fichier (259 = taille max des chemins sur windows < 10)
                    if (BackupPath.Length >= 259) // ou ds catch
                    {
                        BackupPath = BackupPath.Remove(256 - OriginalFileInfo.Extension.Length) + "---" + OriginalFileInfo.Extension;
                        OriginalFileInfo.CopyTo(BackupPath, true);
                    }
                }
                catch (UnauthorizedAccessException uae) { return false; }

                return true;
            }
            #endregion

            #region Test Option - backupSomeFiles
            // Si option backupSomeFiles activée, extension à sauvegarder et taille du fichier <= taille max admissible (unité comparaison: Kb) : copier
            if (this.options != null && this.options.backupSomeFiles)
            {
                if (options.extensionsToBackup.Contains(OriginalFileInfo.Extension) && OriginalFileInfo.Length <= this.options.FileToSaveMaxSizeInKb)
                {
                    try
                    {
                        //System.IO.File.Copy(OriginalFileInfo.FullName, BackupPath, true);
                        OriginalFileInfo.CopyTo(BackupPath, true); //!! DROITS
                    }
                    catch (PathTooLongException ptle)
                    {
                        if (Constantes.DEBUG)
                            //MessageBox.Show("File: Nom de fichier trop long " + BackupPath + " - " + ptle.ToString());

                        // Si le chemin du fichier est supérieur à 259: suppression des caractères en trop et ajout de --- à la fin du nom de fichier (259 = taille max des chemins sur windows < 10)
                        if (ptle is PathTooLongException && BackupPath.Length >= 259) // ou ds catch
                        {
                            BackupPath = BackupPath.Remove(256 - OriginalFileInfo.Extension.Length) + "---" +
                                         OriginalFileInfo.Extension;
                            OriginalFileInfo.CopyTo(BackupPath, true);
                        }
                    }
                    catch (UnauthorizedAccessException uae) { return false; }

                    return true;
                }
            }
            #endregion

            #region Cas spécial - Images
            // Si le fichier est une image, récupérer et sauvegarder la vignette de prévisualisation (thumbnail)
            if (OriginalFileInfo.Extension.Equals(".JPG")) //minuscules const
            {
                Image thumbnailForFile = GetThumbnail(OriginalFileInfo.FullName);

                try
                {
                    thumbnailForFile.Save(BackupPath + OriginalFileInfo.Name);
                }
                catch (PathTooLongException ptle)
                {
                    if (BackupPath.Length >= 259)
                    {
                        BackupPath = BackupPath.Remove(256 - OriginalFileInfo.Extension.Length) + "---" + OriginalFileInfo.Extension;
                        thumbnailForFile.Save(BackupPath + OriginalFileInfo.Name);
                    }
                }
                catch (UnauthorizedAccessException uae) { return false; }
                finally { thumbnailForFile.Dispose(); }

                return true;
            }
            #endregion
            /*********************Fin exceptions selon type de fichiers (à sortir)************************************/

            #region Cas normaux
            // Si aucuns cas spéciaux décris plus haut alors création de la copie "vide" du fichier
            try
            {
                using (System.IO.FileStream fs = System.IO.File.Create(BackupPath))
                {
                    #region Cas spécial Exe
                    // Si exe, créer fichier vierge et copier l'icone.
                    if (OriginalFileInfo.Extension.Equals(".exe")) //! const
                    {
                        // Set a default icon for the file.
                        Icon iconForFile = Icon.ExtractAssociatedIcon(OriginalFileInfo.FullName); //path - SystemIcons.WinLogo;
                        
                        iconForFile?.Save(fs);
                        /* old way
                        if (iconForFile != null)
                            iconForFile.Save(fs); */

                        iconForFile?.Dispose();
                    }
                    #endregion
                    fs.Dispose();
                }
            }
            catch (PathTooLongException ex)
            {
                if (Constantes.DEBUG)
                    //MessageBox.Show("erreur création" + ex.Message); // const

                // Si le chemin du fichier est supérieur à 259: suppression des caractères en trop et ajout de --- à la fin du nom de fichier (259 = taille max des chemins sur windows < 10)
                if (BackupPath.Length >= 259) // ou ds catch
                {
                    BackupPath = BackupPath.Remove(256 - OriginalFileInfo.Extension.Length) + "---" + OriginalFileInfo.Extension;

                    using (System.IO.FileStream fs = System.IO.File.Create(BackupPath))
                    {
                        #region Cas spécial Exe
                        if (OriginalFileInfo.Extension.Equals(".exe")) //! const, sortir + factoriser
                        {
                            Icon iconForFile = Icon.ExtractAssociatedIcon(OriginalFileInfo.FullName);
                            iconForFile?.Save(fs);
                            iconForFile?.Dispose();
                        }

                        #endregion
                        fs.Dispose();
                    }

                    return true;
                }
                
                return false;
            }
            catch (UnauthorizedAccessException uae) { return false; }
            #endregion

            return true;
        }

        // Créer un dossier dossier dans le repertoire de sauvegarde
        private bool CreateFolder(string OriginalFolderPath, string BackupFolderPath) // Les dossiers sont-ils crées automatiquement selon le chemin des fichiers ? -> non
        {
            string BackupPath = null;

            /* Chemin de sauvegarde du dossier (chemin du dossier cible + (chemin du dossier source - le nombre de caractères
               correspondants au chemin du dossier source) */
            BackupPath = BackupFolderPath + @"\" + OriginalFolderPath.Remove(0, BaseFolderPathLenght);
            
            try
            {
                Directory.CreateDirectory(BackupPath);
            }
            catch (PathTooLongException ptle) // Ajouter unhotorized acess ...
            {
                if (Constantes.DEBUG)
                    //MessageBox.Show("Erreur création dossier" + ptle.Message); // const

                // Si le chemin du dossier est supérieur à 259: suppression des caractères en trop et ajout de --- à la fin du nom de fichier (sur windows < 10)
                if (BackupPath.Length >= 259)
                {
                    BackupPath = BackupPath.Remove(256) + "---";
                    Directory.CreateDirectory(BackupPath);
                }
            }
            catch (UnauthorizedAccessException uae) { return false; }

            return true;
        }

        // Callback fonction récupération du thumbnail - Pas utilisé pour le moment
        public bool ThumbnailCallback()
        {
            return true;
        }

        // Fonction d'extraction du thumbnail d'une image
        private Image GetThumbnail(string imagePath)
        {
            Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
            Image image = new Bitmap(@imagePath);
            Image pThumbnail = image.GetThumbnailImage(100, 100, callback, new IntPtr());
            
            return pThumbnail;
        }

        private void throwUpdateProgressbar()
        {
            if (cursor >= this.refreshInterval)
            {
                cursor = 1;
                this.updateProgressbar?.Invoke(new object(), this.refreshInterval);
            }
            else
            {
                cursor++;
            }
        }

        // Ok, mais long (plus de 3 min) pr les gros volumes ( >800 000 fichiers)
        private bool countNbFilesInDirectoryAndSubdirectory(DirectoryInfo di)
        {
            // Compter les fichiers du dossier
            try
            {
                if ((di.Attributes & FileAttributes.System) == 0) // If(directory != @"e:/System Volume Information")
                {
                    if (di.GetFiles().Length>0)
                        nbFilesToScan += di.GetFiles().Length;
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show("1 " + exception.ToString());
                return false; //noError=
            }
            
            // Puis ceux de chacun des sous-dossier
            try
            {
                foreach (var subDirectory in di.EnumerateDirectories())
                {
                    countNbFilesInDirectoryAndSubdirectory(subDirectory);
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show("2 " + exception.ToString());
                return false;
            }
            

            return true;
        }
    }
}
