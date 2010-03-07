using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUHost {

        public const string SUPublicDSAKeyKey = "SUPublicDSAKeyFileKey";
        public const string SUPublicDSAKeyFileKey = "SUPublicDSAKeyFile";

        KNBundle bundle;

        public SUHost(KNBundle aBundle) {
            bundle = aBundle;
        }

        public string SystemVersionString {
            get { return Environment.OSVersion.Version.ToString(); }
        }

        public string Version {
            get { return bundle.Version; }
        }

        public string DisplayVersion {
            get {
                if (bundle.ShortVersionString != null) {
                    return bundle.ShortVersionString;
                } else {
                    return Version;
                }
            }
        }

        public string PublicDSAKey {
            get {
                string key = (string)bundle.InfoDictionary.ValueForKey(SUPublicDSAKeyKey);
                if (!string.IsNullOrWhiteSpace(key)) {
                    return key;
                }

                string keyFile = (string)bundle.InfoDictionary.ValueForKey(SUPublicDSAKeyFileKey);
                if (!string.IsNullOrWhiteSpace(keyFile)) {
                    string keyFilePath = (string)bundle.PathForResourceOfType(keyFile, null);
                    if (!string.IsNullOrWhiteSpace(keyFilePath) && File.Exists(keyFilePath)) {
                        return File.ReadAllText(keyFilePath, Encoding.UTF8);
                    }
                }

                return null;
            }
        }
      
    }
}
