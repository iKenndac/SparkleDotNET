using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    public class SUAppcastItem : IComparable<SUAppcastItem> {

        public SUAppcastItem(Dictionary<string, object> dict) {

            Dictionary<string, string> enclosure = (Dictionary<string, string>)dict.ValueForKey("enclosure");
            string newVersion = (string)enclosure.ValueForKey("sparkle:version");

            // Todo: Parse version number from filename

            if (enclosure == null || enclosure.ValueForKey("url") == null || newVersion == null) {
                throw new Exception("Item doesn't contain version information.");
            } else {

                Title = (string)dict.ValueForKey("title");
                Date = (DateTime)dict.ValueForKey("pubDate");
                ItemDescription = (string)dict.ValueForKey("description");
                FileURL = new Uri((string)dict.ValueForKey("url"));
                DSASignature = (string)enclosure.ValueForKey("sparkle:dsaSignature");
                VersionString = newVersion;
                MinimumSystemVersion = (string)enclosure.ValueForKey("sparkle:minimumSystemVersion");

                string shortVersionString = (string)enclosure.ValueForKey("sparkle:shortVersionString");
                if (shortVersionString != null) {
                    DisplayVersionString = shortVersionString;
                } else {
                    DisplayVersionString = VersionString;
                }

                if (dict.ContainsKey("sparkle:releaseNotesLink")) {
                    ReleaseNotesURL = new Uri((string)dict.ValueForKey("sparkle:releaseNotesLink"));
                } else if (ItemDescription.Substring(0, 7).Equals("http://")) {
                    ReleaseNotesURL = new Uri(ItemDescription);
                }
            }
        }

        private string title;
        private string versionString;
        private string displayVersionString;
        private DateTime date;
        private string itemDescription;
        private Uri releaseNotesURL;
        private Uri fileURL;
        private string dsaSignature;
        private string minimumSystemVersion;

        public Uri FileURL {
            get { return fileURL; }
            private set {
                this.WillChangeValueForKey("FileURL");
                fileURL = value;
                this.DidChangeValueForKey("FileURL");
            }
        }

        public Uri ReleaseNotesURL {
            get { return releaseNotesURL; }
            private set {
                this.WillChangeValueForKey("ReleaseNotesURL");
                releaseNotesURL = value;
                this.DidChangeValueForKey("ReleaseNotesURL");
            }
        }

        public string MinimumSystemVersion {
            get { return minimumSystemVersion; }
            private set {
                this.WillChangeValueForKey("MinimumSystemVersion");
                minimumSystemVersion = value;
                this.DidChangeValueForKey("MinimumSystemVersion");
            }
        }

        public string DSASignature {
            get { return dsaSignature; }
            private set {
                this.WillChangeValueForKey("DSASignature");
                dsaSignature = value;
                this.DidChangeValueForKey("DSASignature");
            }
        }

        public string ItemDescription {
            get { return ItemDescription; }
            private set {
                this.WillChangeValueForKey("ItemDescription");
                itemDescription = value;
                this.DidChangeValueForKey("ItemDescription");
            }
        }

        public DateTime Date {
            get { return date; }
            private set {
                this.WillChangeValueForKey("Date");
                date = value;
                this.DidChangeValueForKey("Date");
            }
        }

        public string DisplayVersionString {
            get { return displayVersionString; }
            private set {
                this.WillChangeValueForKey("DisplayVersionString");
                displayVersionString = value;
                this.DidChangeValueForKey("DisplayVersionString");
            }
        }

        public string VersionString {
            get { return versionString; }
            private set {
                this.WillChangeValueForKey("VersionString");
                versionString = value;
                this.DidChangeValueForKey("VersionString");
            }
        }

        public string Title {
            get { return title; }
            private set {
                this.WillChangeValueForKey("Title");
                title = value;
                this.DidChangeValueForKey("Title");
            }
        }

        #region IComparable<SUAppcastItem> Members

        public int CompareTo(SUAppcastItem other) {

            if (other.Date != null && Date != null) {
                return Date.CompareTo(other.date);
            } else {
                return 0;
            }

        }

        #endregion
    }
}
