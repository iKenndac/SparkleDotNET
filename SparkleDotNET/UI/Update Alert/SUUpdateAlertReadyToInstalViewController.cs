using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using KNFoundation;

namespace SparkleDotNET {
    class SUUpdateAlertReadyToInstallViewController : KNViewController {


        public SUUpdateAlertReadyToInstallViewController(UserControl view)
            : base(view) {
        }

        public Button InstallButton {
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
