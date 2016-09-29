using System;
using System.Collections.Generic;
using System.Data.SQLite;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using GhostDisk.BLL;
using GhostDisk.BO;
//using MessageBox = System.Windows.MessageBox;
//using Application = System.Windows.Application;
//using System.Windows.Forms;
//using System.Windows;

//racourcir constantes.erreurCNX
//using erreurCNX = GhostDisk.BO.Constantes;


namespace GhostDisk.DAL
{
    class Requests : SQLiteHelper
    {
        private static Requests requestsInstance;

        // Le constructeur de base de SQLHelper est appelé (avnt celui de la classe dérivé), voir aussi : base() pr choisir quel constructeur appeler
        private Requests()
        {
            Constantes.erreurCNX connexionError = connexion();

            if (connexionError == Constantes.erreurCNX.NoError)
                return;

            if (connexionError == Constantes.erreurCNX.NonInitailized)
            {
                InitializeDatabase(); // test reussite
                return;
            }

            // Si autre erreur afficher message + demander réessayer - TEST -
            MessageBoxResult result = new MessageBoxResult();
            do
            {
                result = MessageBox.Show(Constantes.DB_CREATION_ERROR_TITLE[(int)connexionError], Constantes.DB_CREATION_ERROR_MSG[(int)connexionError], MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.No)
                    return; // quitter l'appli ou utiliser les options de base
            }
            while (result == MessageBoxResult.Yes) ;
        }

        // Singleton: retourne l'instance si celle-ci existe déjà ou créer un objet Requests le cas échéant
        public static Requests RequestsInstance => requestsInstance ?? (requestsInstance = new Requests());
        /* ==
         * public static Requests RequestsInstance
           {
               get { return requestsInstance ?? (requestsInstance = new Singleton()); }
           }
        */

        // Requêtes
        private bool InitializeDatabase()
        {
            try
            {
                // Table sizeUnits -> renferme toutes les unités de taille (Ko, Mo...) prises en charge par l'application
                executeQuery("CREATE TABLE sizeUnits (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, name TEXT NOT NULL)");

                // Table sauvegardes -> renferme les données sur chaques sauvegardes - PAS ENCORE IMPLÉMENTÉ
                executeQuery("CREATE TABLE sauvegardes (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, name VARCHAR NOT NULL UNIQUE, date VARCHAR NOT NULL)"); // varchar transformé en TEXT

                // Table options -> renferme les données de configuration propre à chaque utilisateurs - PROFILS PAS ENCORE IMPLÉMENTÉS
                executeQuery("CREATE TABLE options (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, useBackupSomeFiles INTEGER NOT NULL, " +
                             "maxSizeOfBackupFiles INTEGER NOT NULL, lastSourceFolderPath TEXT, lastBackupFolderPath TEXT, " +
                             "useIncludeExcludeFilter INTEGER NOT NULL, includeExcludeFilterType INTEGER NOT NULL, sizeUnit_id INTEGER NOT NULL, " + 
                             "FOREIGN KEY(sizeUnit_id) REFERENCES sizeUnits(id))");
                
                // Table extensions -> renferme la liste des extensions dont il faut sauvegarder intégralement les fichiers
                executeQuery("CREATE TABLE extensions (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, extension TEXT NOT NULL, options_id, FOREIGN KEY(options_id) REFERENCES options(id))");
                
                // Table extensionsToFilter -> renferme la liste des extensions à inclure uniquement dans le traitement ou à exclure du traitement
                executeQuery("CREATE TABLE extensionsToFilter (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, extension TEXT NOT NULL, options_id, FOREIGN KEY(options_id) REFERENCES options(id))");

                // Remplissage de la table sizeUnits
                executeQuery("INSERT into sizeUnits (name) values ('Ko')");
                executeQuery("insert into sizeUnits (name) values ('Mo')");
                executeQuery("insert into sizeUnits (name) values ('Go')");

                // Création des options du premier profil avec les valeurs par défaut
                executeQuery("insert into options (useBackupSomeFiles, maxSizeOfBackupFiles, sizeUnit_id, useIncludeExcludeFilter, includeExcludeFilterType) values (1, 10, 1, 0, 1)");

                // Remplissage des tables renfermant les listes d'extensions sujettes à un traitement spécifique
                executeQuery("insert into extensions (extension, options_id) values ('.txt', 1)");
                //executeQuery("insert into extensionsToFilter (extension, options_id) values ('.txt', 1)");
            }
            catch (Exception) { return false; }

            return true;
        }

        // Enregistre l'état de la fonction de sauvegarde des fichiers
        public bool saveBackupFilesOptionState(int profile, bool enable)
        {
            int sqliteEnable = enable ? 1 : 0;

            return executeQuery("UPDATE options SET useBackupSomeFiles=" + sqliteEnable + " WHERE id=" + profile);
        }

        // Enregistre l'état de la fonction de filtrage des fichiers
        public bool saveIncludeExcludeFilterOptionState(int profile, bool etat)
        {
            int sqliteEtat = etat ? 1 : 0;
            return executeQuery($"UPDATE options SET useIncludeExcludeFilter='{sqliteEtat}' WHERE id='{profile}'");
        }

        // Enregistre le poid maximum des fichiers à sauvegarder
        public bool saveBackupFilesMaxSize(int profile, int maxSizeOfBackupFiles)
        {
            return executeQuery("UPDATE options SET maxSizeOfBackupFiles=" + maxSizeOfBackupFiles + " WHERE id=" + profile);
        }

