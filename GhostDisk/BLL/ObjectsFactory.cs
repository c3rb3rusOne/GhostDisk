using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GhostDisk.BO;

namespace GhostDisk.BLL
{
    public static class ObjectsFactory
    {
        // Création d'un objet Options à partir du SQLiteDataReader produit par la requête getOptions et des listes extensionsToBackup et extensionsToFilter
        public static Options newOptions(SQLiteDataReader reader, List<string> extensionsToBackup, List<string> extensionsToFilter)
        {
            /*for (int i =0; i<10;i++)
                MessageBox.Show(reader.GetName(i).ToString());*/

            bool useBackupSomeFiles = Convert.ToInt32(reader["useBackupSomeFiles"])==1 ? true : false; // ToInt64 = long
            int profile = Convert.ToInt32(reader["optionId"]); //int profile = int.Parse(reader["optionId"].ToString());
            int maxSizeOfBackupFiles = (int) Convert.ToInt32(reader["maxSizeOfBackupFiles"]);
            int sizeUnitIndex = Convert.ToInt32(reader["sizeUnitIndex"]); //int.Parse()
            string[] extensionsToBackupTab = new string[extensionsToBackup.Count];
            string[] extensionsToFilterTab = new string[extensionsToFilter.Count];
            bool useIncludeExcludeFilter = reader["useIncludeExcludeFilter"].ToString() != "0";
            int includeExcludeFilterType = int.Parse(reader["includeExcludefilterType"].ToString());
            string lastSourceFolderPath = reader["lastSourceFolderPath"].ToString();
            string lastBackupFolderPath = reader["lastBackupFolderPath"].ToString();

            //for (int i = 0; i < extensionsList.Count; i++) // dictionary not sorted !!
            int i=0;
            foreach (string extension in extensionsToBackup)
            {
                extensionsToBackupTab[i] = extension; //extensions.ElementAt(i);
                i++;
            }
            i = 0;
            foreach (string extension in extensionsToFilter)
            {
                extensionsToFilterTab[i] = extension;
                i++;
            }

            return new Options(profile, useBackupSomeFiles, maxSizeOfBackupFiles, sizeUnitIndex, extensionsToBackupTab, extensionsToFilterTab, useIncludeExcludeFilter, includeExcludeFilterType, lastSourceFolderPath, lastBackupFolderPath);
        }
    }
}