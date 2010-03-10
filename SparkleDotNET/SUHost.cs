using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    public class SUHost {

        public const string SUPublicDSAKeyKey = "SUPublicDSAKey";
        public const string SUPublicDSAKeyFileKey = "SUPublicDSAKeyFile";

        KNBundle bundle;
        KNUserDefaults defaults;

        public SUHost(KNBundle aBundle) {
            bundle = aBundle;
            defaults = KNUserDefaults.UserDefaultsForDomain(bundle.BundleIdentifier);

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

        public string Name {
            get {
                if (bundle.DisplayName != null) {
                    return bundle.DisplayName;
                }
                if (bundle.Name != null) {
                    return bundle.Name;
                }
                return Path.GetDirectoryName(bundle.BundlePath);
            }
        }

        public BitmapSource Icon {
            get {
                return bundle.BundleIcon;
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

        public object ObjectForUserDefaultsKey(string key) {
            if (defaults != null) {
                return defaults.ObjectForKey(key);
            } else {
                return null;
            }
        }

        public void SetObjectForUserDefaultsKey(object value, string key) {
            if (defaults != null) {
                defaults.SetObjectForKey(value, key);
            }
        }
      
    }
}
