using System;
//using System.Linq;
//using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using GhostDisk.BO;

namespace GhostDisk.DAL
{
    class SQLiteHelper
    {
        const string DBName = Constantes.DBName; // Copie locale plus rapide à atteindre
        private SQLiteConnection cnx = null;

        // Plus utile car classe dérivé + singleton
        /*protected SQLiteHelper()
        {
            connexion();
        }*/

        protected Constantes.erreurCNX connexion()
        {
            bool exist = File.Exists(DBName);

            // Test de l'existance du fichier de bdd et tentative de création de celui-ci le cas échéant
            if (!exist)
            {
                bool created = CreateDatabase();
                if (!created)
                    return Constantes.erreurCNX.InexistantDB;
            }
            
            // Ouverture de la connexion
            try
            {
                cnx = new SQLiteConnection("Data Source=" + DBName + ";Version=3;FailIfMissing=True;");
                cnx.Open();
            }
            catch (Exception)
            {
                return Constantes.erreurCNX.ConnexionError;
            }

            if (!exist)
                return Constantes.erreurCNX.NonInitailized;

            return Constantes.erreurCNX.NoError;
        }

        protected bool CreateDatabase()
        {
            try
            {
                SQLiteConnection.CreateFile(DBName);
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        protected bool executeQuery(string query)
        {
            try
            {
               var x = new SQLiteCommand(query, cnx).ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (Constantes.DEBUG)
                {
                    System.Windows.MessageBox.Show("Erreur requête****************");
                    System.Windows.MessageBox.Show(ex.Message);
                }

                return false;
            }

            return true;
        }

        // Exécute une insertion et retourne l'Id de l'entrée insérée ou -1
        protected long executeInsert(string query) //genericité ?
        {
            //SQLiteDatabase SQLiteDatabasexxx = new SQLiteDatabase();
            //long insertedID = SQLiteDatabasexxx.insert(); // -> sql server

            try
            {
                // Début de la définition de la transaction
                /*SQLiteTransaction transaction = cnx.BeginTransaction(); // Tous les changement ds la bdd sont appliqué ou aucuns (rollback)
                SQLiteCommand SQLiteCommand = cnx.CreateCommand(); //using
                SQLiteCommand.Transaction = transaction;
                SQLiteCommand.CommandText = query;
                SQLiteCommand.ExecuteNonQuery();
                id = lastInsertId(); // Exécuté avant la transaction
                // Exécution de la transaction
                transaction.Commit(); // Rollback()*/
                
                if (!(new SQLiteCommand(query, cnx).ExecuteNonQuery() > 0)) //.ExecuteScalar() .Transaction
                    return -1;
            }
            catch (Exception ex)
            {
                if (Constantes.DEBUG)
                {
                    System.Windows.MessageBox.Show("Erreur requête****************");
                    System.Windows.MessageBox.Show(ex.Message);
                }

                return -1;
            }

            return lastInsertId();
        }

        protected SQLiteDataReader executeUniqueRequestQuery(string query)
        {
            SQLiteDataReader reader = null;

            try
            {
                SQLiteCommand commande = new SQLiteCommand(query, this.cnx); //!attention fermeture intempestive cnx
                reader = commande.ExecuteReader();
                reader.Read();
            }
            catch (Exception ex)
            {
                if (Constantes.DEBUG)
                {
                    System.Windows.MessageBox.Show("Erreur requête****************");
                    System.Windows.MessageBox.Show(ex.Message);

                    return null;
                }
            }

            return reader;
        }

        protected SQLiteDataReader/*List<string>*/ executeRequestQuery(string query)
        {
            SQLiteDataReader reader = null;

            try
            {
                SQLiteCommand commande = new SQLiteCommand(query, this.cnx); //!attention fermeture intempestive cnx
                reader = commande.ExecuteReader();

                /*while (reader.Read()) { }*/
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erreur requête****************");
                System.Windows.MessageBox.Show(ex.Message);

                return null;
            }

            return reader;
        }

        protected long lastInsertId()
        {
            return cnx.LastInsertRowId;
        }
    }
}
