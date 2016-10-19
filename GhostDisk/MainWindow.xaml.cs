using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

using GhostDisk.BO;
using GhostDisk.DAL;
using MessageBox = System.Windows.MessageBox;

namespace GhostDisk
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Profile selectedProfile = null;
        public Grid previousContent = null;
        
        public MainWindow()
        {
            InitializeComponent();
            LoadParameters();
            // Charger le userControl de la page principale
            //this.Content = new UC_mainPage(selectedProfile);   
        }
        
        private void LoadParameters()
        {
            Requests request = Requests.RequestsInstance;
            AppsParameters appsParameters = request.getApplicationParameters();

            this.selectedProfile = request.getProfile(appsParameters.lastActiveProfile);
        }

        private void B_Sauvegarder_Click(object sender, RoutedEventArgs e)
        {
            UC_scanInProgress scanInProgress = new UC_scanInProgress(selectedProfile, TB_folderToScan.Text, TB_backupFolder.Text/*, (Grid)this.Content*/); //ref this.previousContent);
            //method... scanInProgress.scanEnding += new EventHandler<bool>(scanEnding); 
            //this.AddVisualChild(scanInProgress); // Content = scanInProgress;
            // Sauvegarder l'ancien contenu
            this.previousContent = (Grid) this.Content;
            this.Content = scanInProgress;
            //? scanInProgress.AddToEventRoute();
            //? scanInProgress.DataContext
        }

        private void B_parcourirSource_Copy_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FBD_sourcePath = new FolderBrowserDialog(); // XAML ?          

            // Do not allow the user to create new files via the FolderBrowserDialog.
            FBD_sourcePath.ShowNewFolderButton = false;
            // Afficher le dernier dossier source utilisé
            FBD_sourcePath.SelectedPath = selectedProfile.lastSourceFolderPath;

            DialogResult result = FBD_sourcePath.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            TB_folderToScan.Text = FBD_sourcePath.SelectedPath;

            Requests.RequestsInstance.saveLastSourceFolder(selectedProfile.id, FBD_sourcePath.SelectedPath);
        }

        private void B_parcourirSauvegarde_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FBD_backupPath = new FolderBrowserDialog();

            // Do not allow the user to create new files via the FolderBrowserDialog.
            FBD_backupPath.ShowNewFolderButton = false;
            // Afficher le dernier dossier destination utilisé
            FBD_backupPath.SelectedPath = selectedProfile.lastBackupFolderPath;

            DialogResult result = FBD_backupPath.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            TB_backupFolder.Text = FBD_backupPath.SelectedPath;
            
            Requests.RequestsInstance.saveLastBackupFolder(selectedProfile.id, FBD_backupPath.SelectedPath);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Ouvrir fenêtre options
            Window1 optionsSauvegarde = new Window1(selectedProfile);
            optionsSauvegarde.ShowDialog();
            LoadParameters();
        }

        /*public void scanEnding(object sender, bool error)
        {
            this.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                                 this.Content = this.previousContent;
            }));
        }*/
    }
}
