using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
using KNFoundation.KNKVC;
using SparkleDotNET;

namespace SparkleDotNET {
    class SUBasicUpdateDriver : SUUpdateDriver, SUAppcastDelegate, SUUnarchiverDelegate {

        protected SUAppcastItem updateItem;
        private WebClient download;
        private string downloadPath;
        private string extractedFilePath;

        public SUBasicUpdateDriver(SUUpdater anUpdater)
            : base(anUpdater) {

        }

        public override void CheckForUpdatesAtURLWithHost(string aUrl, SUHost aHost) {
            base.CheckForUpdatesAtURLWithHost(aUrl, aHost);

            SUAppcast appcast = new SUAppcast();
            appcast.Delegate = this;
            appcast.UserAgentString = String.Format("{0}/{1} SparkleDotNET", Host.Name, Host.DisplayVersion);
            appcast.FetchAppcastFromURL(new Uri(aUrl));

        }

        private SUVersionComparison VersionComparator() {

            SUVersionComparison comparator = null;
            if (Updater.Delegate != null) {
                comparator = Updater.Delegate.VersionComparatorForUpdater(Updater);
            }
            if (comparator == null) {
                comparator = SUStandardVersionComparator.SharedComparator();
            }
            return comparator;
        }

        // ---

        public bool IsItemNewer(SUAppcastItem item) {
            return VersionComparator().CompareVersionToVersion(Host.Version, item.VersionString) < 0;
        }

        public bool HostSupportsItem(SUAppcastItem item) {
            if (String.IsNullOrEmpty(item.MinimumSystemVersion)) {
                return true;
            } else {
                return SUStandardVersionComparator.SharedComparator().CompareVersionToVersion(item.MinimumSystemVersion, Host.SystemVersionString) < 1;
            }
        }

        public bool ItemContainsSkippedVersion(SUAppcastItem item) {

            string skippedVersion = (string)Host.ObjectForUserDefaultsKey(SUConstants.SUSkippedVersionKey);
            if (!String.IsNullOrWhiteSpace(skippedVersion)) {
                return VersionComparator().CompareVersionToVersion(item.VersionString, skippedVersion) <= 0;
            } else {
                return false;
            }
        }

        public bool ItemContainsValidUpdate(SUAppcastItem item) {
            return HostSupportsItem(item) && IsItemNewer(item) && !ItemContainsSkippedVersion(item);
        }

        #region SUAppcastDelegate Members

        public void AppcastDidFinishLoading(SUAppcast anAppcast) {

            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidFinishLoadingAppcast(Updater, anAppcast);
            }

            SUAppcastItem item = null;

            if (Updater.Delegate != null) {
                item = Updater.Delegate.BestValidUpdateInAppcastForUpdater(anAppcast, Updater);
            }

            foreach (SUAppcastItem potentialItem in anAppcast.Items) {
                if (HostSupportsItem(potentialItem)) {
                    item = potentialItem;
                    break;
                }
            }

            updateItem = item;

            if (updateItem == null) {
                DidNotFindUpdate();
                return;
            }

            if (ItemContainsValidUpdate(updateItem)) {
                DidFindValidUpdate();
            } else {
                DidNotFindUpdate();
            }
        }

        public void AppCastFailedToLoadWithError(SUAppcast anAppcast, Exception anError) {
            AbortUpdateWithError(anError);
        }

        #endregion

