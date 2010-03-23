using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SparkleDotNET {
    class SUMSIInstaller : SUInstaller {

        public override bool BeginInstallationOfItemFromPath(SUAppcastItem item, string path) {

            if (String.IsNullOrWhiteSpace(path) || item == null || !File.Exists(path)) {
                return false;
            }

            FileAttributes attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {

                if (!String.IsNullOrWhiteSpace(item.PrimaryInstallationFile) &&
                    File.Exists(Path.Combine(path, item.PrimaryInstallationFile))) {
                    path = Path.Combine(path, item.PrimaryInstallationFile);
                }

                path = Path.Combine(path, FirstMSINameInDirectory(path));
            }

            if (!File.Exists(path)) {
                return false;
            }


            Process installerProcess = new Process();
            installerProcess.StartInfo.FileName = "msiexec.exe";
            installerProcess.StartInfo.Arguments = String.Format("/i \"{0}\" /qb", path);

            try {
                return installerProcess.Start();
            } catch {
                return false;
            }
        }

        private static string FirstMSINameInDirectory(string directoryPath) {

            string[] msiFiles = Directory.GetFiles(directoryPath, ".msi", SearchOption.AllDirectories);

            if (msiFiles != null && msiFiles.Count() > 0) {
                return msiFiles[0];
            }

            return "";
        }

    }
}
