using System;
//using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using GhostDisk.BO;

namespace GhostDisk.BLL
{
    class ScanDisk
    {
        // Nombre de caractères du chemin du dossier (ou lecteur) à sauvegarder
        private ushort BaseFolderPathLenght = 0;
        private Options options = null;

        public ScanDisk(Options options)
        {
            if (options != null)
                this.options = options;
            //static création de l'objet
        }

        // Initialisation d'une sauvegarde
        public bool Start_ScanDisk(string folderPath, string backupPath)
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

            bool error = FolderScan_RecursiveMode(folderPath, backupPath);

            if (!error)
                return false;
            return true;
        }
        
        // Méthode récursive dossier par dossier (fichier par fichier ?)
        private bool FolderScan_RecursiveMode(string folderPath, string backupPath)
        {
            // Si l'un des dossier dossier à sauvegarder ou dossier cible est manquant, sortir - Demander création dossier cible ?
            if (folderPath == null || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Properties.Resources.ERROR_MAIN_FOLDER_UNEXIST);
                return false;
            }

            //Pour chaques fichiers du dossier principal puis des sous dossier, Create file object (bdd)
            foreach (string file in Directory.GetFiles(folderPath)) //'System.UnauthorizedAccessException'
            {
                FileInfo fileInfo = null;

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

                // Créer le fichier de sauvegarde
                CreateFile(fileInfo, backupPath);
            }

            // Pour chaques sous-dossiers, créer le dossier dans le dossier accueillant la sauvegarde et rappeler la fonction
            foreach (string folderInFolder in Directory.GetDirectories(folderPath))
            {
                CreateFolder(folderInFolder, backupPath);
                FolderScan_RecursiveMode(folderInFolder, backupPath);
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
            string BackupPath = BackupFolderPath + OriginalFileInfo.FullName.Remove(0, BaseFolderPathLenght); //BackupPath StringBuilder ? garbageCollector ?

            /*********************Exceptions selon type de fichiers (à sortir)****************************************/
            // Si raccourci, le copier
            if (OriginalFileInfo.Extension.Equals(".lnk"))
            {
                //System.IO.File.Copy(OriginalFilePath, BackupPath, true);
                System.IO.File.Copy(OriginalFileInfo.FullName, BackupPath, true);
                return true;
            }

            // Si option backupSomeFiles activée, extension à sauvegarder et taille du fichier <= taille max admissible (unité comparaison: Kb) : copier
            if (this.options != null && this.options.backupSomeFiles)
            {
                if (options.extensionsToBackup.Contains(OriginalFileInfo.Extension) && OriginalFileInfo.Length <= this.options.FileToSaveMaxSizeInKb)
                {
                    System.IO.File.Copy(OriginalFileInfo.FullName, BackupPath, true);
                    return true;
                }
            }

            // Si le fichier est une image, récupérer et sauvegarder la vignette de prévisualisation (thumbnail)
            if (OriginalFileInfo.Extension.Equals(".JPG")) //minuscules const
            {
                Image thumbnailForFile = GetThumbnail(OriginalFileInfo.FullName);
                //Image image = Image.FromFile(OriginalFileInfo.FullName);
                //Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);

                thumbnailForFile.Save(BackupPath + OriginalFileInfo.Name);
                thumbnailForFile.Dispose();
                    
                return true;
            }
            
            // Si aucuns cas spéciaux décris plus haut alors création de la copie "vide" du fichier
            try
            {
                using (System.IO.FileStream fs = System.IO.File.Create(BackupPath))
                {
                    //fs.WriteByte(50);
                    // Si exe, créer fichier vierge et copier l'icone.
                    if (OriginalFileInfo.Extension.Equals(".exe"))
                    {
                        // Set a default icon for the file.
                        Icon iconForFile = Icon.ExtractAssociatedIcon(OriginalFileInfo.FullName); //path - SystemIcons.WinLogo;

                        iconForFile?.Save(fs);
                        /* old way
                        if (iconForFile != null)
                            iconForFile.Save(fs); */

                        iconForFile.Dispose();
                        fs.Dispose();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Constantes.DEBUG)
                {
                    MessageBox.Show("erreur création" + ex.Message); // const
                }

                return false;
            }
            /*********************Fin exceptions selon type de fichiers (à sortir)************************************/

            return true;
        }

        // Créer ou copie les fichiers vers le dossier de sauvegarde
        private bool CreateFolder(string OriginalFolderPath, string BackupFolderPath) // Les dossiers sont-ils crées automatiquement selon le chemin des fichiers ? -> non
        {
            //MessageBox.Show("crea dossier " + OriginalFolderPath);
            //string fileName = System.IO.Path.GetRandomFileName();
            //string fileName2 = "MyNewFile.txt";

            // Use Combine again to add the file name to the path.
            //pathString = System.IO.Path.Combine(pathString, fileName);
            
            string BackupPath = null;

            /* Chemin de sauvegarde du dossier (chemin du dossier cible + (chemin du dossier source - le nombre de caractères
               correspondants au chemin du dossier source) */
            BackupPath = BackupFolderPath + OriginalFolderPath.Remove(0, BaseFolderPathLenght); ;
            
            try
            {
                Directory.CreateDirectory(BackupPath);
            }
            catch (Exception ex)
            {
                if (Constantes.DEBUG)
                {
                    MessageBox.Show("erreur création dossier" + ex.Message); // const
                }
            }
            
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

    }
}