        protected virtual void DidFindValidUpdate() {
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidFindValidUpdate(Updater, updateItem);
            }
            DownloadUpdate();
        }

        protected virtual void DidNotFindUpdate() {
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidNotFindUpdate(Updater);
            }
            AbortUpdateWithError(new Exception(SUConstants.SUNoUpdateError));
        }

        protected virtual void AbortUpdateWithError(Exception error) {

            CancelDownload(null);
            AbortUpdate();
        }
        public override void AbortUpdate() {



            base.AbortUpdate();
        }

        protected void DownloadUpdate() {

            if (download == null) {
                
                // Get the filename from the URL in the background, 
                // since we hit the http server and it could take a while

                BackgroundWorker filenameWorker = new BackgroundWorker();
                filenameWorker.DoWork += GetFileNameFromURL;
                filenameWorker.RunWorkerCompleted += FileNameWasFound;
                filenameWorker.RunWorkerAsync(updateItem.FileURL);
            }
        }

        private void FileNameWasFound(object sender, RunWorkerCompletedEventArgs e) {

            string path = Path.GetTempPath();
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            downloadPath = Path.Combine(path, (string)e.Result);

            download = new WebClient();
            download.DownloadProgressChanged += DownloadDidProgress;
            download.DownloadFileCompleted += DownloadDidComplete;
            download.DownloadFileAsync(updateItem.FileURL, downloadPath);

        }

        private void GetFileNameFromURL(object sender, DoWorkEventArgs e) {

            // First, see if there's a content-disposition header.

            /*
            * Content-Disposition: attachment; filename=genome.jpeg;
                    modification-date="Wed, 12 Feb 1997 16:29:51 -0500";
           */

            Uri url = (Uri)e.Argument;

            try {
                WebRequest WebRequestObject = HttpWebRequest.Create(url);
                WebResponse ResponseObject = WebRequestObject.GetResponse();
                ResponseObject.Close();

                foreach (string HeaderKey in ResponseObject.Headers) {

                    if (HeaderKey.Equals("content-disposition", StringComparison.OrdinalIgnoreCase)) {

                        string[] sections = ResponseObject.Headers[HeaderKey].Split(';');
                        foreach (string section in sections) {

                            string[] parts = section.Split('=');
                            if (parts.Count() == 2) {
                                e.Result = Uri.UnescapeDataString(parts[1].Trim(new char[] { ' ', '"', '\'' }));
                                return;
                            }
                        }
                    }
                }
            } catch { }

            // If not, strip out any query and return the filename

            e.Result = Path.GetFileName(url.AbsolutePath);

        }

        public void CancelDownload(SUUpdateAlert alert) {

            if (download != null) {
                download.CancelAsync();
            }

        }

        protected virtual void DownloadDidProgress(object sender, DownloadProgressChangedEventArgs e) {
            // Do nothing. Override this if you care.
        }

        protected void DownloadDidComplete(object sender, AsyncCompletedEventArgs e) {

            if (e.Cancelled || e.Error != null) {

                if (File.Exists(downloadPath)) {
                    try {
                        File.Delete(downloadPath);
                    } catch { }
                }

                if (!e.Cancelled) {
                    AbortUpdateWithError(e.Error);
                }
               
            } else {
                VerifySignature();
            }

            download = null;
        }

        protected virtual void VerifySignature() {
            // Verify in the background, since it can take a while.
            BackgroundWorker verifySignatureWorker = new BackgroundWorker();
            verifySignatureWorker.DoWork += VerifySignatureInBackground;
            verifySignatureWorker.RunWorkerCompleted += VerifySignatureCompleted;

            Dictionary<string, object> verifyArguments = new Dictionary<string, object>();
            verifyArguments.SetValueForKey(downloadPath, "SUDownloadPath");
            verifyArguments.SetValueForKey(Host.PublicDSAKey, "SUPublicDSAKey");
            verifyArguments.SetValueForKey(updateItem.DSASignature, "SUUpdateSignature");

            verifySignatureWorker.RunWorkerAsync(verifyArguments);
        }

        private void VerifySignatureInBackground(object sender, DoWorkEventArgs e) {

            // This will be called on a background thread.

            try {
                e.Result = SUDSAVerifier.ValidatePathWithEncodedDSASignatureAndPublicDSAKey(
                    (string)e.Argument.ValueForKey("SUDownloadPath"),
                    (string)e.Argument.ValueForKey("SUUpdateSignature"),
                    (string)e.Argument.ValueForKey("SUPublicDSAKey")
                );
            } catch {
                e.Result = false;
            }
        }

        protected void VerifySignatureCompleted(object sender, RunWorkerCompletedEventArgs e) {

            if ((bool)e.Result == true) {

                // Carry on!
                ExtractUpdate();

            } else {

                AbortUpdateWithError(new Exception(SUConstants.SUSignatureError));

            }

        }

        protected virtual void ExtractUpdate() {

            SUUnarchiver unarchiver = SUUnarchiver.UnarchiverForPath(downloadPath);

            if (unarchiver == null) {
                AbortUpdateWithError(new Exception(SUConstants.SUNoUnarchiverError));
            } else {

                unarchiver.Delegate = this;
                unarchiver.Start(updateItem, downloadPath);
            }
        }

        public void UnarchiverDidFinish(SUUnarchiver unarchiver, string extractedFilesPath) {
            ExtractUpdateCompleted(extractedFilesPath);
        }

        public void UnarchiverDidFail(SUUnarchiver unarchiver) {
            AbortUpdateWithError(new Exception(SUConstants.SUExtractionFailedError));
        }

        protected virtual void ExtractUpdateCompleted(string extractPath) {
            extractedFilePath = extractPath;
        }

        protected virtual void InstallUpdate() {

            if (String.IsNullOrWhiteSpace(extractedFilePath) || !(File.Exists(extractedFilePath) || Directory.Exists(extractedFilePath))) {
                AbortUpdateWithError(new Exception(SUConstants.SUInstallerFailedToLaunchError));
                return;
            }

            SUInstaller installer = null;
            string installationFilePath = extractedFilePath;

            FileAttributes attr = File.GetAttributes(extractedFilePath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                // Look for specified file if it exists

                if (updateItem.PrimaryInstallationFile != null) {

                    installationFilePath = Path.Combine(extractedFilePath, updateItem.PrimaryInstallationFile);
                    installer = SUInstaller.InstallerForFile(installationFilePath);

                } else {
                    string[] files = Directory.GetFiles(extractedFilePath);
                    if (files.Count() > 0) {

                        foreach (string file in files) {
                            string filePath = Path.Combine(extractedFilePath, file);
                            installer = SUInstaller.InstallerForFile(filePath);
                            if (installer != null) {
                                installationFilePath = filePath;
                                break;
                            }
                        }
                    }
                }
            } else if (File.Exists(extractedFilePath)) {
                installer = SUInstaller.InstallerForFile(extractedFilePath);
            }

            if (installer != null) {

                if (installer.BeginInstallationOfItemFromPath(updateItem, installationFilePath)) {
                    Host.SetObjectForUserDefaultsKey(extractedFilePath, SUConstants.SUExtractedFilesForCleanupKey);
                    RemoveDownloadedFiles();

                    System.Windows.Application.Current.Shutdown(0);


                } else {
                    AbortUpdateWithError(new Exception(SUConstants.SUInstallerFailedToLaunchError));
                }

            } else {
                AbortUpdateWithError(new Exception(SUConstants.SUNoInstallerError));
            }

        }

        protected void RemoveDownloadedFiles() {
            if (!String.IsNullOrEmpty(downloadPath) && File.Exists(downloadPath)) {
                try {
                    File.Delete(downloadPath);
                } catch {
                }
            }
        }

    }


}
