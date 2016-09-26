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
        /*public static Folder newFolder(int id, string name)
        {
            return new Folder(id, name);
        }*/

        public static Options newOptions(SQLiteDataReader reader, List<string> extensionsList)
        {
            /*for (int i =0; i<10;i++)
                MessageBox.Show(reader.GetName(i).ToString());*/

            bool backupTxtFiles = Convert.ToInt32(reader["backupTxtFiles"])==1 ? true : false; // ToInt64 = long
            int profile = Convert.ToInt32(reader["optionId"]);
            int maxSizeOfBackupTxtFiles = (int) Convert.ToInt32(reader["maxSizeOfBackupTxtFiles"]);
            //string sizeUnit = Convert.ToString(reader["sizeUnitName"]);
            int sizeUnitIndex = Convert.ToInt32(reader["sizeUnitIndex"]);
            string[] extensions = new string[extensionsList.Count];
            string lastSourceFolderPath = reader["lastSourceFolderPath"].ToString();
            string lastBackupFolderPath = reader["lastBackupFolderPath"].ToString();

            //for (int i = 0; i < extensionsList.Count; i++) // dictionary not sorted !!
            int i=0;
            foreach (string extension in extensionsList)
            {
                extensions[i] = extension.ToString(); //extensions.ElementAt(i);
                i++;
            }

            return new Options(profile, backupTxtFiles, maxSizeOfBackupTxtFiles, sizeUnitIndex, extensions, lastSourceFolderPath, lastBackupFolderPath);
        }
    }
}