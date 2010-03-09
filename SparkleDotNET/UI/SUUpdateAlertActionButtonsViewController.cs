using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using KNFoundation;

namespace SparkleDotNET {
    class SUUpdateAlertActionButtonsViewController : KNViewController {


        public SUUpdateAlertActionButtonsViewController(UserControl view)
            : base(view) {
        }

        public Button InstallButton {
            protected set;
            get;
        }

        public Button SkipVersionButton {
            protected set;
            get;
        }

        public Button RemindLaterButton {
            protected set;
            get;
        }


    }
}
