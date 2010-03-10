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

        protected override void ExtractUpdateCompleted() {
            base.ExtractUpdateCompleted();

            if (alert != null) {
                alert.SwitchToReadyToInstall() ;
            }
        }

        public void UpdateAlertMadeChoice(SUUpdateAlert alert, SUUpdateAlertChoice choice) {

            Host.SetObjectForUserDefaultsKey(null, SUSkippedVersionKey);

            switch (choice) {
                case SUUpdateAlertChoice.SUInstallUpdateChoice:

                    // Download!
                    alert.SwitchToDownloadAction();
                    DownloadUpdate();

                    break;

                case SUUpdateAlertChoice.SUSkipThisVersionChoice:

                    Host.SetObjectForUserDefaultsKey(updateItem.VersionString, SUSkippedVersionKey);
                    alert.Delegate = null;
                    alert.Window.Close();

                    AbortUpdate();
                    break;

                case SUUpdateAlertChoice.SURemindMeLaterChoice:

                    alert.Delegate = null;
                    alert.Window.Close();

                    AbortUpdate();
                    break;
            }
        }

        public void InstallUpdate(SUUpdateAlert alert) {

        }
    }
}
