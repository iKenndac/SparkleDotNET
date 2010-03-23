using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SparkleDotNET;

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
            
            string message = SULocalizedStrings.StringForKey("Generic Error Message");
            string specificMessage = SULocalizedStrings.StringForKey(error.Message);

            if (!String.IsNullOrWhiteSpace(specificMessage)) {
                message = specificMessage;
            }

            MessageBox.Show(message, SULocalizedStrings.StringForKey("Update Failed"), MessageBoxButton.OK, MessageBoxImage.Error);

            base.AbortUpdateWithError(error);
        }

        protected override void DidNotFindUpdate() {

            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidNotFindUpdate(Updater);
            }

            MessageBox.Show(SULocalizedStrings.StringForKey(SUConstants.SUNoUpdateError), SULocalizedStrings.StringForKey("Up To Date"), MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            AbortUpdate();            

        }

        protected override void DownloadDidProgress(object sender, System.Net.DownloadProgressChangedEventArgs e) {
            if (alert != null) {
                alert.UpdateDownloadProgressWithEvent(e);
            }
        }

        protected override void VerifySignature() {
            if (alert != null) {
                alert.SwitchToIndeterminateAction(SULocalizedStrings.StringForKey("Verifying..."));
            }
            base.VerifySignature();
        }

        protected override void ExtractUpdate() {
            if (alert != null) {
                alert.SwitchToIndeterminateAction(SULocalizedStrings.StringForKey("Extracting..."));
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
