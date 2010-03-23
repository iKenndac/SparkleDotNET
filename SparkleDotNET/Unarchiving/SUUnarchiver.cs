using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KNFoundation.KNKVC;

namespace SparkleDotNET {

    public interface SUUnarchiverDelegate {
        void UnarchiverDidFinish(SUUnarchiver unarchiver, string extractedFilesPath);
        void UnarchiverDidFail(SUUnarchiver unarchiver);
    }
    
    public class SUUnarchiver {

        private static Dictionary<string, SUUnarchiver> unarchiverCache;

        public static void AddUnarchiverForFileType(SUUnarchiver unarchiver, string fileType) {

            if (unarchiverCache == null) {
                unarchiverCache = new Dictionary<string, SUUnarchiver>();
            }
            unarchiverCache.SetValueForKey(unarchiver, fileType);
        }

        public static SUUnarchiver UnarchiverForPath(string archivePath) {
            if (unarchiverCache != null && unarchiverCache.ContainsKey(Path.GetExtension(archivePath))) {
                return (SUUnarchiver)unarchiverCache.ValueForKey(Path.GetExtension(archivePath));
            }
            return null;
        }

        private SUUnarchiverDelegate del;

        public virtual void Start(SUAppcastItem item, string path) {
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
