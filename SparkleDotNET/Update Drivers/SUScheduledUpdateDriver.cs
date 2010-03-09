using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkleDotNET {
    class SUScheduledUpdateDriver : SUUIBasedUpdateDriver {

        private bool showsErrors;

        public SUScheduledUpdateDriver(SUUpdater anUpdater)
            : base(anUpdater) {
        }

        protected override void DidFindValidUpdate() {
            showsErrors = true; // Only show errors if we find an update
            base.DidFindValidUpdate();
        }

        protected override void DidNotFindUpdate() {
            AbortUpdate();
            // Don't tell the user that no update was found; this was a scheduled update.
        }

        protected override void AbortUpdateWithError(Exception error) {
            if (showsErrors) {
                base.AbortUpdateWithError(error);
            } else {
                AbortUpdate();
            }
        }
        
        
    }
}
