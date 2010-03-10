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
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;
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
            updater.FeedURL = "http://www.kennettnet.co.uk/files/SparkleDotNETTestAppUpdates.xml";
            updater.CheckForUpdates();

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
            //MessageBox.Show("Found new update: " + item.VersionString);
        }

        public void UpdaterDidNotFindUpdate(SUUpdater updater) {
            //MessageBox.Show("No new updates");
        }


        private void CreateKeysButton_Click(object sender, RoutedEventArgs e) {

            DSACryptoServiceProvider provider = new DSACryptoServiceProvider();
            string privateKeyFile = Convert.ToBase64String(provider.ExportCspBlob(true));
            string publicKeyFile = Convert.ToBase64String(provider.ExportCspBlob(false));

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sparkle Private Key File|*.sparklePrivateKey";
            dialog.Title = "Please choose a location for your Private Key";
            dialog.FileName = "Update Signing Private Key";
            dialog.ShowDialog();

            if (!String.IsNullOrEmpty(dialog.FileName)) {

                try {

                    File.WriteAllText(dialog.FileName, privateKeyFile, Encoding.UTF8);

                } catch (Exception ex) {

                    System.Windows.MessageBox.Show(String.Format("File could not be saved: {0}", ex.Message));
                    return;
                }

            } else {
                return;
            }

            dialog = new SaveFileDialog();
            dialog.Filter = "Sparkle Public Key File|*.sparklePublicKey";
            dialog.Title = "Please choose a location for your Public Key";
            dialog.FileName = "Update Signing Public Key";
            dialog.ShowDialog();

            if (!String.IsNullOrEmpty(dialog.FileName)) {

                try {

                    File.WriteAllText(dialog.FileName, publicKeyFile, Encoding.UTF8);

                } catch (Exception ex) {

                    System.Windows.MessageBox.Show(String.Format("File could not be saved: {0}", ex.Message));
                    return;
                }

            }
        }

        private void SignFileButton_Click(object sender, RoutedEventArgs e) {

            DSACryptoServiceProvider provider = new DSACryptoServiceProvider();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Sparkle Private Key File|*.sparklePrivateKey";
            dialog.Title = "Please choose your Private Key";
            dialog.ShowDialog();

            if (!String.IsNullOrEmpty(dialog.FileName)) {

                try {

                    provider.ImportCspBlob(Convert.FromBase64String(File.ReadAllText(dialog.FileName)));

                } catch (Exception ex) {

                    System.Windows.MessageBox.Show(String.Format("Key not be used: {0}", ex.Message));
                    return;

                }
            } else {
                return;
            }


            dialog = new OpenFileDialog();
            dialog.Filter = "All Files|*.*";
            dialog.Title = "Please choose the file to sign";
            dialog.ShowDialog();

            if (!String.IsNullOrEmpty(dialog.FileName)) {

                try {

                    DSASignatureFormatter formatter = new DSASignatureFormatter(provider);
                    formatter.SetHashAlgorithm("SHA1");

                    SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                    byte[] hash = sha1.ComputeHash(File.ReadAllBytes(dialog.FileName));
                    
                    string signature = Convert.ToBase64String(formatter.CreateSignature(hash));

                    SignatureBox.Text = signature;

                } catch (Exception ex) {
                    System.Windows.MessageBox.Show(String.Format("File could not be signed: {0}", ex.Message));
                    return;
                }


            } else {
                return;
            }
        }

    }
}
