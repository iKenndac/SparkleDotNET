using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace SparkleDotNET {
    class SUZipUnarchiver : SUUnarchiver {


        public override void Start(SUAppcastItem item, string path) {

            BackgroundWorker unzipper = new BackgroundWorker();
            unzipper.DoWork += UnzipFile;
            unzipper.RunWorkerCompleted += UnzipCompleted;
            unzipper.RunWorkerAsync(path);
        }


        private void UnzipFile(object sender, DoWorkEventArgs e) {

            // We're very lenient with exceptions here, 
            // since they automatically cancel the BackgroundWorker, 
            // and any exceptions that occur here are fatal.

            string path = (string)e.Argument;
            string extractionPath = String.Concat(path, " (Extracted)");

            Directory.CreateDirectory(extractionPath);

            ZipInputStream input = new ZipInputStream(File.OpenRead(path));

            ZipEntry currentEntry;
            while ((currentEntry = input.GetNextEntry()) != null) {

                string directoryName = Path.GetDirectoryName(currentEntry.Name);
                string fileName = Path.GetFileName(currentEntry.Name);
                
                if (!String.IsNullOrWhiteSpace(directoryName)) {
                    Directory.CreateDirectory(Path.Combine(extractionPath, directoryName));
                }

                if (!String.IsNullOrWhiteSpace(fileName)) {

                    FileStream writer = File.Create(Path.Combine(extractionPath, directoryName, fileName));

                    int readSize = 2048;
                    byte[] data = new byte[readSize];

                    while (readSize > 0) {
                        readSize = input.Read(data, 0, data.Length);
                        if (readSize > 0) {
                            writer.Write(data, 0, readSize);
                        }
                    }
                }
            }

            e.Result = extractionPath;
        }

        private void UnzipCompleted(object sender, RunWorkerCompletedEventArgs e) {

            if (Delegate != null) {
                if (e.Error != null) {
                    Delegate.UnarchiverDidFail(this);
                } else {
                    Delegate.UnarchiverDidFinish(this, (string)e.Result);
                }
            }
        }

    }
}
