using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUBasicUpdateDriver : SUUpdateDriver, SUAppcastDelegate {

        public const string SUNoUpdateError = "com.Sparkle.NoUpdate";
        public const string SUSignatureError = "com.Sparkle.SignatureError";
        public const string SUSkippedVersionKey = "SUSkippedVersion";

        protected SUAppcastItem updateItem;
        private WebClient download;
        private string downloadPath;

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

            string skippedVersion = (string)Host.ObjectForUserDefaultsKey(SUSkippedVersionKey);
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
            AbortUpdateWithError(new Exception(SUNoUpdateError));
        }

        protected virtual void AbortUpdateWithError(Exception error) {

        }
        public override void AbortUpdate() {
            base.AbortUpdate();
        }

        protected void DownloadUpdate() {

            if (download == null) {
                string path = Path.GetTempPath();
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                downloadPath = Path.Combine(path, updateItem.FileURL.Segments.Last());

                download = new WebClient();
                download.DownloadProgressChanged += DownloadDidProgress;
                download.DownloadFileCompleted += DownloadDidComplete;
                download.DownloadFileAsync(updateItem.FileURL, downloadPath);
            }
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

                AbortUpdateWithError(e.Error);
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

                AbortUpdateWithError(new Exception(SUSignatureError));

            }

        }

        protected virtual void ExtractUpdate() {
            ExtractUpdateCompleted();
        }

        protected virtual void ExtractUpdateCompleted() {

        }

    }


}
