using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkleDotNET {
    public class SUInstaller {


        public virtual bool BeginInstallationOfItemFromPath(SUAppcastItem item, string path) {
            return false;
        }


    }
}
