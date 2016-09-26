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
        public string[] extensionsToSave;
        public int /*ulong*/ tailleMaxFichierTxt;
        public int tailleMaxFichierTxtInKb;
        public string lastSourceFolderPath;
        public string lastBackupFolderPath;
        public int sizeUnitIndex;
        

        //public Collection<string>;
        //Dictionary<string, int>
        //list Or arrayList



        public bool traitementParExtension;
        public string[] extentions;
        public bool exclusionInclusion; // true: extensions exclues, false: extensions uniquements

        // propriété par défaut const
        public Options()
        {
            this.profile = 1;
            this.backupSomeFiles = true;
            this.tailleMaxFichierTxt = 10;
            this.sizeUnitIndex = 1; //const
            this.tailleMaxFichierTxtInKb = tailleMaxFichierTxt * Constantes.valuesOfUnitsInKb[sizeUnitIndex-1]; // index commence à 1 en bdd et 0 ds les arrays
            this.extensionsToSave = new string[] { ".txt" };
            this.lastSourceFolderPath = "";
            this.lastBackupFolderPath = "";
        }

        //pas ds le meme ordre que la bdd
        public Options(int profile, bool backupSomeFiles, int tailleMaxFichierTxt, int sizeUnitIndex, string[] extensionsToSave, string lastSourceFolderPath, string lastBackupFolderPath) //afficher les unité depuis la bdd
        {
            this.profile = profile > 0 ? profile : 1;            
            this.backupSomeFiles = backupSomeFiles;
            this.tailleMaxFichierTxt = tailleMaxFichierTxt > 0 ? tailleMaxFichierTxt : 10;
            this.sizeUnitIndex = sizeUnitIndex > 0 ? sizeUnitIndex : 1;
            this.tailleMaxFichierTxtInKb = tailleMaxFichierTxt * Constantes.valuesOfUnitsInKb[sizeUnitIndex - 1]; // index commence à 1 en bdd et 0 ds les arrays
            this.extensionsToSave = extensionsToSave.Length > 0 ? extensionsToSave : new string[] {".txt"};
            this.lastSourceFolderPath = lastSourceFolderPath.Length>1 ? lastSourceFolderPath : "";
            this.lastBackupFolderPath = lastBackupFolderPath.Length > 1 ? lastBackupFolderPath : "";
        }
    }
}
//diag flux