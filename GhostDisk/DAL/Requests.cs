using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using GhostDisk.BLL;
using GhostDisk.BO;
// using Application = GhostDisk.BO.AppsParameters; // Evite un conflit avec une classe System.Windows

//using MessageBox = System.Windows.MessageBox;
//using Application = System.Windows.Application;
//using System.Windows.Forms;
//using System.Windows;

//racourcir constantes.erreurCNX
//using erreurCNX = GhostDisk.BO.Constantes;

//! Envisager de passer à l'entity framework ou de persister les données via des objets (ex option -> insert or update)

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

        // Création et remplissage des tables
        private bool InitializeDatabase()
        {
            try
            {
                #region Création des tables

                // Table sizeUnits -> renferme toutes les unités de taille (Ko, Mo...) prises en charge par l'application
                executeQuery("CREATE TABLE sizeUnits (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, name TEXT NOT NULL)");

                // Table options -> renferme les données de configuration propre à chaque utilisateurs - PROFILS PAS ENCORE IMPLÉMENTÉS
                executeQuery("CREATE TABLE options (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, useBackupSomeFiles INTEGER NOT NULL, " +
                             "useIncludeExcludeFilter INTEGER NOT NULL, maxSizeOfBackupFiles INTEGER NOT NULL, includeExcludeFilterType INTEGER NOT NULL, " +
                             "sizeUnit_id INTEGER NOT NULL, FOREIGN KEY(sizeUnit_id) REFERENCES sizeUnits(id))");

                // Table profiles -> renferme les informations sur les profiles de sauvegarde, associés à la table options.
                executeQuery("CREATE TABLE profiles (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, name TEXT NOT NULL, lastBackupFolderPath TEXT, " +
                             "lastSourceFolderPath TEXT, options_id INTEGER NOT NULL, FOREIGN KEY(options_id) REFERENCES options(id))");

                // Table application -> renferme les informations sur les paramètres de base de l'application
                executeQuery("CREATE TABLE application (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, lastActiveProfile INTEGER NOT NULL, " +
                             "FOREIGN KEY(lastActiveProfile) REFERENCES profiles(id))");
                
                // Table extensions -> renferme la liste des extensions dont il faut sauvegarder intégralement les fichiers
                executeQuery("CREATE TABLE extensions (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, extension TEXT NOT NULL, options_id, FOREIGN KEY(options_id) REFERENCES options(id))");

                // Table extensionsToFilter -> renferme la liste des extensions à inclure uniquement dans le traitement ou à exclure du traitement
                executeQuery("CREATE TABLE extensionsToFilter (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, extension TEXT NOT NULL, options_id, FOREIGN KEY(options_id) REFERENCES options(id))");
                
                // Table sauvegardes -> renferme les données sur chaques sauvegardes - PAS ENCORE IMPLÉMENTÉ
                executeQuery("CREATE TABLE sauvegardes (id INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL, name VARCHAR NOT NULL UNIQUE, date VARCHAR NOT NULL)"); // varchar transformé en TEXT

                #endregion

                #region Remplissage des tables
                // Remplissage de la table sizeUnits
                executeQuery("INSERT into sizeUnits (name) values ('Ko')");
                executeQuery("INSERT into sizeUnits (name) values ('Mo')");
                executeQuery("INSERT into sizeUnits (name) values ('Go')");

                // Création des options du premier profil avec les valeurs par défaut
                executeQuery("INSERT into options (useBackupSomeFiles, maxSizeOfBackupFiles, sizeUnit_id, useIncludeExcludeFilter, includeExcludeFilterType) values (1, 10, 1, 0, 1)");

                // Création du profil par défaut
                executeQuery("INSERT into profiles (name, options_id) values ('Profil 1 (défaut)', 1)");

                // Remplissage des tables renfermant les listes d'extensions sujettes à un traitement spécifique
                executeQuery("INSERT into extensions (extension, options_id) values ('.txt', 1)");
                //executeQuery("insert into extensionsToFilter (extension, options_id) values ('.txt', 1)");

                // Création des paramètres de l'application
                executeQuery("INSERT into application (lastActiveProfile) values (1)");

                #endregion
            }
            catch (Exception) { return false; }

            return true;
        }

        #region Application

        // Enregistre l'id du dernier profil sélectionné
        public bool saveLastActiveProfile(int idProfile)
        {
            return executeQuery(String.Format("UPDATE application SET lastActiveProfile='{0}' WHERE id=1", idProfile));
        }

        // Renvoi un objet contenant les informations de fonctionnements de l'application
        public AppsParameters getApplicationParameters()
        {
            SQLiteDataReader reader = executeUniqueRequestQuery("SELECT lastActiveProfile FROM application LIMIT 1");

            return ObjectsFactory.newAppsParameters(reader);
        }

        #endregion

        #region Filters

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

        // Enregistre le type de filtrage à effectuer
        public bool saveIncludeExcludeFilterType(int profile, int type)
        {
            return executeQuery($"UPDATE options SET includeExcludeFilterType='{type}' WHERE id='{profile}'");
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

        #endregion

        #region Derniers dossiers utilisés

        // Enregistre le dernier dossier sélectionné pour sauvegarde
        public bool saveLastSourceFolder(int profile, string sourcePath)
        {
            return executeQuery(String.Format("UPDATE profiles SET lastSourceFolderPath='{0}' WHERE id='{1}'", sourcePath, profile));
        }
        // Enregistre le dernier dossier sélectionné pour accueillir la sauvegarde
        public bool saveLastBackupFolder(int profile, string backupPath)
        {
            return executeQuery(String.Format("UPDATE profiles SET lastBackupFolderPath='{0}' WHERE id='{1}'", backupPath, profile));
        }

        #endregion

        #region Options

        // Renvoi l'objet Options correspondant au profil utilisateur
        public Options getOptions(int profile=1)
        {
            //executeUniqueRequestQuery
            SQLiteDataReader reader = executeUniqueRequestQuery("SELECT p.id as profilId, p.name, o.id as optionId, o.useBackupSomeFiles, o.useIncludeExcludeFilter, o.maxSizeOfBackupFiles, o.includeExcludeFilterType, su.id as sizeUnitIndex " +
                                                                "FROM profiles p JOIN options o ON p.options_id = o.id " +
                                                                "JOIN sizeUnits su ON o.sizeUnit_id = su.id WHERE p.id=" + profile);

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
        public bool persistOptions(Options options)
        {
            int useBackupSomeFiles = options.backupSomeFiles ? 1 : 0;
            int useIncludeExcludeFilter = options.UseIncludeExcludeFilter ? 1 : 0;

            return executeQuery($"UPDATE options SET useBackupSomeFiles='{useBackupSomeFiles}', useIncludeExcludeFilter='{useIncludeExcludeFilter}', maxSizeOfBackupFiles='{options.FileToBackupMaxSize}', " +
                                $"includeExcludeFilterType='{options.includeExcludeFilterType}', sizeUnit_id='{options.sizeUnitIndex}'");
        }

        // Insertion d'un nouvel objet Options
        public bool saveNewOptions(Options options)
        {
            int useBackupSomeFiles = options.backupSomeFiles ? 1 : 0;
            int useIncludeExcludeFilter = options.UseIncludeExcludeFilter ? 1 : 0;

            /*StringBuilder query = new StringBuilder("BEGIN TRANSACTION;");
            query.Append(" INSERT into options(useBackupSomeFiles, useIncludeExcludeFilter, maxSizeOfBackupFiles, includeExcludeFilterType, sizeUnit_id) " +
                           $"values('{useBackupSomeFiles}', '{useIncludeExcludeFilter}', '{options.FileToBackupMaxSize}', '{options.includeExcludeFilterType}', '{options.sizeUnitIndex}')");
            query.Append(" COMMIT;");
            return executeQuery(query.ToString());*/

            executeQuery("INSERT into options(useBackupSomeFiles, useIncludeExcludeFilter, maxSizeOfBackupFiles, includeExcludeFilterType, sizeUnit_id) " +
                                $"values('{useBackupSomeFiles}', '{useIncludeExcludeFilter}', '{options.FileToBackupMaxSize}', '{options.includeExcludeFilterType}', '{options.sizeUnitIndex}')");

            //long lastId = (long)command.ExecuteScalar("SELECT last_insert_rowid() as lastInsertRowId");
            // voir transaction pr etre sur qu'il s'agisse du bon id
            long optionsId = lastInsertId();

            // Ajouter table extensions... insert or update
            executeQuery($"INSERT into extensions (extension, options_id) values ('.txt', {optionsId})");
            //executeQuery("insert into extensionsToFilter (extension, options_id) values ('.txt', 1)");

            return true;

        }

        #endregion

        #region Profiles

        // Retourne un liste contenant les objets Profile relatifs à tous les profils stockés en bdd
        public List<Profile> getAllProfiles() //? dictionary
        {
            /*SQLiteDataReader reader = executeUniqueRequestQuery("SELECT p.id as profilId, p.name, o.id as optionId, o.useBackupSomeFiles, o.maxSizeOfBackupFiles, o.lastSourceFolderPath, o.lastBackupFolderPath, o.useIncludeExcludeFilter, o.includeExcludeFilterType, su.id as sizeUnitIndex " +
                                                                "FROM profiles p JOIN options o ON p.options_id = o.id " +
                                                                "JOIN sizeUnits su ON o.sizeUnit_id = su.id WHERE p.id=" + profile);
            */

            SQLiteDataReader reader = executeRequestQuery("SELECT id, name, lastSourceFolderPath, lastBackupFolderPath, options_id FROM profiles"); //list ou count for tab index

            List<Profile> profilslList = new List<Profile>(); // Quite similar perf to arrays
            while (reader.Read())
            {
                profilslList.Add(ObjectsFactory.newProfil(reader));
            }

            return profilslList;
        }

        /* Retourne une ObservableCollection contenant les objets Profile relatifs à tous les profils stockés en bdd
           Avantage: permet le databinding avec la listBox des profils (tout changement sur la collection (pas sur les propriété des objet inotify) etant automatiquement répercuter sur celle-ci)
        */
        public ObservableCollection<Profile> getAllProfiles2()
        {
            SQLiteDataReader reader = executeRequestQuery("SELECT id, name, lastSourceFolderPath, lastBackupFolderPath, options_id FROM profiles"); //list ou count for tab index

            ObservableCollection<Profile> profilsCollection = new ObservableCollection<Profile>();
            while (reader.Read())
            {
                profilsCollection.Add(ObjectsFactory.newProfil(reader));
            }

            return profilsCollection;
        }

        // Retourne l'id du nouveau profile
        public long saveNewProfile(Profile profile)
        {
            // Créer une entrée dans la table options, avec les valeurs par défaut
            saveNewOptions(new Options()); //defaut construct

            // Récupérer l'id du nouvel enregistrement dans la table options (maintenant inclu dans la requête d'insertion du profil)
            //SQLiteDataReader readerOptionsId = executeUniqueRequestQuery("SELECT last_insert_rowid();");

            // OUTPUT Inserted.ID -> avant values -> sql server
            long id = executeInsert($"INSERT into profiles (name, options_id) values ('{profile.name}', (SELECT last_insert_rowid() ) )");

            return id;
        }

        // Retourne l'objet Profile correspondant à l'id passé en paramètre
        public Profile getProfile(int profileId)
        {
            SQLiteDataReader reader = executeUniqueRequestQuery($"SELECT id, name, lastSourceFolderPath, lastBackupFolderPath, options_id FROM profiles WHERE id={profileId}"); //list ou count for tab index

            return ObjectsFactory.newProfil(reader);
        }

        // Supprime un Profil de la bdd via son id
        public bool deleteProfile(int profileId)
        {
            return executeQuery($"DELETE FROM profiles WHERE id={profileId}");
        }

        // Persistance d'un objet Profile
        /*public bool persistProfile(Profile profile, int profileId)
        {
            //insert or update -> nouvelle entré options ...
            return executeQuery($"INSERT OR REPLACE into profiles () values ");
        }*/

        #endregion
    }
}