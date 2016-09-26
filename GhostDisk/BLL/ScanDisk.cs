using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GhostDisk.BO;

namespace GhostDisk.BLL
{
    class ScanDisk
    {
        private string BaseFolderPath = null; // Chemin complet vers le dossier (ou disque) à sauvegarder
        private ushort BaseFolderPathLenght = 0;
        //private string BackupPath = null;
        private Options options = null;

        public ScanDisk(Options options)
        {
            if (options != null)
                this.options = options;
            //static création de l'objet
        }

        public bool Start_ScanDisk(string folderPath, string backupPath)
        {
            // Teste l'existance des dossiers à sauvegarder et de sauvegarde
            if (folderPath == null || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Properties.Resources.ERROR_MAIN_FOLDER_UNEXIST);
                return false;
            }
            if (backupPath == null || !Directory.Exists(backupPath))
            {
                //sizeof disque existe le créer ?
                MessageBox.Show(Properties.Resources.ERROR_BACKUP_FOLDER_UNEXIST); // LE CR2ER ;
                return false;
            }

            this.BaseFolderPath = folderPath;
            this.BaseFolderPathLenght = (ushort)folderPath.Length;
            //this.BackupPath = backupPath;

            bool error = FolderScan_RecursiveMode(folderPath, backupPath);

            if (!error)
                return false;
            return true;
        }
        
        //inscan field
        // Méthode récursive dossier par dossier (fichier par fichier ?)
        private bool FolderScan_RecursiveMode(string folderPath, string backupPath)
        {
            if (folderPath == null || !Directory.Exists(folderPath))
            {
                MessageBox.Show(Properties.Resources.ERROR_MAIN_FOLDER_UNEXIST);
                return false;
            }

            //Pour chaques fichiers du dossier principal puis des sous dossier, Create file object (bdd)
            foreach (string file in Directory.GetFiles(folderPath)) //'System.UnauthorizedAccessException'
            {
                CreateFile(file, backupPath);
            }

            // Pour chaques sous-dossiers
            foreach (string folderInFolder in Directory.GetDirectories(folderPath))
            {
                CreateFolder(folderInFolder, backupPath);
                FolderScan_RecursiveMode(folderInFolder, backupPath);
            }

            return true;
        }
        
        //profondeur - base ?
        private bool CreateFile(string OriginalFilePath, string BackupFolderPath)
        {
            string BackupPath = null; //StringBuilder ? garbageCollector ?
            
            // DANGER: System.IO.File.Create will overwrite the file if it already exists.
            if (System.IO.File.Exists(OriginalFilePath))
            {
                FileInfo OriginalFileInfo = new FileInfo(OriginalFilePath); 
                BackupPath = BackupFolderPath + OriginalFileInfo.FullName.Remove(0, BaseFolderPathLenght); ;

                /*********************Exceptions selon type de fichiers (à sortir)****************************************/
                // Si raccourci, le copier
                if (OriginalFileInfo.Extension.Equals(".lnk"))
                {
                    System.IO.File.Copy(OriginalFilePath, BackupPath, true);
                    return true;
                }

                // Si option backupSomeFiles activée, extension à sauvegarder et taille du fichier <= taille max admissible (unité comparaison: Kb) : copier
                if (this.options != null && this.options.backupSomeFiles)
                {
                    if (options.extensionsToSave.Contains(OriginalFileInfo.Extension) && OriginalFileInfo.Length <= this.options.tailleMaxFichierTxtInKb)
                    {
                        System.IO.File.Copy(OriginalFilePath, BackupPath, true);
                        return true;
                    }
                }

                if (OriginalFileInfo.Extension.Equals(".JPG")) //minuscules
                {
                    Image thumbnailForFile = GetThumbnail(OriginalFileInfo.FullName);
                    //Image image = Image.FromFile(OriginalFileInfo.FullName);
                    //Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);

                    thumbnailForFile.Save(BackupPath + OriginalFileInfo.Name);
                    thumbnailForFile.Dispose();
                    
                    return true;
                }
                
                try
                {
                    using (System.IO.FileStream fs = System.IO.File.Create(BackupPath))
                    {
                        //fs.WriteByte(50);
                        // Si exe, créer fichier vierge et copier l'icone.
                        if (OriginalFileInfo.Extension.Equals(".exe"))
                        {
                            // Set a default icon for the file.
                            //Icon iconForFile = SystemIcons.WinLogo;
                            Icon iconForFile = Icon.ExtractAssociatedIcon(OriginalFilePath);

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
                    MessageBox.Show("erreur création" + ex.Message);
                    return false;
                }
                /*********************Fin exceptions selon type de fichiers (à sortir)************************************/
            }
            else
            {
                //MessageBox.Show(Properties.Resources.ERROR_MAIN_FOLDER_UNEXIST); //("File \"{0}\" already exists.", fileName);
                return false;
            }

            return true;
        }

        private bool CreateFolder(string OriginalFolderPath, string BackupFolderPath) // Les dossiers sont-ils crées automatiquement selon le chemin des fichiers ? -> non
        {
            //MessageBox.Show("crea dossier " + OriginalFolderPath);
            //string fileName = System.IO.Path.GetRandomFileName();
            //string fileName2 = "MyNewFile.txt";

            // Use Combine again to add the file name to the path.
            //pathString = System.IO.Path.Combine(pathString, fileName);
            
            string BackupPath = null;
            
            BackupPath = BackupFolderPath + OriginalFolderPath.Remove(0, BaseFolderPathLenght); ;
            //MessageBox.Show(BackupPath);
                //ico exe
            try
            {
                Directory.CreateDirectory(BackupPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("erreur création dossier" + ex.Message);
            }
            
            return true;
        }

        public bool ThumbnailCallback()
        {
            return true;
        }

        private Image GetThumbnail(string imagePath)
        {
            Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
            Image image = new Bitmap(@imagePath);
            Image pThumbnail = image.GetThumbnailImage(100, 100, callback, new IntPtr());
            
            return pThumbnail;
        }

    }
}
