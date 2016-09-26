using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        Options backupOptions = null;
        private readonly Requests request = null;

        public Window1()
        {
            InitializeComponent();

            request = Requests.RequestsInstance;

            //charger les valeur otpion bdd verif loaded avant utiliser option
            this.backupOptions = request.getOptions(1);

            if (backupOptions.backupSomeFiles)
                CB_backupSomeFiles.IsChecked = true;

            TB_maxTxtSize.Text = backupOptions.tailleMaxFichierTxt.ToString();
            CBX_sizeUnits.SelectedIndex = backupOptions.sizeUnitIndex - 1;
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
            TB_extensionsToSave.Text = "";
            foreach (string extension in backupOptions.extensionsToSave)
            {
                TB_extensionsToSave.Text += extension + " ";
            }
        }

        // Modifie l'état du champ backupTxtFiles en bdd 
        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            // Si ORM -> modif objet options et persister
            if (CB_backupSomeFiles.IsChecked.Value)
                backupOptions.backupSomeFiles = true;
            else
                backupOptions.backupSomeFiles = false;

            try
            {
                // 1 = profile 1 const error, erreurs bdd seulement constante debug
                request.setBackupTxtFiles(1, backupOptions.backupSomeFiles);
            }
            catch (Exception)
            {
                // const error second niveau (utilisateur final)
                //throw;
            }
        }

        // Modifie la valeur du champ BackupTxtFilesMaxSize en bdd
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

            int maxFileSize = maxTxtSize != 0 ? maxTxtSize : 10; //const

            try
            {
                request.setBackupTxtFilesMaxSize(1, maxFileSize); // 1 = profile 1 try ou test retour*/
            }
            catch (Exception)
            {
                // const error 2nd niveau
                //throw;
            }
        }

        // Empêche de saisir autre chose que des chiffres
        private void TB_maxTxtSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1)) // -1 -> true if the character at position index in s is a decimal digit; otherwise, false.
                e.Handled = true;
        }

        private void CBX_sizeUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //macro MessageBox.Show();
            if (!this.IsLoaded) // sinon pb lors du chargement initial de la valeur
                return;

            int indexUnitselected = CBX_sizeUnits.SelectedIndex + 1; // Différence index bdd et comboBox

            request.setBackupTxtFilesMaxSizeUnit(1, indexUnitselected); // 1 = profile 1*/
        }

        private void TB_extensionsToSave_TextChanged(object sender, TextChangedEventArgs e) //not text_changed (sinon a chaque lettr)...
        {

        }

        private void TB_extensionsToSave_LostFocus(object sender, RoutedEventArgs e) //ou btn sauvegarder
        {
            if (!this.IsLoaded) // sinon pb lors du chargement initial de la valeur
                return;

            /*if (IsNullOrWhiteSpace(TB_extensionsToSave.Text))
                return;*/

            string[] modifiedExtensionToSave = null;

            //vérif nb max extension et longueur max extension regex, debute par '.', ne contient pas d'espace ...
            // ATTENTION aux espaces

            modifiedExtensionToSave = TB_extensionsToSave.Text.Split(' ');

            if (modifiedExtensionToSave.Equals(backupOptions.extensionsToSave))
                return;

            // Supprimer les extensions qui existaient mais ont été supprimées dans l'interface
            foreach (string extension in backupOptions.extensionsToSave)
            {
                // Supprimer espaces + vérifier que l'extension commence par '.' + mini 1 caract
                //stringbuiler extensionLocal = extension.Trim(); // déjà split

                if (!modifiedExtensionToSave.Contains(extension)) //deporter ds fonction insertion
                {
                    if (request.deleteBackupExtensions(1, extension))
                        ; // cons error
                }
            }

            // Faire la différence entre chaine reçue du TB et contenu bdd pour insérer seulement les nvlles extensions  - sortire method
            foreach (string extension in modifiedExtensionToSave)
            {
                // Si l'extension n'existe pas encore en bdd l'ajouter
                if (extension.StartsWith(".") && extension.Length > 1 && !backupOptions.extensionsToSave.Contains(extension))
                    request.setBackupExtensions(1, extension);
            }
            
            // Ou plus simplement suppr toutes les extensions et toutes les réinsérer !
        }
    }
}
