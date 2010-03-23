using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Web;
using System.IO;
using System.Reflection;
using KNFoundation.KNKVC;
using KNFoundation;

namespace SparkleDotNET {

    public interface SUUpdaterDelegate {
        SUVersionComparison VersionComparatorForUpdater(SUUpdater updater);
        SUAppcastItem BestValidUpdateInAppcastForUpdater(SUAppcast appcast, SUUpdater updater);

        void UpdaterDidFinishLoadingAppcast(SUUpdater updater, SUAppcast appcast);
        void UpdaterDidFindValidUpdate(SUUpdater updater, SUAppcastItem item);
        void UpdaterDidNotFindUpdate(SUUpdater updater);
    }
    
    public class SUUpdater : SUUpdatePermissionPromptWindowControllerDelegate  {

        static Dictionary<KNBundle, SUUpdater> sharedUpdaters = new Dictionary<KNBundle, SUUpdater>();

        /// <summary>
        /// Get the shared update for the main bundle.
        /// </summary>
        /// <returns>The updater for the main bundle.</returns>
        public static SUUpdater SharedUpdater() {
            return UpdaterForBundle(KNBundle.MainBundle());
        }

        /// <summary>
        /// Returns the shared updater for the given bundle.
        /// </summary>
        /// <param name="bundle">The bundle to get the updater for.</param>
        /// <returns>A shared updater for the given bundle.</returns>
        public static SUUpdater UpdaterForBundle(KNBundle bundle) {

            if (bundle == null) {
                bundle = KNBundle.MainBundle();
            }

            SUUpdater updater = null;

            if (!sharedUpdaters.TryGetValue(bundle, out updater)) {
                updater = new SUUpdater(bundle);

            }
           
            return updater;
        }

        private SUHost host;
        private DispatcherTimer checkTimer;
        private SUUpdateDriver driver;
        private SUUpdaterDelegate del;

        private SUUpdater(KNBundle aBundle) {

            KNBundle bundle = KNBundle.BundleWithAssembly(Assembly.GetAssembly(this.GetType()));

            if (aBundle == null) {
                aBundle = KNBundle.MainBundle();
            }

            if (sharedUpdaters.ContainsKey(aBundle)) {
                throw new Exception("Updater for this bundle exists - use SUUpdater.UpdaterForBundle()");
            }

            SUInstaller.AddInstallerForFileType(new SUExecutableInstaller(), ".exe");
            SUInstaller.AddInstallerForFileType(new SUMSIInstaller(), ".msi");

            SUUnarchiver.AddUnarchiverForFileType(new SUZipUnarchiver(), ".zip");
            SUUnarchiver.AddUnarchiverForFileType(new SUExeUnarchiver(), ".exe");
            SUUnarchiver.AddUnarchiverForFileType(new SUExeUnarchiver(), ".msi");

            sharedUpdaters.Add(aBundle, this);
            host = new SUHost(aBundle);

            // Clean out old update files if they exist

            if (host.ObjectForUserDefaultsKey(SUConstants.SUExtractedFilesForCleanupKey) != null) {
                string path = (string)host.ObjectForUserDefaultsKey(SUConstants.SUExtractedFilesForCleanupKey);

                try {

                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                        Directory.Delete(path, true);
                    } else {
                        File.Delete(path);
                    }
                } catch {
                } finally {
                    host.SetObjectForUserDefaultsKey(null, SUConstants.SUExtractedFilesForCleanupKey);
                }
            }

            OfferToAutomaticallyUpdateIfAppropriate();
            ScheduleNextUpdateCheck();
        }

        private SUUpdater()
            : this(KNBundle.MainBundle()) {
        }

        // ------------------

        public void CheckForUpdates() {
            CheckForUpdatesWithDriver(new SUUserInitiatedUpdateDriver(this));
        }

        public void CheckForUpdatesInBackground() {
            CheckForUpdatesWithDriver(new SUScheduledUpdateDriver(this));
            // Todo: Implement auto-downloading and SUAutomaticUpdaterDriver
        }

        public void CheckForUpdateInformation() {
            CheckForUpdatesWithDriver(new SUProbingUpdateDriver(this));
        }

        public bool UpdateInProgress() {
            if (driver == null) {
                return false;
            } else {
                return !driver.Finished;
            }
        }

        public string FeedURL {
            get {

                string url = host.FeedURL;
                if (String.IsNullOrEmpty(url)) {
                    throw new Exception(SUConstants.SUNoFeedURLError);
                }
                return url;
            }
            set {
                this.WillChangeValueForKey("FeedURL");
                host.FeedURL = value;
                this.DidChangeValueForKey("FeedURL");
            }
        }

        public SUUpdaterDelegate Delegate {
            get { return del; }
            set {
                this.WillChangeValueForKey("Delegate");
                del = value;
                this.DidChangeValueForKey("Delegate");
            }
        }

        public bool AutomaticallyChecksForUpdates {
            get {
                if (UpdateCheckInterval == 0) {
                    return false;
                }

                object value = host.ObjectForUserDefaultsKey(SUConstants.SUEnableAutomaticChecksKey);
                if (value != null) {
                    return (bool)value;
                } else {
                    return false;
                }
            }
            set {
                this.WillChangeValueForKey("AutomaticallyChecksForUpdates");
                host.SetObjectForUserDefaultsKey(value, SUConstants.SUEnableAutomaticChecksKey);

                if (value && UpdateCheckInterval == 0) {
                    UpdateCheckInterval = SUConstants.SU_DEFAULT_CHECK_INTERVAL;
                }

                ResetUpdateCycle();

                this.DidChangeValueForKey("AutomaticallyChecksForUpdates");
            }
        }

