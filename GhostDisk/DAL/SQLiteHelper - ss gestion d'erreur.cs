using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using GhostDisk.BO;
using System.Windows.Forms;
using GhostDisk.BLL;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

// for messageBox

namespace GhostDisk.DAL
{
    class SQLiteHelper
    {
        const string DBName = "GhostDiskDB.db";
        private SQLiteConnection cnx = null;

        public SQLiteHelper()
        {
            // test si la bdd existe sinon la crée
            bool exist = File.Exists(DBName);
            if (!exist)
                CreateDatabase();
            
            try
            {
                exist = CreateDatabase();
            }
            catch (Exception)
            {
                // const error
            }

            try
            {
                cnx = new SQLiteConnection("Data Source=" + DBName + ";Version=3;FailIfMissing=True;");
                cnx.Open();
            }
            catch (Exception)
            {
                //const erreur
                MessageBox.Show("erreur cnx");
            }
            
            if (!exist)
                InitializeDatabase();
        }

        private bool CreateDatabase()
        {
            try
            {
                SQLiteConnection.CreateFile(DBName);
            }
            catch (Exception)
            {
                MessageBox.Show(Constantes.DB_CREATION_ERROR_MSG, Constantes.DB_CREATION_ERROR_TITLE, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
                return false;
            }
            
            return true;
        }

        private bool InitializeDatabase()
        {
            try
            {
                executeQuery("CREATE TABLE sizeUnits (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, name TEXT NOT NULL)");
                
                executeQuery("CREATE TABLE sauvegardes (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, name VARCHAR NOT NULL UNIQUE, date VARCHAR NOT NULL)"); // varchar transformé en TEXT
                executeQuery("CREATE TABLE options (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, backupTxtFiles INTEGER NOT NULL, maxSizeOfBackupTxtFiles INTEGER NOT NULL," +
                             " lastSourceFolderPath TEXT, lastBackupFolderPath TEXT, sizeUnit_id INTEGER NOT NULL, FOREIGN KEY(sizeUnit_id) REFERENCES sizeUnits(id))");

                executeQuery("CREATE TABLE extensionsToSave (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, extension TEXT NOT NULL, options_id, FOREIGN KEY(options_id) REFERENCES options(id))");

                executeQuery("INSERT into sizeUnits (name) values ('Ko')");
                executeQuery("insert into sizeUnits (name) values ('Mo')");
                executeQuery("insert into sizeUnits (name) values ('Go')");

                executeQuery("insert into options (backupTxtFiles, maxSizeOfBackupTxtFiles, sizeUnit_id) values (1, 10, 1)");
                executeQuery("insert into extensionsToSave (extension, options_id) values ('.txt', 1)");
            }
            catch (Exception)
            {
                MessageBox.Show("erreur init");
                return false;
            }

            return true;
        }

        private bool executeQuery(string query)
        {
            try
            {
                new SQLiteCommand(query, cnx).ExecuteNonQuery(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur requête");
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }

        private SQLiteDataReader executeUniqueRequestQuery(string query)
        {
            try
            {
                SQLiteCommand commande = new SQLiteCommand(query, this.cnx); //!attention fermeture intempestive cnx
                SQLiteDataReader reader = commande.ExecuteReader();
                reader.Read();

                return reader;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur requête****************");
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        private SQLiteDataReader/*List<string>*/ executeRequestQuery(string query)
        {
            SQLiteDataReader reader = null;

            try
            {
                SQLiteCommand commande = new SQLiteCommand(query, this.cnx); //!attention fermeture intempestive cnx
                reader = commande.ExecuteReader();

                /*while (reader.Read())
                {
                    //
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur requête");
                MessageBox.Show(ex.Message);
                return null;
            }

            return reader;
        }

        // Requêtes
        public bool setBackupTxtFiles(int profile, bool enable)
        {
            int sqliteEnable = enable ? 1 : 0;

            return executeQuery("UPDATE options SET backupTxtFiles=" + sqliteEnable + " WHERE id=" + profile);
        }

        public bool setBackupTxtFilesMaxSize(int profile, int maxSizeOfBackupTxtFiles)
        {
            return executeQuery("UPDATE options SET maxSizeOfBackupTxtFiles=" + maxSizeOfBackupTxtFiles + " WHERE id=" + profile);
        }

        public bool setBackupTxtFilesMaxSizeUnit(int profile, int BackupTxtFilesMaxSizeUnit)
        {
            return executeQuery("UPDATE options SET sizeUnit_id=" + BackupTxtFilesMaxSizeUnit + " WHERE id=" + profile);
        }

        public bool setBackupExtensions(int profile, string extension)
        {
            //return executeQuery("INSERT into extensionsToSave (extension, options_id) values ('" + extension + "', " + profile + ")");
            return executeQuery(String.Format("INSERT into extensionsToSave (extension, options_id) values ('{0}', '{1}')", extension, profile));
        }
        public bool deleteBackupExtensions(int profile, string extension)
        {
            return executeQuery(String.Format("DELETE FROM extensionsToSave WHERE options_id='{0}' AND extension='{1}'", profile, extension));
        }

        public bool saveLastSourceFolder(int profile, string sourcePath)
        {
            return executeQuery(String.Format("UPDATE options SET lastSourceFolderPath='{0}' WHERE id='{1}'", sourcePath, profile));
        }
        public bool saveLastBackupFolder(int profile, string backupPath)
        {
            return executeQuery(String.Format("UPDATE options SET lastBackupFolderPath='{0}' WHERE id='{1}'", backupPath, profile));
        }

        public Options getOptions(int profile)
        {
            //SQLiteDataReader reader = executeUniqueRequestQuery("SELECT o.id as optionId, o.backupTxtFiles, o.maxSizeOfBackupTxtFiles, su.id as sizeUnitIndex, ext.extension FROM options o JOIN sizeUnits su ON o.sizeUnit_id = su.id JOIN extensionsToSave ext ON o.id = ext.options_id WHERE o.id=" + profile);
            SQLiteDataReader reader = executeUniqueRequestQuery("SELECT o.id as optionId, o.backupTxtFiles, o.maxSizeOfBackupTxtFiles, o.lastSourceFolderPath, o.lastBackupFolderPath, su.id as sizeUnitIndex FROM options o JOIN sizeUnits su ON o.sizeUnit_id = su.id WHERE o.id=" + profile);

            SQLiteDataReader readerExtensions = executeRequestQuery/*executeUniqueRequestQuery*/("SELECT extension FROM extensionsToSave ext JOIN options o ON ext.options_id = o.id WHERE o.id=" + profile);

            List<string> extensions = new List<string>(); //add count to request to use tableau

            while (readerExtensions.Read())
            {
                extensions.Add(readerExtensions["extension"].ToString());
            }

            return ObjectsFactory.newOptions(reader, extensions);
        }

        ~SQLiteHelper()
        {
            //cnx.Close();
        }

    }
}
