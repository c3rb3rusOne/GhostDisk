using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using GhostDisk.BLL;
using GhostDisk.BO;
using GhostDisk.DAL;

namespace GhostDisk
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Options options = null;
        
        public MainWindow()
        {
            InitializeComponent();

            this.options = LoadOptions();
        }

        //common ? Gestion des erreurs ici ou ds la bdd ?
        private Options LoadOptions(int profile = 1)
        {
            SQLiteHelper SQLiteHelper = new SQLiteHelper();
            //test suppr bdd entre 2 appels
            //charger les valeur otpion bdd verif loaded avant utiliser option
            return SQLiteHelper.getOptions(profile);
        }

        private void B_Sauvegarder_Click(object sender, RoutedEventArgs e)
        {
            Options optionsSauvegarde = LoadOptions();
            ScanDisk scanneur = new ScanDisk(optionsSauvegarde);
            
            scanneur.Start_ScanDisk(TB_folderToScan.Text, TB_backupFolder.Text);
        }

        private void B_parcourirSource_Copy_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FBD_sourcePath = new FolderBrowserDialog();

            // Do not allow the user to create new files via the FolderBrowserDialog.
            FBD_sourcePath.ShowNewFolderButton = false;
            // Default to the My Documents folder.
            //FBD_sourcePath.RootFolder = Environment.SpecialFolder.Personal;
            FBD_sourcePath.SelectedPath = options.lastSourceFolderPath;

            DialogResult result = FBD_sourcePath.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TB_folderToScan.Text = FBD_sourcePath.SelectedPath;

                SQLiteHelper SQLiteHelper = new SQLiteHelper();
                SQLiteHelper.saveLastSourceFolder(1, FBD_sourcePath.SelectedPath);
            }
        }

        private void B_parcourirSauvegarde_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FBD_backupPath = new FolderBrowserDialog();

            // Do not allow the user to create new files via the FolderBrowserDialog.
            FBD_backupPath.ShowNewFolderButton = false;
            // Default to the My Documents folder.
            //FBD_backupPath.RootFolder = Environment.SpecialFolder.Personal;
            FBD_backupPath.SelectedPath = options.lastBackupFolderPath;

            DialogResult result = FBD_backupPath.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TB_backupFolder.Text = FBD_backupPath.SelectedPath;

                SQLiteHelper SQLiteHelper = new SQLiteHelper();
                SQLiteHelper.saveLastBackupFolder(1, FBD_backupPath.SelectedPath);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //ouvrir options
            Window1 optionsSauvegarde = new Window1();
            optionsSauvegarde.Show();
        } // reload options when close windows options
    }
}
