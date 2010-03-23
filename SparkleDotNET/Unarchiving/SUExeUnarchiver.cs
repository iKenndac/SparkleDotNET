using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkleDotNET {
    class SUExeUnarchiver : SUUnarchiver {


        public override void Start(SUAppcastItem item, string path) {

            // This is a bit of a hack - SparkleDotNET expects to have 
            // to extract everything it downloads. This isn't the case 
            // for .exe files.

            if (Delegate != null) {
                Delegate.UnarchiverDidFinish(this, path);
            }

        }
    }
}
