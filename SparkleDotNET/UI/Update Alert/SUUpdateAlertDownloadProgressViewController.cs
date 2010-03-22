using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using KNFoundation;

namespace SparkleDotNET {
    class SUUpdateAlertDownloadProgressViewController : KNViewController {


        public SUUpdateAlertDownloadProgressViewController(UserControl view)
            : base(view) {
        }

        public void ResetView() {

            ProgressLabel.Text = SULocalizedStrings.StringForKey("Downloading update...");
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;

        }

        public Button CancelButton {
            protected set;
            get;
        }

        public ProgressBar ProgressBar {
            protected set;
            get;
        }

        public TextBlock ProgressLabel {
            protected set;
            get;
        }


    }
}
