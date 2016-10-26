using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GhostDisk.BLL;
using GhostDisk.BO;
using GhostDisk.DAL;

namespace GhostDisk
{
    /// <summary>
    /// Logique d'interaction pour UC_scanInProgress.xaml
    /// </summary>
    public partial class UC_scanInProgress : UserControl
    {
        // Retourner à la fenêtre de base: Méthode 1 (contenu précédent passé en paramètre
        //private Grid previousContent; // Eviter tant que possible les var globales

        // Retourner à la fenêtre de base: Méthode 3 (Par évènement)
        //public event EventHandler<bool> scanEnding;

        // Token permettant l'arrêt d'une tâche (thread), ici utiliser pour annuler un scan
        private readonly CancellationTokenSource CancellationTokenSource;

        public UC_scanInProgress(Profile selectedProfile, string source, string destination/*, Grid previousContent*/)
        {
            InitializeComponent();
            
            this.CancellationTokenSource = new CancellationTokenSource();
            // Retourner à la fenêtre de base: Méthode 1 (contenu précédent passé en paramètre
            //this.previousContent = previousContent;
            Scan(selectedProfile, source, destination/*, previousContent*/);
        }

        private async void Scan(Profile selectedProfile, string source, string destination/*, Grid previousContent*/)
        {
            Requests request = Requests.RequestsInstance;
            Options optionsSauvegarde = request.getOptions(selectedProfile.id);
            ScanDisk scanneur = new ScanDisk(optionsSauvegarde);
            scanneur.initializeProgressbar += new EventHandler<int>(InitializeProgressBar);
            scanneur.updateProgressbar += new EventHandler<int>(UpdateProgressBar);

            await Task.Run(() =>
            {
                //try
                //{
                    scanneur.Start_ScanDisk(source, destination, this.CancellationTokenSource.Token);
                //}
                //catch (Exception exception)
                //{
                    //MessageBox.Show("Task cancelled (Vraiment !)");
                    //MessageBox.Show(exception.ToString());
                    //returnToPreviousWiew();
                //}
                
            }, this.CancellationTokenSource.Token);

            /*await Task.Run(() => scanneur.Start_ScanDisk(source, destination, this.CancellationTokenSource.Token),
                                 this.CancellationTokenSource.Token); */

            /*Task t1 = new Task(() => scanneur.Start_ScanDisk(source, destination, this.CancellationTokenSource.Token),
                                 this.CancellationTokenSource.Token); */

            this.PB_scanAdvancement.Value = this.PB_scanAdvancement.Maximum;
            MessageBox.Show("Sauvegarde Terminée !");

            returnToPreviousWiew();
        }

        public void InitializeProgressBar(object sender, int nbFilesToScan)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                PB_scanAdvancement.Maximum = nbFilesToScan;
            }
            else
            {
                PB_scanAdvancement.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => {
                        PB_scanAdvancement.Maximum = nbFilesToScan;
                    }));
            }
        }
        public void UpdateProgressBar(object sender, int increment)
        {
            // Dispatcher général
            /*Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => {
                PB_scanAdvancement.Value = 50;
            }));*/
            
            // Dispatcher du control
            if (Application.Current.Dispatcher.CheckAccess())
            {
                PB_scanAdvancement.Value += increment;
            }
            else
            {
                PB_scanAdvancement.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => {
                        PB_scanAdvancement.Value += increment;
                    }));
            }
        }

        private void B_cancelScan_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource.Cancel();
            //t1.Wait();
            //MessageBox.Show("Demande d'annulation");
        }

        private void returnToPreviousWiew()
        {
            // Retourner à la fenêtre de base: Méthode 1 (contenu précédent passé en paramètre)
            //Window.GetWindow(this).Content = previousContent;
            
            // Retourner à la fenêtre de base: Méthode 2 (Par ref aux variables de la fenêtre) Getters/setters ?
            //MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
            MainWindow mainWindow = this.Parent as MainWindow;
            if (mainWindow != null)
                mainWindow.Content = mainWindow.previousContent;
            //Window.GetWindow(this).Content = new UC_mainWindows();

            // Retourner à la fenêtre de base: Méthode 3 (Par évènement) - event fin de scan
            //this.scanEnding?.Invoke(new object(), false);
        }
    }
}
