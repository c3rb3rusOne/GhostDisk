using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace GhostDisk.BO
{
    public class Options // parametres
    {
        public int profile;
        public string profileName;
        public bool backupSomeFiles;
        public string[] extensionsToBackup;
        public string[] extensionsToFilter;
        public int FileToBackupMaxSize;
        public int FileToSaveMaxSizeInKb;
        public int sizeUnitIndex;
        public bool UseIncludeExcludeFilter; // true: extensions exclues, false: extensions uniquements
        public int includeExcludeFilterType;
        
        public Options()
        {
            this.profile = 1; //?
            this.profileName = "Profil 1 (Défaut)";//?
            this.backupSomeFiles = true;
            this.UseIncludeExcludeFilter = false;
            this.includeExcludeFilterType = 1; // 1: extensions à sauvegarder exclusivement, 2: extensions à exclures
            this.FileToBackupMaxSize = 10;
            this.sizeUnitIndex = Constantes.defaultFileToSaveUnitIndex;
            this.FileToSaveMaxSizeInKb = FileToBackupMaxSize * Constantes.valuesOfUnitsInKb[sizeUnitIndex-1]; // index commence à 1 en bdd et 0 ds les arrays
            this.extensionsToBackup = new string[] { ".txt" };
            this.extensionsToFilter = new string[] { "" }; //?
        }

        //! Pas ds le meme ordre que la bdd
        public Options(int profile, string profilName, bool backupSomeFiles, int tailleMaxFichierTxt, int sizeUnitIndex, string[] extensionsToBackup, string[] extensionsToFilter, bool useIncludeExcludeFilter, int includeExcludeFilterType) //afficher les unité depuis la bdd
        {
            this.profile = profile > 0 ? profile : 1;
            this.profileName = profilName;
            this.backupSomeFiles = backupSomeFiles;
            this.FileToBackupMaxSize = tailleMaxFichierTxt > 0 ? tailleMaxFichierTxt : 10;
            this.sizeUnitIndex = sizeUnitIndex > 0 ? sizeUnitIndex : 1;
            this.FileToSaveMaxSizeInKb = tailleMaxFichierTxt * Constantes.valuesOfUnitsInKb[sizeUnitIndex - 1]; // index commence à 1 en bdd et 0 ds les arrays
            this.extensionsToBackup = extensionsToBackup.Length > 0 ? extensionsToBackup : new string[] {".txt"};
            this.extensionsToFilter = extensionsToFilter.Length > 0 ? extensionsToFilter : new string[] { "" };
            this.UseIncludeExcludeFilter = useIncludeExcludeFilter;
            this.includeExcludeFilterType = includeExcludeFilterType > 0 && includeExcludeFilterType <= 2 ? includeExcludeFilterType : 1;
        }
    }
}