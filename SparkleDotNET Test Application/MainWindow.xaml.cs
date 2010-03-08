using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SparkleDotNET;

namespace SparkleDotNET_Test_Application {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, SUUpdaterDelegate {
        public MainWindow() {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {

            SUUpdater updater = SUUpdater.SharedUpdater();
            updater.Delegate = this;
            updater.FeedURL = "http://dbprocessor.kennettnet.co.uk/MusicRescueUpdates.xml";
            updater.CheckForUpdateInformation();

        }

        public SUVersionComparison VersionComparatorForUpdater(SUUpdater updater) {
            return null;
        }

        public SUAppcastItem BestValidUpdateInAppcastForUpdater(SUAppcast appcast, SUUpdater updater) {
            return null;
        }

        public void UpdaterDidFinishLoadingAppcast(SUUpdater updater, SUAppcast appcast) {
            
        }

        public void UpdaterDidFindValidUpdate(SUUpdater updater, SUAppcastItem item) {
            MessageBox.Show("Found new update: " + item.VersionString);
        }

        public void UpdaterDidNotFindUpdate(SUUpdater updater) {
            MessageBox.Show("No new updates");
        }
    }
}
