using System;
//using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using GhostDisk.BO;
using GhostDisk.DAL;
using MessageBox = System.Windows.MessageBox;

namespace GhostDisk
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        // Options de l'utilisateur en cours
        Options backupOptions = null;

        // Permet les appels à la bdd
        private readonly Requests request = null;

        public Window1()
        {
            InitializeComponent();

            request = Requests.RequestsInstance;

            //charger les valeur otpion bdd verif loaded avant utiliser option
            this.backupOptions = request.getOptions(1);

            if (backupOptions.backupSomeFiles)
                CB_backupSomeFiles.IsChecked = true;

            TB_maxTxtSize.Text = backupOptions.FileToBackupMaxSize.ToString();
            CBX_sizeUnits.SelectedIndex = backupOptions.sizeUnitIndex - 1;

            #region Region old method

            // ! le nom ds le comboBox et celui ds la bdd doivent être identiques
            /*foreach (var item in CBX_sizeUnits.Items)
            {
                var item2 = (System.Windows.Controls.RadioButton) item;
                if (item2.Content.ToString() == backupOptions.sizeUnit)
                    item2.IsChecked = true;
                CBX_sizeUnits.SelectedItem = item2;
            }*/

            //System.Windows.Controls.RadioButton selectedItem = (System.Windows.Controls.RadioButton) CBX_sizeUnits.SelectedItem;
            //selectedItem.IsChecked = true;

            //foreach option.extensions

            #endregion

            // Rempli la liste des extensions à sauvegarder à partir de la liste sauvegardé en bdd (contenue dans l'objet backupOptions)
            TB_extensionsToSave.Text = "";
            foreach (string extension in backupOptions.extensionsToBackup)
            {
                TB_extensionsToSave.Text += extension + " ";
            }

            // Rempli la liste des extensions à filtrer à partir de la liste sauvegardé en bdd (contenue dans l'objet backupOptions)
            TB_extensionsToFilter.Text = "";
            foreach (string extension in backupOptions.extensionsToFilter)
            {
                TB_extensionsToFilter.Text += extension + " ";
            }

            // set les options de filtre à leurs etats sauvegarder en bdd (contenus dans l'objet backupOptions)
            if (backupOptions.UseIncludeExcludeFilter)
                CB_useIncludeExcludeFilter.IsChecked = true;

            // set le ttype de filtre utilisé
            switch (backupOptions.includeExcludeFilterType)
            {
                case 1:
                    RB_inclusion.IsChecked = true;
                    break;
                case 2:
                    RB_exclusion.IsChecked = true;
                    break;
            }

        }

        // Modifie l'état de l'option sauvegarde des fichiers en bdd (champ backupFiles) //!
        private void CB_backupSomeFiles_Click(object sender, RoutedEventArgs e)
        {
            // Si ORM -> modif objet options et persister
            backupOptions.backupSomeFiles = CB_backupSomeFiles.IsChecked.Value;

            // try catch seulement néccéssaire si l'on remonte l'erreur (throw) pour l'afficher à l'utilisateur final (erreur niveau2)
            try
            {
                request.saveBackupFilesOptionState(1, backupOptions.backupSomeFiles);
            }
            catch (Exception)
            {
                // const error second niveau (utilisateur final)
            }
        }

        // Modifie la valeur de la taille maximum des fichiers à sauvegarder en bdd (champ BackupFilesMaxSize)
        private void TB_maxTxtSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsLoaded) // sinon pb lors du chargement initial de la valeur
                return;

            int maxTxtSize = 0;

            try
            {
                maxTxtSize = int.Parse(TB_maxTxtSize.Text);
            }
            catch (Exception)
            {
                // const erreor 2dn niveau
            }

            int maxFileSize = maxTxtSize != 0 ? maxTxtSize : Constantes.defaultFileToSaveMaxSize;

            backupOptions.FileToBackupMaxSize = maxFileSize;

            try
            {
                request.saveBackupFilesMaxSize(1, maxFileSize); // 1 = profile 1 try ou test retour*/
            }
            catch (Exception)
            {
                // const error 2nd niveau
                //throw;
            }
        }

        // Empêche de saisir autre chose que des chiffres dans le champ poid max des fichiers à sauvegarder
        private void TB_maxTxtSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                // true if the character at position index in s is a decimal digit; otherwise, false.
                e.Handled = true;
        }

        // Modifie la valeur de l'unité de la taille maximum des fichiers à sauvegarder en bdd (champ BackupFilesMaxSizeUnits)
        private void CBX_sizeUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) // Sinon pb lors du chargement initial de la valeur
                return;

            int indexUnitSelected = CBX_sizeUnits.SelectedIndex + 1; // +1 -> Différence index bdd et index comboBox

            backupOptions.sizeUnitIndex = indexUnitSelected;

            request.saveBackupFilesMaxSizeUnit(1, indexUnitSelected); // 1 = profile 1
        }

        // Enregistre la liste des extensions dont les fichiers sont à sauvegarder intégralement en bdd
        private void TB_extensionsToSave_LostFocus(object sender, RoutedEventArgs e) //ou btn sauvegarder
        {
            if (!this.IsInitialized) // sinon pb lors du chargement initial de la valeur
                return;

            // if (IsNullOrWhiteSpace(TB_extensionsToSave.Text)) inutile

            string[] modifiedExtensionToSave = null;

            //vérif nb max extension et longueur max extension regex, debute par '.', ne contient pas d'espace ...
            // ATTENTION aux espaces

            modifiedExtensionToSave = TB_extensionsToSave.Text.Split(' ');

            // Si pas de modifications de la liste des extensions, sortir
            if (modifiedExtensionToSave.Equals(backupOptions.extensionsToBackup))
                return;

             //! Ajouter test validité extension
             // Supprimer espaces + vérifier que l'extension commence par '.' + mini 1 caract
             //stringbuiler extensionLocal = extension.Trim(); // déjà split

            #region Supprimer les extensions qui existaient mais ont été supprimées dans l'interface
            /* 
             * Pour chaques extensions contenues dans les options chargés depuis la bdd la supprimer si elle n'est pas
             * contenue dans les extensions provenant de la gui
             */
            #endregion
            foreach (string extension in backupOptions.extensionsToBackup)
            {
                if (!modifiedExtensionToSave.Contains(extension)) //deporter ds fonction insertion
                {
                    if (!request.deleteBackupExtensions(1, extension))
                        ;
                }
            }
            
            #region Insérer seulement les extensions non encore présentes dans la bdd - sortir method ?
            /* 
             * Pour chaques extensions contenues dans les extensions provenant de la gui l'insérer en bdd si elle n'est pas
             * contenue dans les options chargés depuis la bdd
             */
            #endregion
            foreach (string extension in modifiedExtensionToSave)
            {
                if (extension.StartsWith(".") && extension.Length > 1 && !backupOptions.extensionsToBackup.Contains(extension))
                {
                    //backupOptions.extensionsToBackup -> utiliser une LIST au lieu d'un array pour pouvoir modifier ou faire copie nouveau tableau taille modifiedExtensionToSave.lenght 
                    request.saveBackupExtensions(1, extension.ToLower());
                }
            }

            backupOptions.extensionsToBackup = modifiedExtensionToSave;

            // Ou plus simplement suppr toutes les extensions et toutes les réinsérer !
        }

        // Enregistre la liste des extensions dont les fichiers sont à sauvegarder intégralement en bdd - Comparer perf avec TB_extensionsToSave_LostFocus
        private void TB_extensionsToExclude_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) //  Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> probleme à la sortie
                return;
            
            string[] modifiedExtensionToFilter = null;

            //vérif nb max extension et longueur max extension regex, debute par '.', ne contient pas d'espace ...
            // ATTENTION aux espaces

            modifiedExtensionToFilter = TB_extensionsToFilter.Text.Split(' ');

             //! Ajouter test validité extension
             // Supprimer espaces + vérifier que l'extension commence par '.' + mini 1 caract
            // stringbuiler extensionLocal = extension.Trim(); // déjà split

            // Si pas de modifications de la liste des extensions, sortir
            if (modifiedExtensionToFilter.Equals(backupOptions.extensionsToFilter))
                return;

            // Supprimer toutes les extensions existantes
            request.deleteAllExtensionsToFilter();

            // Insérer toues les nouvelles extensions
            foreach (string extension in modifiedExtensionToFilter)
            {
                if (extension.StartsWith(".") && extension.Length > 1)
                    request.saveExtensionsToFilter(1, extension.ToLower()); //replace update au lieu d'inserer
            }

            backupOptions.extensionsToFilter = modifiedExtensionToFilter;
        }

        // Modifie l'état de l'option de filtre des extension en bdd (champ includeExcludeFilter) //!
        private void CB_useIncludeExcludeFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) //  Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> probleme à la sortie
                return;

            request.saveIncludeExcludeFilterOptionState(1, true);

            RB_inclusion.IsEnabled = true;
            RB_exclusion.IsEnabled = true;
        }

        // Modifie l'état de l'option de filtre des extension en bdd (champ includeExcludeFilter) //!
        private void CB_useIncludeExcludeFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) //  Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> probleme à la sortie
                return;

            request.saveIncludeExcludeFilterOptionState(1, false);

            RB_inclusion.IsEnabled = false;
            RB_exclusion.IsEnabled = false;
        }

        // Enregistre en bdd la sélection du filtre d'inclusion exclusive
        private void RB_inclusion_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) //  Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> probleme à la sortie
                return;

            request.saveIncludeExcludeFilterType(1, 1);
        }

        // Enregistre en bdd la sélection du filtre d'exclusion
        private void RB_exclusion_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) //  Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> probleme à la sortie
                return;

            request.saveIncludeExcludeFilterType(1, 2);
        }

        // persistance de l'objet option; à appeler lorsque la fenêtre se ferme ou par bouton; EntityFramework; ORM
        //private void persistanceOptions() {}
    }
}
