using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Net;
using System.Runtime.InteropServices;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {

    public interface SUUpdateAlertDegate {

        void UpdateAlertMadeChoice(SUUpdateAlert alert, SUUpdateAlertChoice choice, bool shouldCloseWindowIfNeeded);
        void CancelDownload(SUUpdateAlert alert);
        void InstallUpdate(SUUpdateAlert alert);
        void UpdateWindowClosed(SUUpdateAlert alert);
    }

    public enum SUUpdateAlertChoice {
        SUInstallUpdateChoice,
        SURemindMeLaterChoice,
        SUSkipThisVersionChoice
    };

    public class SUUpdateAlert : KNWindowController {

        private enum WindowStatus {
            WaitingForInitialAction,
            Downloading,
            UncancellableAction,
            WaitingForInstall,
            UpdateAborted
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImportAttribute("user32.dll")]
        static extern int FindWindow(String ClassName, String WindowName);

        [DllImport("user32.dll")]
        static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public Int32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        public static SUUpdateAlert CreateAlert(SUHost host, SUAppcastItem item) {
            SUUpdateAlertWindow window = new SUUpdateAlertWindow();
            SUUpdateAlert alert = new SUUpdateAlert(window, host, item);
            return alert;
        }

        private SUUpdateAlertActionButtonsViewController buttons;
        private SUUpdateAlertWindowViewController mainViewController;
        private SUUpdateAlertDownloadProgressViewController downloadingViewController;
        private SUUpdateAlertIndeterminateProgressViewController indeterminateViewController;
        private SUUpdateAlertReadyToInstallViewController readyToInstallViewController;
        private SUUpdateAlertDegate del;
        private WindowStatus status;

        public SUUpdateAlert(Window window, SUHost host, SUAppcastItem item)
            : base(window) {

            Window.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            Window.Closing += WindowShouldClose;
            Window.Icon = host.Icon;
            Window.Topmost = true;

            status = WindowStatus.WaitingForInitialAction;

            mainViewController = new SUUpdateAlertWindowViewController(new SUUpdateAlertWindowView());
            ViewController = mainViewController;

            buttons = new SUUpdateAlertActionButtonsViewController(new SUUpdateAlertActionButtonsView());
            buttons.InstallButton.Click += DownloadButtonClicked;
            buttons.RemindLaterButton.Click += RemindLaterButtonClicked;
            buttons.SkipVersionButton.Click += SkipVersionButtonClicked;

            downloadingViewController = new SUUpdateAlertDownloadProgressViewController(new SUUpdateAlertDownloadProgressView());
            downloadingViewController.CancelButton.Click += CancelDownloadClicked;

            indeterminateViewController = new SUUpdateAlertIndeterminateProgressViewController(new SUUpdateAlertIndeterminateProgressView());
            readyToInstallViewController = new SUUpdateAlertReadyToInstallViewController(new SUUpdateAlertReadyToInstallView());

            readyToInstallViewController.InstallButton.Click += InstallButtonClicked;

            mainViewController.ActionViewController = buttons;
            mainViewController.Host = host;
            mainViewController.Item = item;
        }

        public void ForceClose() {
            status = WindowStatus.UpdateAborted;
            try {
                Window.Close();
            } catch { }
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
            status = WindowStatus.Downloading;
            downloadingViewController.ResetView();
            mainViewController.ActionViewController = downloadingViewController;
            Window.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            Window.TaskbarItemInfo.ProgressValue = 0.0;
        }

        public void SwitchToIndeterminateAction(string statusText) {
            status = WindowStatus.UncancellableAction;
            indeterminateViewController.ProgressLabel.Text = statusText;
            mainViewController.ActionViewController = indeterminateViewController;

            Window.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;

        }

        public void SwitchToReadyToInstall() {

            status = WindowStatus.WaitingForInstall;

            mainViewController.ActionViewController = readyToInstallViewController;
            Window.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            Window.TaskbarItemInfo.ProgressValue = 1.0;

            IntPtr handle = (IntPtr)FindWindow(null, Window.Title);

            if (handle != IntPtr.Zero) {

                if (handle != GetForegroundWindow()) {

                    // Flash if we're not frontmost

                    FLASHWINFO info = new FLASHWINFO();
                    info.hwnd = handle;
                    info.dwFlags = 12 | 3;
                    info.uCount = UInt32.MaxValue;
                    info.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));

                    FlashWindowEx(ref info);

                } 
            }
        }

        public void UpdateDownloadProgressWithEvent(DownloadProgressChangedEventArgs e) {
            downloadingViewController.ProgressBar.Value = e.ProgressPercentage;
            downloadingViewController.ProgressLabel.Text = String.Format(SULocalizedStrings.StringForKey("Downloading Status"),
                HumanReadableFileSize(e.BytesReceived), 
                HumanReadableFileSize(e.TotalBytesToReceive));

            Window.TaskbarItemInfo.ProgressValue = ((double)e.ProgressPercentage) / 100.0;
        
        }

        private void CancelDownloadClicked(object sender, EventArgs e) {
            if (Delegate != null) {
                Delegate.CancelDownload(this);
                mainViewController.ActionViewController = buttons;
                Window.TaskbarItemInfo.ProgressValue = 0;
                status = WindowStatus.WaitingForInitialAction;
                Window.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                downloadingViewController.ProgressBar.Value = 0;
                downloadingViewController.ProgressLabel.Text = SULocalizedStrings.StringForKey("Downloading update...");
            }
        }

        private void WindowShouldClose(object sender, CancelEventArgs e) {

            if (status == WindowStatus.Downloading) {
                CancelDownloadClicked(null, null);
                if (Delegate != null) {
                    Delegate.UpdateWindowClosed(this);
                }
            } else if (status == WindowStatus.UncancellableAction) {
                e.Cancel = true;
                MessageBox.Show(SULocalizedStrings.StringForKey("Update Cannot Be Cancelled"));
            } else if (status == WindowStatus.WaitingForInitialAction) {
                if (Delegate != null) {
                    Delegate.UpdateAlertMadeChoice(this, SUUpdateAlertChoice.SURemindMeLaterChoice, false);
                }

            } else if (status == WindowStatus.WaitingForInstall) {
            }

            if (!e.Cancel && status != WindowStatus.UpdateAborted) {
                if (Delegate != null) {
                    Delegate.UpdateWindowClosed(this);
                }
            }

        }

        private void DownloadButtonClicked(object sender, EventArgs e) {
            MakeChoice(SUUpdateAlertChoice.SUInstallUpdateChoice);
        }

        private void RemindLaterButtonClicked(object sender, EventArgs e) {
            MakeChoice(SUUpdateAlertChoice.SURemindMeLaterChoice);
        }

        private void SkipVersionButtonClicked(object sender, EventArgs e) {
            MakeChoice(SUUpdateAlertChoice.SUSkipThisVersionChoice);
        }

        private void InstallButtonClicked(object sender, EventArgs e) {
            readyToInstallViewController.InstallButton.IsEnabled = false;
            if (Delegate != null) {
                Delegate.InstallUpdate(this);
            }
        }

        private void MakeChoice(SUUpdateAlertChoice choice) {
            if (Delegate != null) {
                Delegate.UpdateAlertMadeChoice(this, choice, true);
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
                return "0" + SULocalizedStrings.StringForKey("Byte Unit");
            } else if ( value >= (Math.Pow(1024, 4))) {
                //TB
                dblAns = (((value / 1024) / 1024) / 1024) / 1024;
                return String.Format("{0:###,###,##0.##}", dblAns) + SULocalizedStrings.StringForKey("Terabyte Unit");
            } else if (value >= (Math.Pow(1024, 3))) {
                //GB
                dblAns = ((value / 1024) / 1024) / 1024;
                return String.Format("{0:###,###,##0.##}", dblAns) + SULocalizedStrings.StringForKey("Gigabyte Unit");
            } else if (value >= (Math.Pow(1024, 2))) {
                //MB
                dblAns = (value / 1024) / 1024;
                return String.Format("{0:###,###,##0.0}", dblAns) + SULocalizedStrings.StringForKey("Megabyte Unit");
            } else if ( value >= 1024) {
                //KB
                dblAns = value / 1024;
                return String.Format("{0:###,###,##0}", dblAns) + SULocalizedStrings.StringForKey("Kilobyte Unit");
            } else {
                //Bytes
                return String.Format("{0:###,###,##0}", dblAns) + SULocalizedStrings.StringForKey("Byte Unit");
            }

            } catch {}

           return SULocalizedStrings.StringForKey("unknown");
        }

    }
}
