using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SparkleDotNET {
    class SUExecutableInstaller : SUInstaller {

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

                path = Path.Combine(path, FirstExecutableNameInDirectory(path));
            }

            if (!File.Exists(path)) {
                return false;
            }

            string switches = "";

            if (!String.IsNullOrWhiteSpace(item.ExecutableType)) {

                if (item.ExecutableType.Equals("InstallShieldSetup")) {
                    switches = "/S /v/qb";
                } else if (item.ExecutableType.Equals("InnoSetup")) {
                    switches = "/SILENT /SP-";
                }
            }

            Process installerProcess = new Process();
            installerProcess.StartInfo.FileName = path;
            installerProcess.StartInfo.Arguments = switches;

            try {
                return installerProcess.Start();
            } catch {
                return false;
            }
        }

        private static string FirstExecutableNameInDirectory(string directoryPath) {

            string[] exeFiles = Directory.GetFiles(directoryPath, ".exe", SearchOption.AllDirectories);

            if (exeFiles != null && exeFiles.Count() > 0) {
                return exeFiles[0];
            }

            return "";
        }

    }
}
