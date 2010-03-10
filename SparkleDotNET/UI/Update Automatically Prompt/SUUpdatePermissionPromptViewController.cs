using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUUpdatePermissionPromptViewController : KNViewController {

        public SUUpdatePermissionPromptViewController(SUHost host)
            : base(new SUUpdatePermissionPromptView()) {

                Host = host;
                UpdateHeaderDescription.Text = String.Format("Should {0} automatically check for updates? You can always check for updates manually in the {1} menu.",
                    Host.Name, "Help");
                IconView.Source = host.Icon;
                
                string systemInfo = "";

                foreach (Dictionary<string, string> item in SUSystemProfiler.SystemProfileForHost(Host)) {

                    systemInfo = String.Concat(systemInfo, item.ValueForKey(SUConstants.SUProfileItemDisplayKeyKey), ": ",
                        item.ValueForKey(SUConstants.SUProfileItemDisplayValueKey), Environment.NewLine);

                }

                InfoBox.Text = systemInfo;

        }

        public TextBox InfoBox {
            get;
            set;
        }

        public CheckBox SendProfileCheck {
            get;
            set;
        }

        public TextBlock UpdateHeaderDescription {
            get;
            set;
        }

        public Button CheckAutomaticallyButton {
            get;
            set;
        }

        public Button DontCheckButton {
            get;
            set;
        }

        public Image IconView {
            get;
            set;
        }

        public SUHost Host {
            get;
            set;
        }


    }
}
