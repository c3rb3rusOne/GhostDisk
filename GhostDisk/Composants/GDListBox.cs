using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GhostDisk.BO;

namespace GhostDisk.Composants
{
    class GDListBox : ListBox
    {
        // Retourne l'index correspondant à la valeur passée en paramètre
        public int FindByValue(int id)
        {
            //ou (Profile) LB_profils.item
            foreach (Profile item in this.Items.Cast<Profile>().Where(item => item.id.Equals(id)))
            {
                //MessageBox.Show(LB_profils.Items.IndexOf(item).ToString());
                return this.Items.IndexOf(item);
            }

            return -1;
        }

        // Sélectionne l'index correspondant à la valeur passée en paramètre
        public bool SelectByValue(int id)
        {
            foreach (Profile item in this.Items) //profiles.Items)
            {
                if (item.id.Equals(id))
                {
                    this.SelectedIndex = this.Items.IndexOf(item);

                    return true;
                }

            }

            return false;
        }
    }
}
