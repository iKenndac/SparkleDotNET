using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNFoundation.KNKVC;
using KNFoundation;

namespace SparkleDotNET {
    abstract class SUUpdateDriver {

        SUHost host;
        SUUpdater updater;
        string appcastURL;
        bool finished;

        public SUUpdateDriver(SUUpdater anUpdater) {
            Updater = anUpdater;
        }

        virtual public void CheckForUpdatesAtURLWithHost(string aUrl, SUHost aHost) {
            Host = aHost;
            appcastURL = aUrl;
        }

        virtual public void AbortUpdate() {
            Finished = true;
            KNNotificationCentre.SharedCentre().PostNotificationWithName(SUConstants.SUUpdateDriverFinishedNotification, this);
        }

        public bool Finished {
            get { return finished; }
            private set {
                this.WillChangeValueForKey("Finished");
                finished = value;
                this.DidChangeValueForKey("Finished");
            }
        }

        public SUHost Host {
            get { return host; }
            set {
                this.WillChangeValueForKey("Host");
                host = value;
                this.DidChangeValueForKey("Host");
            }
        }

        public SUUpdater Updater {
            get { return updater; }
            set {
                this.WillChangeValueForKey("Updater");
                updater = value;
                this.DidChangeValueForKey("Updater");
            }
        }

    }
}