        public double UpdateCheckInterval {
            get {
                return host.UpdateCheckInterval;
            }
            set {
                this.WillChangeValueForKey("UpdateCheckInterval");
                if (value == 0) {
                    AutomaticallyChecksForUpdates = false;
                }
                host.UpdateCheckInterval = value;
                this.DidChangeValueForKey("UpdateCheckInterval");

                ResetUpdateCycle();
            }
        }

        public DateTime LastUpdateCheckDate {
            get {
                return host.LastUpdateCheckDate;
            }

            set {
                this.WillChangeValueForKey("LastUpdateCheckDate");
                host.LastUpdateCheckDate = value;
                this.DidChangeValueForKey("LastUpdateCheckDate");
            }
        }

        public bool SendsSystemProfile {
            get {
                object value = host.ObjectForUserDefaultsKey(SUConstants.SUSendProfileInfoKey);
                if (value != null) {
                    return (bool)value;
                } else {
                    return false;
                }
            }
            set {
                this.WillChangeValueForKey("SendsSystemProfile");
                host.SetObjectForUserDefaultsKey(value, SUConstants.SUSendProfileInfoKey); ;
                this.DidChangeValueForKey("SendsSystemProfile");
            }
        }

        // -------------------

        private void OfferToAutomaticallyUpdateIfAppropriate() {

            bool shouldPrompt = false;

            if (host.ObjectForUserDefaultsKey(SUConstants.SUEnableAutomaticChecksKey) == null) {
                
                object val = host.ObjectForUserDefaultsKey(SUConstants.SUHasLaunchedBeforeKey);
                if (val == null || (bool)val == false) {
                    host.SetObjectForUserDefaultsKey(true, SUConstants.SUHasLaunchedBeforeKey);
                } else {
                    shouldPrompt = true;
                }
            }

            if (shouldPrompt) {

                SUUpdatePermissionPromptWindowController controller = new SUUpdatePermissionPromptWindowController(host);
                controller.Delegate = this;
                controller.ShowWindow();

            }
        }

        public void PermissionPromptDidComplete(bool shouldCheckForUpdates, bool shouldSendSystemInfo) {

            AutomaticallyChecksForUpdates = shouldCheckForUpdates;
            SendsSystemProfile = shouldSendSystemInfo;
        }

        private void ResetUpdateCycle() {
            ScheduleNextUpdateCheck();
        }

        private void ScheduleNextUpdateCheck() {

            if (checkTimer != null) {
                checkTimer.IsEnabled = false;
                checkTimer.Stop();
                checkTimer = null;
            }

            if (!AutomaticallyChecksForUpdates) {
                return;
            }

            DateTime lastCheckDate = LastUpdateCheckDate;
            double timeSinceLastUpdate = (DateTime.Now - lastCheckDate).TotalSeconds;
            double updateCheckInterval = UpdateCheckInterval;
            double delayUntilCheck = 0.0;

            if (updateCheckInterval < SUConstants.SU_MIN_CHECK_INTERVAL) {
                updateCheckInterval = SUConstants.SU_MIN_CHECK_INTERVAL;
            }

            if (timeSinceLastUpdate < updateCheckInterval) {
                delayUntilCheck = updateCheckInterval - timeSinceLastUpdate;
            }

            if (delayUntilCheck <= 0) {
                CheckForUpdatesInBackground();
            } else {

                checkTimer = new DispatcherTimer(TimeSpan.FromSeconds(delayUntilCheck), DispatcherPriority.Normal, CheckTimerDidFire, Dispatcher.CurrentDispatcher);
                checkTimer.IsEnabled = true;
                checkTimer.Start();
            }
        }

        private void CheckTimerDidFire(object sender, EventArgs e) {
                       
            CheckForUpdatesInBackground();
        }

        private void CheckForUpdatesWithDriver(SUUpdateDriver aDriver) {

            if (UpdateInProgress()) {
                return;
            }

            if (checkTimer != null) {
                checkTimer.IsEnabled = false;
                checkTimer.Stop();
                checkTimer = null;
            }

            host.LastUpdateCheckDate = DateTime.Now;
            
            driver = aDriver;
            driver.CheckForUpdatesAtURLWithHost(ParameterizedFeedURL(), host);

            ResetUpdateCycle();
        }

        
        private string ParameterizedFeedURL() {

           // Only send parameters weekly to help normalise data

            if (!SendsSystemProfile || !LastUpdateWasMoreThanAWeekAgo()) {
                return FeedURL;
            } else {

                ArrayList parameterStrings = new ArrayList();

                foreach (Dictionary<string, string> item in SUSystemProfiler.SystemProfileForHost(host)) {

                    parameterStrings.Add(String.Format("{0}={1}",
                        Uri.EscapeUriString((string)item.ValueForKey(SUConstants.SUProfileItemKeyKey)),
                        Uri.EscapeUriString((string)item.ValueForKey(SUConstants.SUProfileItemValueKey))));
                }

                if (parameterStrings.Count > 0) {

                    string url = FeedURL;
                    string divider = "?";

                    if (url.Contains("?")) {
                        // Just in case the url already contains a query
                        divider = "&";
                    }

                    foreach (string item in parameterStrings) {
                        url = string.Concat(url, divider, item);
                        divider = "&";
                    }

                    System.Windows.MessageBox.Show(url);

                    return url;

                } else {
                    return FeedURL;
                }
            }
        }

        private bool LastUpdateWasMoreThanAWeekAgo() {

            DateTime lastUpdate = LastUpdateCheckDate;
            const double oneWeek = 60 * 60 * 24 * 7;
            return ((DateTime.Now - lastUpdate).TotalSeconds >= oneWeek);
        }


        
    }
}
