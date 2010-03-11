using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    public class SUInstaller {

        private static Dictionary<string, SUInstaller> installerCache;

        public static SUInstaller InstallerForFile(string path) {

            if (installerCache != null && installerCache.ContainsKey(Path.GetExtension(path))) {
                return (SUInstaller)installerCache.ValueForKey(Path.GetExtension(path));
            }
            return null;
        }

        public static void AddInstallerForFileType(SUInstaller installer, string fileType) {

            if (installerCache == null) {
                installerCache = new Dictionary<string,SUInstaller>();
            }
            installerCache.SetValueForKey(installer, fileType);
        }

        public virtual bool BeginInstallationOfItemFromPath(SUAppcastItem item, string path) {
            return false;
        }


    }
}
