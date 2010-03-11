using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUUnarchiver {

        public interface SUUnarchiverDelegate {

            void UnarchiverExtractedLength(SUUnarchiver unarchiver, long length);
            void UnarchiverDidFinish(SUUnarchiver unarchiver);
            void UnarchiverDidFail(SUUnarchiver unarchiver);
        }

        public static SUUnarchiver UnarchiverForPath(string archivePath) {
        }

        private SUUnarchiverDelegate del;

        public abstract void Start() {
        }

        public SUUnarchiverDelegate Delegate {
            get { return del; }
            set {
                this.WillChangeValueForKey("Delegate");
                del = value;
                this.DidChangeValueForKey("Delegate");
            }
        }


    }
}
