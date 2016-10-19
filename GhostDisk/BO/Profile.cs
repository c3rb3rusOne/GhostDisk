using System.ComponentModel;

namespace GhostDisk.BO
{
    public class Profile : INotifyPropertyChanged
    {
        public int id { get; set; } // Accesseur pour LB_profils...
        public string name {get; set; }
        public int options_id = 0;
        public string lastSourceFolderPath = null;
        public string lastBackupFolderPath = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public Profile(string name)
        {
            this.id = 0;
            this.name = name;
        }

        public Profile(int id, string name, string lastSourceFolderPath, string lastBackupFolderPath, int options_id)
        {
            this.id = id;
            this.name = name;
            this.options_id = options_id;
            this.lastSourceFolderPath = lastSourceFolderPath.Length > 1 ? lastSourceFolderPath : "";
            this.lastBackupFolderPath = lastBackupFolderPath.Length > 1 ? lastBackupFolderPath : "";
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return name;
        }
    }
}