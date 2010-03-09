using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Net;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {

    public interface SUUpdateAlertDegate {

        void UpdateAlertMadeChoice(SUUpdateAlert alert, SUUpdateAlertChoice choice);
        void CancelDownload(SUUpdateAlert alert);
    }

    public enum SUUpdateAlertChoice {
        SUInstallUpdateChoice,
        SURemindMeLaterChoice,
        SUSkipThisVersionChoice
    };

    public class SUUpdateAlert : KNWindowController {

        public static SUUpdateAlert CreateAlert(SUHost host, SUAppcastItem item) {
            SUUpdateAlertWindow window = new SUUpdateAlertWindow();
            SUUpdateAlert alert = new SUUpdateAlert(window, host, item);
            return alert;
        }

        private SUUpdateAlertActionButtonsViewController buttons;
        private SUUpdateAlertWindowViewController mainViewController;
        private SUUpdateAlertDownloadProgressViewController downloadingViewController;
        private SUUpdateAlertDegate del;

        public SUUpdateAlert(Window window, SUHost host, SUAppcastItem item)
            : base(window) {

            mainViewController = new SUUpdateAlertWindowViewController(new SUUpdateAlertWindowView());
            ViewController = mainViewController;

            buttons = new SUUpdateAlertActionButtonsViewController(new SUUpdateAlertActionButtonsView());
            buttons.InstallButton.Click += InstallButtonClicked;
            buttons.RemindLaterButton.Click += RemindLaterButtonClicked;
            buttons.SkipVersionButton.Click += SkipVersionButtonClicked;

            downloadingViewController = new SUUpdateAlertDownloadProgressViewController(new SUUpdateAlertDownloadProgressView());
            downloadingViewController.CancelButton.Click += CancelDownloadClicked;

            mainViewController.ActionViewController = buttons;
            mainViewController.Host = host;
            mainViewController.Item = item;
        }

        public SUUpdateAlertDegate Delegate {
            get { return del; }
            set {
                this.WillChangeValueForKey("Delegate");
                del = value;
                this.DidChangeValueForKey("Delegate");
            }
        }

        public void SwitchToDownloadAction() {
            mainViewController.ActionViewController = downloadingViewController;
        }

        public void UpdateDownloadProgressWithEvent(DownloadProgressChangedEventArgs e) {
            downloadingViewController.ProgressBar.Value = e.ProgressPercentage;
            downloadingViewController.ProgressLabel.Text = String.Format("Downloading {0} of {1}...",
                HumanReadableFileSize(e.BytesReceived), 
                HumanReadableFileSize(e.TotalBytesToReceive));
        
        }

        private void CancelDownloadClicked(object sender, EventArgs e) {
            if (Delegate != null) {
                Delegate.CancelDownload(this);
                mainViewController.ActionViewController = buttons;
                downloadingViewController.ProgressBar.Value = 0;
                downloadingViewController.ProgressLabel.Text = "Downloading update...";
            }
        }

        private void InstallButtonClicked(object sender, EventArgs e) {
            MakeChoice(SUUpdateAlertChoice.SUInstallUpdateChoice);
        }

        private void RemindLaterButtonClicked(object sender, EventArgs e) {
            MakeChoice(SUUpdateAlertChoice.SURemindMeLaterChoice);
        }

        private void SkipVersionButtonClicked(object sender, EventArgs e) {
            MakeChoice(SUUpdateAlertChoice.SUSkipThisVersionChoice);
        }

        private void MakeChoice(SUUpdateAlertChoice choice) {
            if (Delegate != null) {
                Delegate.UpdateAlertMadeChoice(this, choice);
            } else {
                // Eeek! Panic! 
                this.Window.Close();
            }
        }

        private string HumanReadableFileSize(double value) {

           try {

               double dblAns = 0;

            if (value <= 0) {
                //this method should never attempt to divide by 0
                //this is just an extra precaution
                return "0 Bytes";
            } else if ( value >= (Math.Pow(1024, 4))) {
                //TB
                dblAns = (((value / 1024) / 1024) / 1024) / 1024;
                return String.Format("{0:###,###,##0.##}", dblAns) + " TB";
            } else if (value >= (Math.Pow(1024, 3))) {
                //GB
                dblAns = ((value / 1024) / 1024) / 1024;
                return String.Format("{0:###,###,##0.##}", dblAns) + " GB";
            } else if (value >= (Math.Pow(1024, 2))) {
                //MB
                dblAns = (value / 1024) / 1024;
                return String.Format("{0:###,###,##0.0}", dblAns) + " MB";
            } else if ( value >= 1024) {
                //KB
                dblAns = value / 1024;
                return String.Format("{0:###,###,##0}", dblAns) + " KB";
            } else {
                //Bytes
                return String.Format("{0:###,###,##0}", dblAns) + " Bytes";
            }

            } catch {}
            
            return "unknown";
        }

    }
}