        // Enregistre l'unité de poid des ficheirs à sauvegarder (Ko, ou Mo)
        public bool saveBackupFilesMaxSizeUnit(int profile, int BackupFilesMaxSizeUnit)
        {
            return executeQuery("UPDATE options SET sizeUnit_id=" + BackupFilesMaxSizeUnit + " WHERE id=" + profile);
        }

        // Enregistre une extension dans la liste des extensions dont les fichiers sont à sauvegarder intégralement
        public bool saveBackupExtensions(int profile, string extension)
        {
            //return executeQuery("INSERT into extensions (extension, options_id) values ('" + extension + "', " + profile + ")");
            return executeQuery(String.Format("INSERT into extensions (extension, options_id) values ('{0}', '{1}')", extension, profile));
        }
        // Supprime une extension dans la liste des extensions dont les fichiers sont à sauvegarder intégralement
        public bool deleteBackupExtensions(int profile, string extension)
        {
            return executeQuery(String.Format("DELETE FROM extensions WHERE options_id='{0}' AND extension='{1}'", profile, extension));
        }

        // Enregistre une extension dans la liste des extensions filtrés
        public bool saveExtensionsToFilter(int profile, string extension)
        {
            return executeQuery($"INSERT into extensionsToFilter (extension, options_id) values ('{extension}', '{profile}')");
        }
        // Supprime une extension dans la liste des extensions filtrés
        public bool deleteAllExtensionsToFilter()
        {
            return executeQuery("DELETE FROM extensionsToFilter");

            /* ou
                    executeQuery("DROP TABLE extensionsToFilter");
                    executeQuery("CREATE TABLE extensionsToFilter (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, extension TEXT NOT NULL, options_id, FOREIGN KEY(options_id) REFERENCES options(id))");
           */

        }

        // Enregistre le type de filtrage à effectuer
        public bool saveIncludeExcludeFilterType(int profile, int type)
        {
            return executeQuery($"UPDATE options SET includeExcludeFilterType='{type}' WHERE id='{profile}'");
        }

        // Enregistre le dernier dossier sélectionné pour sauvegarde
        public bool saveLastSourceFolder(int profile, string sourcePath)
        {
            return executeQuery(String.Format("UPDATE options SET lastSourceFolderPath='{0}' WHERE id='{1}'", sourcePath, profile));
        }
        // Enregistre le dernier dossier sélectionné pour accueillir la sauvegarde
        public bool saveLastBackupFolder(int profile, string backupPath)
        {
            return executeQuery(String.Format("UPDATE options SET lastBackupFolderPath='{0}' WHERE id='{1}'", backupPath, profile));
        }

        // Renvoi l'objet Options correspondant au profil utilisateur
        public Options getOptions(int profile=1)
        {
            //SQLiteDataReader reader = executeUniqueRequestQuery("SELECT o.id as optionId, o.useBackupSomeFiles, o.maxSizeOfBackupFiles, su.id as sizeUnitIndex, ext.extension FROM options o JOIN sizeUnits su ON o.sizeUnit_id = su.id JOIN extensions ext ON o.id = ext.options_id WHERE o.id=" + profile);
            SQLiteDataReader reader = executeUniqueRequestQuery("SELECT o.id as optionId, o.useBackupSomeFiles, o.maxSizeOfBackupFiles, o.lastSourceFolderPath, o.lastBackupFolderPath, "
                                                                + "o.useIncludeExcludeFilter, o.includeExcludeFilterType, su.id as sizeUnitIndex FROM options o JOIN sizeUnits su ON o.sizeUnit_id = su.id WHERE o.id=" + profile);

            SQLiteDataReader readerExtensionsToBackup = executeRequestQuery("SELECT extension FROM extensions ext JOIN options o ON ext.options_id = o.id WHERE o.id=" + profile);
            SQLiteDataReader readerExtensionsToFilter = executeRequestQuery("SELECT extension FROM extensionsToFilter ext JOIN options o ON ext.options_id = o.id WHERE o.id=" + profile);

            //List<string> extensions = new List<string>(); //add count to request to use tableau

            // Extension à sauvegarder
            List<string> extensionsToBackup = new List<string>();         
            while (readerExtensionsToBackup.Read())
            {
                //if (readerExtensionsToBackup["type"].ToString() == "0")
                extensionsToBackup.Add(readerExtensionsToBackup["extension"].ToString());
            }

            // Extension à exclure
            List<string> extensionsToFilter = new List<string>();
            while (readerExtensionsToFilter.Read())
            {
                extensionsToFilter.Add(readerExtensionsToFilter["extension"].ToString());
            }

            return ObjectsFactory.newOptions(reader, extensionsToBackup, extensionsToFilter);
        }

        // Persistance d'un objet Options (dans le cas ou toutes les options sont enregistrés à la fermeture de la fenêtre)
        public bool persistOptions(Options options, int profil=1)
        {
            int useBackupSomeFiles = options.backupSomeFiles ? 1 : 0;
            int UseIncludeExcludeFilter = options.UseIncludeExcludeFilter ? 1 : 0;

            return executeQuery($"UPDATE options SET useBackupSomeFiles='{useBackupSomeFiles}', maxSizeOfBackupFiles='{options.FileToBackupMaxSize}', " +
                                $"sizeUnit_id='{options.sizeUnitIndex}', useIncludeExcludeFilter='{UseIncludeExcludeFilter}', " +
                                $"includeExcludeFilterType='{options.includeExcludeFilterType}'");
        }
    }
}