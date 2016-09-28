using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace GhostDisk.BO
{
    public class Options
    {
        public int profile;
        public bool backupSomeFiles;
        public string[] extensionsToBackup;
        public string[] extensionsToFilter;
        public int /*ulong*/ FileToBackupMaxSize;
        public int FileToSaveMaxSizeInKb;
        public string lastSourceFolderPath;
        public string lastBackupFolderPath;
        public int sizeUnitIndex;
        public bool UseIncludeExcludeFilter; // true: extensions exclues, false: extensions uniquements
        public int includeExcludeFilterType;

        //public Collection<string>;
        //Dictionary<string, int>
        //list Or arrayList

        // propriété par défaut const
        public Options()
        {
            this.profile = 1;
            this.backupSomeFiles = true;
            this.UseIncludeExcludeFilter = false; // true: extensions exclues, false: extensions uniquements
            this.includeExcludeFilterType = 1; // 1 -> include
            this.FileToBackupMaxSize = 10;
            this.sizeUnitIndex = Constantes.defaultFileToSaveUnitIndex;
            this.FileToSaveMaxSizeInKb = FileToBackupMaxSize * Constantes.valuesOfUnitsInKb[sizeUnitIndex-1]; // index commence à 1 en bdd et 0 ds les arrays
            this.extensionsToBackup = new string[] { ".txt" };
            this.extensionsToFilter = new string[] { "" };
            this.lastSourceFolderPath = "";
            this.lastBackupFolderPath = "";
        }

        //pas ds le meme ordre que la bdd
        public Options(int profile, bool backupSomeFiles, int tailleMaxFichierTxt, int sizeUnitIndex, string[] extensionsToBackup, string[] extensionsToFilter, bool useIncludeExcludeFilter, int includeExcludeFilterType, string lastSourceFolderPath, string lastBackupFolderPath) //afficher les unité depuis la bdd
        {
            this.profile = profile > 0 ? profile : 1;            
            this.backupSomeFiles = backupSomeFiles;
            this.FileToBackupMaxSize = tailleMaxFichierTxt > 0 ? tailleMaxFichierTxt : 10;
            this.sizeUnitIndex = sizeUnitIndex > 0 ? sizeUnitIndex : 1;
            this.FileToSaveMaxSizeInKb = tailleMaxFichierTxt * Constantes.valuesOfUnitsInKb[sizeUnitIndex - 1]; // index commence à 1 en bdd et 0 ds les arrays
            this.extensionsToBackup = extensionsToBackup.Length > 0 ? extensionsToBackup : new string[] {".txt"};
            this.extensionsToFilter = extensionsToFilter.Length > 0 ? extensionsToFilter : new string[] { "" };
            this.UseIncludeExcludeFilter = useIncludeExcludeFilter;
            this.includeExcludeFilterType = includeExcludeFilterType > 0 && includeExcludeFilterType <= 2 ? includeExcludeFilterType : 1;
            this.lastSourceFolderPath = lastSourceFolderPath.Length>1 ? lastSourceFolderPath : "";
            this.lastBackupFolderPath = lastBackupFolderPath.Length > 1 ? lastBackupFolderPath : "";
        }
    }
}
//diag flux