using System;
using System.Collections;
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
using KNFoundation;
using KNFoundation.KNKVC;
using KNControls;

namespace SparkleDotNET_Test_Application {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, KNKVOObserver, KNTableViewDataSource, KNTableViewDelegate {


        private ArrayList keys = new ArrayList();
        private UpdateSigningKey selectedKey;

        public MainWindow() {
            InitializeComponent();

            // Load!

            if (KNUserDefaults.StandardUserDefaults().ObjectForKey("Keys") != null) {
                foreach (Dictionary<string, object> dict in (ArrayList)KNUserDefaults.StandardUserDefaults().ObjectForKey("Keys")) {
                    keys.Add(new UpdateSigningKey(dict));
                }
            }

            KNTableView keyTable = new KNTableView();
            keyTable.CornerCell = new KNLeopardCornerCell();
            KNCell cell = new KNTextCell();
            ((KNTextCell)cell).IsEditable = true;
            KNTableColumnDelegate del = null;
            KNHeaderCell header = new KNLeopardStyleHeaderCell("Name", false, true, System.Drawing.StringAlignment.Near);
            KNTableColumn col = new KNTableColumn("name", ref cell, ref header, ref del);
            col.Width = (int)KeyListHost.Width;
            keyTable.AddColumn(col);

            keyTable.TableDelegate = this;
            keyTable.DataSource = this;
            keyTable.AlternatingRowBackgrounds = true;

            keyTable.CellPerformedAction += Action;
            keyTable.SelectionChanged += TableSelectionChanged;

            KeyListHost.Child = keyTable;

            KNLeopardStyleHeaderButton button = new KNLeopardStyleHeaderButton();
            button.Enabled = false;
            ButtonBarHost.Child = button;

            SUUpdater updater = SUUpdater.SharedUpdater();

            this.AddObserverToKeyPathWithOptions(this, "SelectedKey", KNKeyValueObservingOptions.KNKeyValueObservingOptionInitial, null);

        }

        public UpdateSigningKey SelectedKey {
            get { return selectedKey; }
            set {
                this.WillChangeValueForKey("SelectedKey");
                selectedKey = value;
                this.DidChangeValueForKey("SelectedKey");
            }
        }


        private void button1_Click(object sender, RoutedEventArgs e) {

            SUUpdater.SharedUpdater().CheckForUpdates();
        }

        private void Save() {

            ArrayList keysForUserDefaults = new ArrayList();
            foreach (UpdateSigningKey key in keys) {
                keysForUserDefaults.Add(key.PlistRepresentation());
            }

            KNUserDefaults.StandardUserDefaults().SetObjectForKey(keysForUserDefaults, "Keys");
            KNUserDefaults.StandardUserDefaults().Synchronise();
        }

        #region Buttons 

        private void AddKey_Click(object sender, RoutedEventArgs e) {
            keys.Add(new UpdateSigningKey());
            ((KNTableView)KeyListHost.Child).ReloadData();
            Save();
        }

        private void ExportPrivateButton_Click(object sender, RoutedEventArgs e) {

            string privateKeyFile = Convert.ToBase64String(SelectedKey.PrivateKey);
            string privateFileName;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sparkle Private Key File|*.sparklePrivateKey";
            dialog.Title = "Please choose a location for your Private Key";
            dialog.FileName = SelectedKey.Name;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }

            privateFileName = dialog.FileName;

            if (String.IsNullOrEmpty(privateFileName)) {
                return;
            }

            try {
                File.WriteAllText(privateFileName, privateKeyFile, Encoding.UTF8);
            } catch (Exception ex) {

                System.Windows.MessageBox.Show(String.Format("File could not be saved: {0}", ex.Message));
                return;
            }

        }

        private void ExportPublicButton_Click(object sender, RoutedEventArgs e) {

            string publicKeyFile = Convert.ToBase64String(SelectedKey.PublicKey);
            string publicFileName;

            SaveFileDialog dialog = new SaveFileDialog();

            dialog = new SaveFileDialog();
            dialog.Filter = "Sparkle Public Key File|*.sparklePublicKey";
            dialog.Title = "Please choose a location for your Public Key";
            dialog.FileName = SelectedKey.Name;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
                return;
            }

            publicFileName = dialog.FileName;

            if (String.IsNullOrEmpty(publicFileName)) {
                return;
            }

            try {
                File.WriteAllText(publicFileName, publicKeyFile, Encoding.UTF8);
            } catch (Exception ex) {

                System.Windows.MessageBox.Show(String.Format("File could not be saved: {0}", ex.Message));
                return;
            }
        }

        private void SignFileButton_Click(object sender, RoutedEventArgs e) {

            DSACryptoServiceProvider provider = new DSACryptoServiceProvider();
            provider.ImportCspBlob(SelectedKey.PrivateKey);

            OpenFileDialog dialog = new OpenFileDialog();
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

        #endregion

        public void Action(int row, ref KNTableColumn Column, KNActionCell Cell) {

            ((UpdateSigningKey)keys[row]).Name = (string)Cell.ObjectValue;
            Save();
        }

        public int NumberOfItemsInTableView(ref KNTableView tableView) {
            return keys.Count;
        }

        public object ObjectForRow(ref KNTableView tableView, ref KNTableColumn tableColumn, int rowIndex) {
            return ((UpdateSigningKey)keys[rowIndex]).Name;
        }

        public SortOrder ColumnHeaderClicked(ref KNTableView tableView, ref KNTableColumn column, SortOrder suggestedNewSortOrder) {
            return suggestedNewSortOrder;
        }

        public bool ShouldSelectRow(ref KNTableView tableView, int rowIndex) {
            return true;
        }

        public void TableSelectionChanged(ArrayList Rows) {

            if (Rows.Count == 0) {
                SelectedKey = null;
            } else {
                SelectedKey = (UpdateSigningKey)keys[(int)Rows[0]];
            }
        }


        public void ObserveValueForKeyPathOfObject(string keyPath, object obj, Dictionary<string, object> change, object context) {
            if (keyPath.Equals("SelectedKey")) {
                SignFileButton.IsEnabled = (SelectedKey != null);
                ExportPrivateButton.IsEnabled = (SelectedKey != null);
                ExportPublicButton.IsEnabled = (SelectedKey != null);
            }
        }

        
    }
}
