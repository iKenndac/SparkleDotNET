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
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            SUUpdater updater = SUUpdater.SharedUpdater();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {

            

            SUUpdater.SharedUpdater().CheckForUpdates();
        }


        private void CreateKeysButton_Click(object sender, RoutedEventArgs e) {

            DSACryptoServiceProvider provider = new DSACryptoServiceProvider();
            string privateKeyFile = Convert.ToBase64String(provider.ExportCspBlob(true));
            string publicKeyFile = Convert.ToBase64String(provider.ExportCspBlob(false));

            string privateFileName, publicFileName;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sparkle Private Key File|*.sparklePrivateKey";
            dialog.Title = "Please choose a location for your Private Key";
            dialog.FileName = "Update Signing Private Key";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }

            privateFileName = dialog.FileName;

            if (String.IsNullOrEmpty(privateFileName)) {
                return;
            }

            dialog = new SaveFileDialog();
            dialog.Filter = "Sparkle Public Key File|*.sparklePublicKey";
            dialog.Title = "Please choose a location for your Public Key";
            dialog.FileName = "Update Signing Public Key";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }

            publicFileName = dialog.FileName;

            if (String.IsNullOrEmpty(publicFileName)) {
                return;
            }

            try {
                File.WriteAllText(privateFileName, privateKeyFile, Encoding.UTF8);
                File.WriteAllText(publicFileName, publicKeyFile, Encoding.UTF8);
            } catch (Exception ex) {

                System.Windows.MessageBox.Show(String.Format("File could not be saved: {0}", ex.Message));
                return;
            }
        }
        

        private void SignFileButton_Click(object sender, RoutedEventArgs e) {

            DSACryptoServiceProvider provider = new DSACryptoServiceProvider();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Sparkle Private Key File|*.sparklePrivateKey";
            dialog.Title = "Please choose your Private Key";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }

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
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }

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
