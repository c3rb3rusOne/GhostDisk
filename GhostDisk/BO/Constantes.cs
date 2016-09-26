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
        public static readonly int[] valuesOfUnitsInKb = {1024, 1048576, 1073741824 }; //Ko, Mo, Go in Kb

        /*public readonly Dictionary<string, int> = Dictionary {
            {"abc",1}, {"def",2}, {"ghi",3}
        };*/

        /********************************************* ERRORS ****************************************************************/

        // DB ERRORS

        // Erreur à la création de la base de données.
        public const string DB_CREATION_ERROR_TITLE = "Erreur de création de la base de donnée";
        public const string DB_CREATION_ERROR_MSG = "Emplacement innacessible ou droits d'administration insuffisants.";








        /********************************************* END ERRORS ************************************************************/

    }
}
