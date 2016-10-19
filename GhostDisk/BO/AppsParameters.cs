using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostDisk.BO
{
    public class AppsParameters
    {
        public int lastActiveProfile = 0;

        public AppsParameters(int lastActiveProfile)
        {
            this.lastActiveProfile = lastActiveProfile >= 1 ? lastActiveProfile : 1;
        }
    }
}
