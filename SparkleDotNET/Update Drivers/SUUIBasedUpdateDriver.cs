using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SparkleDotNET {
    class SUUIBasedUpdateDriver : SUBasicUpdateDriver, SUUpdateAlertDegate {

        public SUUIBasedUpdateDriver(SUUpdater anUpdater)
            : base(anUpdater) {
        }

        SUUpdateAlert alert;

        protected override void DidFindValidUpdate() {
            
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidFindValidUpdate(Updater, updateItem);
            }

            alert = SUUpdateAlert.CreateAlert(Host, updateItem);
            alert.Delegate = this;
            alert.ShowWindow();

        }

        public override void AbortUpdate() {
            if (alert != null) {
                alert.ForceClose();
            }
            base.AbortUpdate();
        }

        protected override void AbortUpdateWithError(Exception error) {

            string message = "An error occurred while updating. Please try again later.";

            //SUNoUpdateError 

            if (error.Message.Equals(SUConstants.SUNoFeedURLError)) {
                message = "An error occurred in retrieving update information. Please try again later.";
            } else if (error.Message.Equals(SUConstants.SUDownloadFailedError)) {
                message = "An error occurred while downloading the update. Please try again later.";
            } else if (error.Message.Equals(SUConstants.SUExtractionFailedError) || error.Message.Equals(SUConstants.SUSignatureError)) {
                message = "An error occurred while extracting the archive. Please try again later.";
            } else if (error.Message.Equals(SUConstants.SUInstallerFailedToLaunchError) || error.Message.Equals(SUConstants.SUNoInstallerError)) {
                message = "An error occurred while installing the update. Please try again later.";
            }

            MessageBox.Show(message, "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);

            base.AbortUpdateWithError(error);
        }

        protected override void DidNotFindUpdate() {

            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidNotFindUpdate(Updater);
            }

            MessageBox.Show("You're up to date!", "Hi", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            AbortUpdate();            

        }

        protected override void DownloadDidProgress(object sender, System.Net.DownloadProgressChangedEventArgs e) {
            if (alert != null) {
                alert.UpdateDownloadProgressWithEvent(e);
            }
        }

        protected override void VerifySignature() {
            if (alert != null) {
                alert.SwitchToIndeterminateAction("Verifying...");
            }
            base.VerifySignature();
        }

        protected override void ExtractUpdate() {
            if (alert != null) {
                alert.SwitchToIndeterminateAction("Extracting...");
            }
            base.ExtractUpdate();
        }

        protected override void ExtractUpdateCompleted(string extractPath) {
            base.ExtractUpdateCompleted(extractPath);

            if (alert != null) {
                alert.SwitchToReadyToInstall() ;
            }
        }

        public void UpdateWindowClosed(SUUpdateAlert alert) {
            AbortUpdate();
        }

        public void UpdateAlertMadeChoice(SUUpdateAlert anAlert, SUUpdateAlertChoice choice, bool shouldCloseWindowIfNeeded) {

            Host.SetObjectForUserDefaultsKey(null, SUConstants.SUSkippedVersionKey);

            switch (choice) {
                case SUUpdateAlertChoice.SUInstallUpdateChoice:

                    // Download!
                    anAlert.SwitchToDownloadAction();
                    DownloadUpdate();

                    break;

                case SUUpdateAlertChoice.SUSkipThisVersionChoice:

                    Host.SetObjectForUserDefaultsKey(updateItem.VersionString, SUConstants.SUSkippedVersionKey);
                    anAlert.Delegate = null;
                    if (shouldCloseWindowIfNeeded) { anAlert.Window.Close(); }
                    alert = null;

                    AbortUpdate();
                    break;

                case SUUpdateAlertChoice.SURemindMeLaterChoice:

                    anAlert.Delegate = null;
                    if (shouldCloseWindowIfNeeded) { anAlert.Window.Close(); }
                    alert = null;

                    AbortUpdate();
                    break;
            }
        }

        public void InstallUpdate(SUUpdateAlert alert) {
            InstallUpdate();
        }
    }
}
