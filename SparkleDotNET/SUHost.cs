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

        public BitmapSource LargeIcon {
            get {
                return bundle.LargeBundleIcon;
            }
        }

        public string FeedURL {
            get {

                object url = ObjectForUserDefaultsKey(SUConstants.SUFeedURLKey);

                if (url != null) {
                    return (string)url;
                }

                return (string)bundle.InfoDictionary.ValueForKey(SUConstants.SUFeedURLKey);
            }

            set {
                SetObjectForUserDefaultsKey(value, SUConstants.SUFeedURLKey);
            }
        }

        public double UpdateCheckInterval {
            get {
                object value = ObjectForUserDefaultsKey(SUConstants.SUScheduledCheckIntervalKey);

                if (value != null) {
                    return (double)value;
                }

                value = bundle.InfoDictionary.ValueForKey(SUConstants.SUScheduledCheckIntervalKey);

                if (value != null) {
                    return (double)value;
                }

                return SUConstants.SU_DEFAULT_CHECK_INTERVAL;
            }
            set {
                SetObjectForUserDefaultsKey(value, SUConstants.SUScheduledCheckIntervalKey);
            }
        }

        public DateTime LastUpdateCheckDate {
            get {
                object value = ObjectForUserDefaultsKey(SUConstants.SULastCheckTimeKey);
                if (value != null) {
                    return (DateTime)value;
                }

                return DateTime.MinValue;
            }
            set {
                SetObjectForUserDefaultsKey(value, SUConstants.SULastCheckTimeKey);
            }
        }


        public string PublicDSAKey {
            get {
                string key = (string)bundle.InfoDictionary.ValueForKey(SUConstants.SUPublicDSAKeyKey);
                if (!string.IsNullOrWhiteSpace(key)) {
                    return key;
                }

                string keyFile = (string)bundle.InfoDictionary.ValueForKey(SUConstants.SUPublicDSAKeyFileKey);
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
