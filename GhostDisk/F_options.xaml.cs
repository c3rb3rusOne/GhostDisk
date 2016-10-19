using System;
using System.Collections.ObjectModel;
//using System.Drawing;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
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
        // profil actif, détermine les options
        private Profile selectedProfile = null;

        // Options de l'utilisateur en cours
        private Options backupOptions = null;

        // Permet les appels à la bdd
        private readonly Requests request = null;

        // FTO
        //private List<Profile> profiles = null;
        private readonly ObservableCollection<Profile> profiles = null;

        public Window1(Profile selectedProfile)
        {
            InitializeComponent();
            //this.DataContext = this;

            this.selectedProfile = selectedProfile;
            request = Requests.RequestsInstance;

            /* Indique l'utilisations des champs "name" et "id" pour respectivement la string 
            d'affichage des items dans la listbox et leur valeur (Maintenant en XAML)*/
            //LB_profils.DisplayMemberPath = "name";
            //LB_profils.SelectedValuePath = "id"; //! id=0 avec constructeur 1 de Profile !

            // Charger la liste de tous les profils de sauvegarde
            //LB_profils.ItemsSource = request.getAllProfiles();

            profiles = request.getAllProfiles2();
            LB_profils.ItemsSource = profiles;
            LB_profils.DataContext = profiles;

            // Sélection du profil dans la listbox
            //this.SelectByValue(selectedProfile.id);
            LB_profils.SelectByValue(selectedProfile.id);
            LB_profils.Focus();

            // Charger les options associé au profil sélectionné
            LoadOptions();
        }

        // Charger les options associé au profil sélectionné
        private bool LoadOptions()
        {
            // Charger les valeur otpion bdd verif loaded avant utiliser option
            this.backupOptions = request.getOptions(selectedProfile.id);

            if (backupOptions.backupSomeFiles)
                CB_backupSomeFiles.IsChecked = true;

            TB_maxTxtSize.Text = backupOptions.FileToBackupMaxSize.ToString();
            CBX_sizeUnits.SelectedIndex = backupOptions.sizeUnitIndex - 1;

            // Rempli la liste des extensions à sauvegarder à partir de la liste sauvegardé en bdd (contenue dans l'objet backupOptions)
            TB_extensionsToSave.Clear();
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
                default:
                    RB_inclusion.IsChecked = true;
                    break;
            }

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

            return true;
        }

        // Modifie l'état de l'option sauvegarde des fichiers en bdd (champ backupFiles) //!
        private void CB_backupSomeFiles_Click(object sender, RoutedEventArgs e)
        {
            // Si ORM -> modif objet options et persister
            backupOptions.backupSomeFiles = CB_backupSomeFiles.IsChecked ?? false;

            // try catch seulement néccéssaire si l'on remonte l'erreur (throw) pour l'afficher à l'utilisateur final (erreur niveau2)
            try
            {
                request.saveBackupFilesOptionState(selectedProfile.id, backupOptions.backupSomeFiles);
            }
            catch (Exception)
            {
                // const error second niveau (utilisateur final)
            }
        }

        // Modifie la valeur de la taille maximum des fichiers à sauvegarder en bdd (champ BackupFilesMaxSize)
        private void TB_maxTxtSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsLoaded) // Sinon pb lors du chargement initial de la valeur
                return;

            int maxTxtSize = 0;

            try
            {
                maxTxtSize = int.Parse(TB_maxTxtSize.Text);
            }
            catch (Exception)
            {
                // const error 2dn niveau
            }

            int maxFileSize = maxTxtSize != 0 ? maxTxtSize : Constantes.defaultFileToSaveMaxSize;

            backupOptions.FileToBackupMaxSize = maxFileSize;

            try
            {
                request.saveBackupFilesMaxSize(selectedProfile.id, maxFileSize); // try ou test retour*/
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
                // true si le caractère n'est pas un decimal digit, sinon false
                e.Handled = true;
        }

        // Modifie la valeur de l'unité de la taille maximum des fichiers à sauvegarder en bdd (champ BackupFilesMaxSizeUnits)
        private void CBX_sizeUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) // Sinon pb lors du chargement initial de la valeur
                return;

            int indexUnitSelected = CBX_sizeUnits.SelectedIndex + 1; // +1 -> Différence entre l'index de la bdd (débute à 1) et l'index du comboBox (débute à 0)

            backupOptions.sizeUnitIndex = indexUnitSelected;
            request.saveBackupFilesMaxSizeUnit(selectedProfile.id, indexUnitSelected);
        }

        //! 2 Méthodes de gestions de la persistance des extensions en test ici 

        // Méthode 1: suppression des extensions qui ne sont plus présente ds la texbox puis insertion des nouvelles
        // Enregistre la liste des extensions dont les fichiers sont à sauvegarder intégralement en bdd
        private void TB_extensionsToSave_LostFocus(object sender, RoutedEventArgs e) //ou btn sauvegarder
        {
            /* //! Ajouter test validité extension: vérif nb max d'extensions, longueur max de chacunes extension,
               qu'elles débutent bien par '.' + mini 1 caract, et ne contiennent pas d'espace (les supprimer le cas échéant ? etc...
               ? Utiliser une régex ?
            */
            // stringbuiler extensionLocal = extension.Trim(); // déjà split
            // if (IsNullOrWhiteSpace(TB_extensionsToSave.Text)) inutile

            if (!this.IsInitialized) // Sinon pb lors du chargement initial de la valeur
                return;
            
            string[] modifiedExtensionToSave = null;

            modifiedExtensionToSave = TB_extensionsToSave.Text.Split(' ');

            // Si pas de modifications de la liste des extensions, sortir
            if (modifiedExtensionToSave.Equals(backupOptions.extensionsToBackup))
                return;

            #region Supprimer les extensions qui existaient mais ont été supprimées dans l'interface

            /* 
             * Pour chaques extensions contenues dans les options chargés depuis la bdd la supprimer si elle n'est pas
             * contenue dans les extensions provenant de la gui
             */

            #endregion
            foreach (string extension in backupOptions.extensionsToBackup)
            {
                // Si l'extension provenant de la liste des extesions enregistrées en bdd n'est pas contenue dans la liste de celles provenant de la gui (textBox)
                if (!modifiedExtensionToSave.Contains(extension)) // déporter ds fonction insertion
                {
                    // La supprimer en bdd
                    request.deleteBackupExtensions(selectedProfile.id, extension);
                }
            }

            #region Insérer seulement les extensions non encore présentes dans la bdd - sortir method ?

            /* 
             * Pour chaques extensions contenues dans les extensions provenant de la gui l'insérer en bdd si elle n'est pas
             * contenue dans les options chargés depuis la bdd
             */

            #endregion

            // Pour chaque extension provenant de la gui (textBox)
            foreach (string extension in modifiedExtensionToSave)
            {
                //Vérifier son intégrité
                if (extension.StartsWith(".") && extension.Length > 1 && !backupOptions.extensionsToBackup.Contains(extension))
                {
                    // Et l'insérer en bdd
                    request.saveBackupExtensions(selectedProfile.id, extension.ToLower());
                    //backupOptions.extensionsToBackup -> utiliser une LIST au lieu d'un array pour pouvoir modifier ou faire copie nouveau tableau taille modifiedExtensionToSave.lenght
                }
            }

            backupOptions.extensionsToBackup = modifiedExtensionToSave;

            // Ou plus simplement suppr toutes les extensions et toutes les réinsérer !
        }

        // Méthode 2: suppression de toutes les extensions en bdd puis insertion des nouvelles
        // Enregistre la liste des extensions dont les fichiers sont à sauvegarder intégralement en bdd - Comparer perf avec TB_extensionsToSave_LostFocus
        private void TB_extensionsToExclude_LostFocus(object sender, RoutedEventArgs e)
        {
            /* //! Ajouter test validité extension: vérif nb max d'extensions, longueur max de chacunes extension,
               qu'elles débutent bien par '.' + mini 1 caract, et ne contiennent pas d'espace (les supprimer le cas échéant ? etc...
               ? Utiliser une régex ?
            */
            // stringbuiler extensionLocal = extension.Trim(); // déjà split
            // if (IsNullOrWhiteSpace(TB_extensionsToSave.Text)) inutile

            if (!this.IsInitialized) // Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> problème à la fermeture de la fenêtre
                return;

            string[] modifiedExtensionToFilter = null;
            
            modifiedExtensionToFilter = TB_extensionsToFilter.Text.Split(' ');

            // Si pas de modifications de la liste des extensions, sortir
            if (modifiedExtensionToFilter.Equals(backupOptions.extensionsToFilter))
                return;

            // Supprimer toutes les extensions existantes
            request.deleteAllExtensionsToFilter();

            // Insérer toues les nouvelles extensions
            foreach (string extension in modifiedExtensionToFilter)
            {
                if (extension.StartsWith(".") && extension.Length > 1)
                    request.saveExtensionsToFilter(selectedProfile.id, extension.ToLower());
                    //replace update au lieu d'inserer
            }

            backupOptions.extensionsToFilter = modifiedExtensionToFilter;
        }

        // Modifie l'état de l'option de filtre des extension en bdd (champ includeExcludeFilter) //!
        private void CB_useIncludeExcludeFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) //  Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> problème à la fermeture de la fenêtre
                return;

            backupOptions.UseIncludeExcludeFilter = true;
            request.saveIncludeExcludeFilterOptionState(selectedProfile.id, true);

            RB_inclusion.IsEnabled = true;
            RB_exclusion.IsEnabled = true;
        }

        // Modifie l'état de l'option de filtre des extension en bdd (champ includeExcludeFilter) //!
        private void CB_useIncludeExcludeFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) // Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> problème à la fermeture de la fenêtre
                return;

            backupOptions.UseIncludeExcludeFilter = false;
            request.saveIncludeExcludeFilterOptionState(selectedProfile.id, false);

            RB_inclusion.IsEnabled = false;
            RB_exclusion.IsEnabled = false;
        }

        // Enregistre en bdd la sélection du filtre d'inclusion exclusive
        private void RB_inclusion_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) // Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> problème à la fermeture de la fenêtre
                return;

            backupOptions.includeExcludeFilterType = 1;
            request.saveIncludeExcludeFilterType(selectedProfile.id, 1);
        }

        // Enregistre en bdd la sélection du filtre d'exclusion
        private void RB_exclusion_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) // Sinon pb lors du chargement initial de la valeur - !this.IsLoaded -> problème à la fermeture de la fenêtre
                return;

            backupOptions.includeExcludeFilterType = 2;
            request.saveIncludeExcludeFilterType(selectedProfile.id, 2);
        }

        // Persistance de l'objet option; à appeler lorsque la fenêtre se ferme ou par bouton; EntityFramework; ORM
        private void F_options_Unloaded(object sender, RoutedEventArgs e)
        {
            //request.persistOptions(backupOptions, 1);
        }

        private void LB_profils_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ne s'éxécute que si un profil est séléctionné
            if (LB_profils.SelectedIndex == -1)
                return;

            //MessageBox.Show(LB_profils.SelectedItem.ToString());
            //MessageBox.Show(LB_profils.SelectedIndex.ToString());

            this.selectedProfile = (Profile) LB_profils.SelectedItem;

            // Changer les options, Charger les valeur otpiond bdd verif loaded avant utiliser option
            //this.backupOptions = request.getOptions(selectedProfile.id);
            LoadOptions();

            // Sauvegarder l'id du dernier profil actif
            request.saveLastActiveProfile(selectedProfile.id);
        }

        private void AddNewProfile_Click(object sender, RoutedEventArgs e)
        {
            Profile newProfile = new Profile(null);
            // Empêcher 2 mêmes nom de profil

            try
            {
                F_newProfile F_newProfile = new F_newProfile(ref newProfile);
                // Don't forget to set the Owner property on the dialog window
                F_newProfile.Owner = this;
                bool? dialogResult = F_newProfile.ShowDialog(); // bool? -> Nullable<bool>

                //F_newProfile.updateProgressbar += new EventHandler<paramsEvent>(etapeEventHandler_changementEtape);

                newProfile.id = (int) request.saveNewProfile(newProfile); // ! In very large Db: long to int

                // L'ajouter à la liste
                //LB_profils.Items.SourceCollection
                this.profiles.Add(newProfile);

                // Ou Recharger toute la liste (ou utiliser listBoxItemsSource global)
                //LB_profils.ItemsSource = request.getAllProfiles(); // Ok

                // Le sélectionner
                // En sélectionnant le dernier index
                //LB_profils.UnselectAll();
                LB_profils.SelectedIndex = LB_profils.Items.Count - 1;
                // Ou par la method SelectByValue
                //this.SelectByValue(newProfile.id);
                LB_profils.Focus();
                //LB_profils.Items.MoveCurrentToFirst();

                selectedProfile = newProfile;

                // Sauvegarder id du dernier profil actif
                request.saveLastActiveProfile(newProfile.id);

                // Les options serons changées grâce à LB_profils_SelectionChanged
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            // Si l'id est présent dans la listBox
            //if (int = FindByValue(LB_profils))

            //Profile profileToDelete = (Profile) LB_profils.SelectedItem;
            //MessageBox.Show(profileToDelete.id.ToString()); // id du profil
            //MessageBox.Show(LB_profils.SelectedValue.ToString()); // id du profil

            // Le supprimer puis en sélectionner un autre (le premeier ?)
            try
            {
                // Suppression du profil de la bdd
                request.deleteProfile((int) LB_profils.SelectedValue);
                
                // Recharger les profils
                //LB_profils.ItemsSource = request.getAllProfiles();
                this.profiles.Remove(LB_profils.SelectedItem as Profile);
                //LB_profils.ItemsSource = profiles;
                
                // sélectionner le premier profil de la liste
                LB_profils.SelectedIndex = 0;
                LB_profils.Focus();

                // Sauvegarder id du dernier profil actif
                Profile FirstItemAvailable = (Profile)LB_profils.Items[0];
                request.saveLastActiveProfile(FirstItemAvailable.id);

                // Les options serons changées grâce à LB_profils_SelectionChanged
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        /*private void B_up_Click(object sender, RoutedEventArgs e)
        {
            //LB_profils.SelectedIndex -= 1;
            if (LB_profils.SelectedIndex < LB_profils.Items.Count - 1)
            {
                int vIndex = LB_profils.SelectedIndex;
                object vTemp = LB_profils.SelectedItem;

                profiles.RemoveAt(vIndex);
                profiles.Insert(vIndex + 1, vTemp as Profile);
                LB_profils.SelectedIndex = vIndex + 1;
            }
        }*/
    }
}