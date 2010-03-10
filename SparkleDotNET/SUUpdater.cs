using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
    
    public class SUUpdater {

        public const string SULastCheckTimeKey = "SULastCheckTime";

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
        private Timer checkTimer;
        private SUUpdateDriver driver;
        private string feedURL;
        private SUUpdaterDelegate del;

        private SUUpdater(KNBundle aBundle) {

            if (aBundle == null) {
                aBundle = KNBundle.MainBundle();
            }

            if (sharedUpdaters.ContainsKey(aBundle)) {
                throw new Exception("Updater for this bundle exists - use SUUpdater.UpdaterForBundle()");
            }

            sharedUpdaters.Add(aBundle, this);
            host = new SUHost(aBundle);
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
            get { return feedURL; }
            set {
                this.WillChangeValueForKey("FeedURL");
                feedURL = value;
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

        // -------------------

        private void CheckForUpdatesWithDriver(SUUpdateDriver aDriver) {

            if (UpdateInProgress()) {
                return;
            }

            if (checkTimer != null) {
                checkTimer.Enabled = false;
                checkTimer = null;
            }

            host.SetObjectForUserDefaultsKey(DateTime.Now, SULastCheckTimeKey);

            driver = aDriver;
            driver.CheckForUpdatesAtURLWithHost(ParameterizedFeedURL(), host);

        }

        
        private string ParameterizedFeedURL() {
            return FeedURL;
            //TODO: Allow profile sending
        }

    }
}
