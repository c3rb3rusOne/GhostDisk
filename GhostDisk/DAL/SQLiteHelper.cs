using System;
//using System.Linq;
//using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using GhostDisk.BO;

// for messageBox

namespace GhostDisk.DAL
{
    class SQLiteHelper
    {
        const string DBName = "GhostDiskDB.db";
        private SQLiteConnection cnx = null;

        /*protected SQLiteHelper()
        {
            connexion();
        }*/

        protected Constantes.erreurCNX connexion()
        {
            // test si la bdd existe sinon la crée
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
            catch (Exception) {}
            
            return true;
        }

        protected bool executeQuery(string query)
        {
            try
            {
                new SQLiteCommand(query, cnx).ExecuteNonQuery(); 
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Erreur requête");
                //MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }

        protected SQLiteDataReader executeUniqueRequestQuery(string query)
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
                //MessageBox.Show("Erreur requête****************");
                //MessageBox.Show(ex.Message);
            }

            return null;
        }

        protected SQLiteDataReader/*List<string>*/ executeRequestQuery(string query)
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
                //MessageBox.Show("Erreur requête");
                //MessageBox.Show(ex.Message);
                return null;
            }

            return reader;
        }
    }
}
