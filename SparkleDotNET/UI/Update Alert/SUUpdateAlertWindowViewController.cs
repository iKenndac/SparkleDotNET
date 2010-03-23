using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUUpdateAlertWindowViewController : KNViewController, KNKVOObserver {

        private SUHost host;
        private SUAppcastItem item;
        private KNViewController actionViewController;
        private Canvas currentActionContainer;

        public SUUpdateAlertWindowViewController(UserControl view)
            : base(view) {

                this.AddObserverToKeyPathWithOptions(this, "Host", 0, null);
                this.AddObserverToKeyPathWithOptions(this, "Item", 0, null);
        }

        public SUHost Host {
            get { return host; }
            set {
                this.WillChangeValueForKey("Host");
                host = value;
                this.DidChangeValueForKey("Host");
            }
        }

        public SUAppcastItem Item {
            get { return item; }
            set {
                this.WillChangeValueForKey("Item");
                item = value;
                this.DidChangeValueForKey("Item");
            }
        }

        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("Host") || keyPath.Equals("Item")) {
                // Set up the UI!

                if (Host != null && Item != null) {

                    ReleaseNotes.Source = Item.ReleaseNotesURL;
                    ReleaseNotes.LoadCompleted += ReleaseNotesDidLoad;
                    UpdateHeaderLabel.Text = String.Format(SULocalizedStrings.StringForKey("Update Available Header"), Host.Name);

                    if (Item.DisplayVersionString.Equals(Host.DisplayVersion)) {
                        // Display more info if the version strings are the same; useful for betas.
                        UpdateHeaderDescription.Text = String.Format(SULocalizedStrings.StringForKey("Update Available Extra Detail"),
                            Host.Name, Item.DisplayVersionString, Host.DisplayVersion, Item.VersionString, Host.Version);
                    } else {
                        UpdateHeaderDescription.Text = String.Format(SULocalizedStrings.StringForKey("Update Available Detail"),
                            Host.Name, Item.DisplayVersionString, Host.DisplayVersion);
                    }
                    IconView.Source = Host.LargeIcon;
                }
            }
        }

        private void ReleaseNotesDidLoad(object sender, EventArgs e) {

            // Give the UI thread a bit of time to render and settle. 
            // Without this, the window can look a bit flickery if the 
            // page loads really fast.

            DispatcherTimer timer = new DispatcherTimer(TimeSpan.FromSeconds(0.25),
                DispatcherPriority.Normal,
                TimerFired,
                Dispatcher.CurrentDispatcher);

            timer.Start();
        }

        private void TimerFired(object sender, EventArgs e) {
            LoadingReleaseNotesProgressBar.Visibility = Visibility.Hidden;
            ReleaseNotes.Visibility = Visibility.Visible;
        }

        public KNViewController ActionViewController {
            get { return actionViewController; }
            set {

                if (!Object.ReferenceEquals(value, actionViewController)) {

                    this.WillChangeValueForKey("ActionViewController");

                    if (actionViewController != null) {
                        CurrentActionContainer.Children.Remove(actionViewController.View);
                    }

                    actionViewController = value;

                    if (value != null) {

                        Canvas.SetTop(value.View, 0);
                        Canvas.SetLeft(value.View, 0);

                        value.View.Width = CurrentActionContainer.ActualWidth;
                        value.View.Height = CurrentActionContainer.ActualHeight;

                        CurrentActionContainer.Children.Add(value.View);

                    }

                    this.DidChangeValueForKey("ActionViewController");
                }
            }
        }

        private void ActionContainerResized(object sender, SizeChangedEventArgs e) {
            if (ActionViewController != null) {
                ActionViewController.View.Width = e.NewSize.Width;
                ActionViewController.View.Height = e.NewSize.Height;
            }
        }

        public ProgressBar LoadingReleaseNotesProgressBar {
            protected set;
            get;
        }

        public Image IconView {
            protected set;
            get;
        }

        public TextBlock UpdateHeaderLabel {
            protected set;
            get;
        }

        public TextBlock UpdateHeaderDescription {
            protected set;
            get;
        }

        public WebBrowser ReleaseNotes {
            protected set;
            get;
        }

        public Canvas CurrentActionContainer {
            protected set {
                value.SizeChanged += ActionContainerResized;
                currentActionContainer = value;
            }
            get {
                return currentActionContainer;
            }
        }
    }
}
