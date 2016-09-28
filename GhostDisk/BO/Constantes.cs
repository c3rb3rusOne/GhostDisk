using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostDisk.BO
{
    public static class Constantes
    {
        //public enum Units : ulong { Ko = 1024, Mo = 1048576, Go = 1073741824 };
        //public readonly Dictionary<string, int> = Dictionary {{"abc",1}, {"def",2}, {"ghi",3}};
        public static readonly int[] valuesOfUnitsInKb = {1024, 1048576, 1073741824 }; //Ko, Mo, Go in Kb

        /********************************************** CONFIG ***************************************************************/
        public const bool DEBUG = true;
        public const string DBName = "GhostDiskDB.db";

        public const int defaultFileToSaveUnitIndex = 1; // 1 -> Ko
        public const int defaultFileToSaveMaxSize = 10;
        /********************************************** FIN CONFIG ***********************************************************/

        /********************************************* ERRORS ****************************************************************/

        // DB ERRORS

        // Erreurs de connexion à la bdd
        public enum erreurCNX : short { NoError = 0, InexistantDB = 1, ConnexionError = 2, NonInitailized = 3 };

        // Messages d'erreurs correspondants
        public static readonly string[] DB_CREATION_ERROR_TITLE = {"Ok", "Erreur création bdd", "Erreur de connexion", "Bdd vide"};
        public static readonly string[] DB_CREATION_ERROR_MSG = {"GhostDisk s'est correctement initialisé", "La bdd n'a pu être créée", "Erreur d'accès aux règlages", "Erreur d'initialisation des règlages" }; //base reessayer ...
        //public const string DB_CREATION_ERROR_MSG = "Emplacement innacessible ou droits d'administration insuffisants.";


        /********************************************* FIN ERRORS ************************************************************/








        /********************************************* END ERRORS ************************************************************/

    }
}
