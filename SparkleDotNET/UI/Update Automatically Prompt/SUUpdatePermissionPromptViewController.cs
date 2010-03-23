using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUUpdatePermissionPromptViewController : KNViewController {

        private double oldHeight;

        public SUUpdatePermissionPromptViewController(SUHost host)
            : base(new SUUpdatePermissionPromptView()) {

                Host = host;
                UpdateHeaderDescription.Text = String.Format(SULocalizedStrings.StringForKey("Update Automatically Prompt Header"),
                    Host.Name, SULocalizedStrings.StringForKey("Help"));
                IconView.Source = host.LargeIcon;
                ExtendedInfoContainer.Expanded += ExpandWindow;
                ExtendedInfoContainer.Collapsed += CollapseWindow;
                ExtendedInfoContainer.IsExpanded = false;
                oldHeight = 150;

                string systemInfo = "";

                foreach (Dictionary<string, string> item in SUSystemProfiler.SystemProfileForHost(Host)) {

                    systemInfo = String.Concat(systemInfo, item.ValueForKey(SUConstants.SUProfileItemDisplayKeyKey), ": ",
                        item.ValueForKey(SUConstants.SUProfileItemDisplayValueKey), Environment.NewLine);

                }

                InfoBox.Text = systemInfo;

        }

        private void CollapseWindow(object sender, EventArgs e) {
            oldHeight = ExtendedInfoContainer.ActualHeight;
            View.Height = 28 + ExtendedInfoContainer.Margin.Bottom + ExtendedInfoContainer.Margin.Top;
            //ExpandWindow(sender, e);
        }

        private void ExpandWindow(object sender, EventArgs e) {
            View.Height = oldHeight + ExtendedInfoContainer.Margin.Bottom + ExtendedInfoContainer.Margin.Top;
        }

        public Expander ExtendedInfoContainer {
            get;
            set;
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
