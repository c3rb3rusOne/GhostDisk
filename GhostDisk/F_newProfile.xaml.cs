using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GhostDisk.BO;

namespace GhostDisk
{
    //public event EventHandler<paramsEvent> updateProgressbar;
    public partial class F_newProfile : Window
    {
        private readonly Profile emptyProfile = null;
        
        public F_newProfile(ref Profile emptyProfile)
        {
            InitializeComponent();
            
            this.emptyProfile = emptyProfile;
        }

        private void B_ValiderNewProfile_Click(object sender, RoutedEventArgs e)
        {
            this.emptyProfile.name = TB_profileName.Text;

            // WinForm: this.DialogResult = System.Windows.Forms.DialogResult.OK;
            DialogResult = true; // wpf -> bool?
            Close();
        }

        private void B_annulerNewProfile_Click(object sender, RoutedEventArgs e)
        {
            // DialogResult = false; // -> valeur par défaut
            Close();
        }
    }
}
