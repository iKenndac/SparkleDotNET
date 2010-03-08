using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkleDotNET {
    class SUProbingUpdateDriver : SUBasicUpdateDriver {

        public SUProbingUpdateDriver(SUUpdater anUpdater)
            : base(anUpdater) {
        }

        public override void DidFindValidUpdate() {
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidFindValidUpdate(Updater, updateItem);
            }
            AbortUpdate();
        }

        public override void DidNotFindUpdate() {
            if (Updater.Delegate != null) {
                Updater.Delegate.UpdaterDidNotFindUpdate(Updater);
            }
            AbortUpdate();
        }

    }
}
