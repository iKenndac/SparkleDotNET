using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkleDotNET {
    class SUBasicUpdateDriver : SUUpdateDriver, SUAppcastDelegate {

        public const string SUNoUpdateError = "com.Sparkle.NoUpdate";

        protected SUAppcastItem updateItem;

        public SUBasicUpdateDriver(SUUpdater anUpdater)
            : base(anUpdater) {

        }

        public override void CheckForUpdatesAtURLWithHost(string aUrl, SUHost aHost) {
            base.CheckForUpdatesAtURLWithHost(aUrl, aHost);

            SUAppcast appcast = new SUAppcast();
            appcast.Delegate = this;
            //Todo: Set user agent properly.
            appcast.FetchAppcastFromURL(new Uri(aUrl));
            
        }

        private SUVersionComparison VersionComparator() {

            SUVersionComparison comparator = null;
            if (Updater.Delegate != null) {
                comparator = Updater.Delegate.VersionComparatorForUpdater(Updater);
            }
            if (comparator == null) {
                comparator = SUStandardVersionComparator.SharedComparator();
            }
            return comparator;
        }

        // ---

        public bool IsItemNewer(SUAppcastItem item) {
            return VersionComparator().CompareVersionToVersion(Host.Version, item.VersionString) < 0;
        }

        public bool HostSupportsItem(SUAppcastItem item) {
            if (String.IsNullOrEmpty(item.MinimumSystemVersion)) {
                return true;
            } else {
                return SUStandardVersionComparator.SharedComparator().CompareVersionToVersion(item.MinimumSystemVersion, Host.SystemVersionString) < 1;
            }
        }

        public bool ItemContainsSkippedVersion(SUAppcastItem item) {
            return false;
            // Todo: implement this
        }

        public bool ItemContainsValidUpdate(SUAppcastItem item) {
            return HostSupportsItem(item) && IsItemNewer(item) && !ItemContainsSkippedVersion(item);
        }

        #region SUAppcastDelegate Members

public void AppcastDidFinishLoading(SUAppcast anAppcast) {

            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidFinishLoadingAppcast(Updater, anAppcast);
            }

            SUAppcastItem item = null;

            if (Updater.Delegate != null) {
                item = Updater.Delegate.BestValidUpdateInAppcastForUpdater(anAppcast, Updater);
            }

            foreach (SUAppcastItem potentialItem in anAppcast.Items) {
                if (HostSupportsItem(potentialItem)) {
                    item = potentialItem;
                    break;
                }
            }

            updateItem = item;

            if (updateItem == null) {
                DidNotFindUpdate();
                return;
            }

            if (ItemContainsValidUpdate(updateItem)) {
                DidFindValidUpdate();
            } else {
                DidNotFindUpdate();
            }
        }

        public void AppCastFailedToLoadWithError(SUAppcast anAppcast, Exception anError) {
            AbortUpdateWithError(anError);
        }

        #endregion

        public virtual void DidFindValidUpdate() {
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidFindValidUpdate(Updater, updateItem);
            }
            DownloadUpdate();
        }

        public virtual void DidNotFindUpdate() {
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidNotFindUpdate(Updater);
            }
            AbortUpdateWithError(new Exception(SUNoUpdateError));
        }

        private void AbortUpdateWithError(Exception error) {

        }
        public override void AbortUpdate() {
            base.AbortUpdate();
        }

        private void DownloadUpdate() {
        }

    }


}
