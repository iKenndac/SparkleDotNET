using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNFoundation;

namespace SparkleDotNET {

    public interface SUUpdatePermissionPromptWindowControllerDelegate {

        void PermissionPromptDidComplete(bool shouldCheckForUpdates, bool shouldSendSystemInfo);
    }

    class SUUpdatePermissionPromptWindowController : KNWindowController {

        private SUUpdatePermissionPromptViewController viewController;
        bool sendDelegateOnWindowClose;

        public SUUpdatePermissionPromptWindowController(SUHost host)
            : base(new SUUpdatePermissionPromptWindow()) {

                Window.Icon = host.Icon;
                Window.Topmost = true;

                viewController = new SUUpdatePermissionPromptViewController(host);
                viewController.CheckAutomaticallyButton.Click += CheckAutomatically;
                viewController.DontCheckButton.Click += DontCheck;

                ViewController = viewController;
                sendDelegateOnWindowClose = true;
        }

        void WindowIsClosing() {
            if (sendDelegateOnWindowClose) {
                if (Delegate != null) {
                    Delegate.PermissionPromptDidComplete(false, ShouldSendSystemInfo);
                }
            }
        }

        void CheckAutomatically(object sender, EventArgs e) {
            sendDelegateOnWindowClose = false;

            if (Delegate != null) {
                Delegate.PermissionPromptDidComplete(true, ShouldSendSystemInfo);
            }

            Window.Close();
        }

        void DontCheck(object sender, EventArgs e) {
            sendDelegateOnWindowClose = false;

            if (Delegate != null) {
                Delegate.PermissionPromptDidComplete(false, ShouldSendSystemInfo);
            }

            Window.Close();
        }

        private bool ShouldSendSystemInfo {
            get {
                return viewController.SendProfileCheck.IsChecked.Value;
            }
        }

        public SUUpdatePermissionPromptWindowControllerDelegate Delegate {
            get;
            set;
        }

    }
}
