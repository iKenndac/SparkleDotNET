using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.Xml;
using System.Globalization;
using KNFoundation.KNKVC;

namespace SparkleDotNET {

    public interface SUAppcastDelegate {
        void AppcastDidFinishLoading(SUAppcast anAppcast);
        void AppCastFailedToLoadWithError(SUAppcast anAppcast, Exception anError);
    }

    public class SUAppcast {

        ArrayList items;
        string userAgentString;
        SUAppcastDelegate appcastDelegate;

        public SUAppcast() {
            userAgentString = "SparkleDotNET";
        }

        public string UserAgentString {
            get { return userAgentString; }
            set {
                this.WillChangeValueForKey("UserAgentString");
                userAgentString = value;
                this.DidChangeValueForKey("UserAgentString");
            }
        }

        public SUAppcastDelegate Delegate {
            get { return appcastDelegate; }
            set {
                this.WillChangeValueForKey("Delegate");
                appcastDelegate = value;
                this.DidChangeValueForKey("Delegate");
            }
        }

        public ArrayList Items {
            get { return items; }
            private set {
                this.WillChangeValueForKey("Items");
                items = value;
                this.DidChangeValueForKey("Items");
            }
        }

        // ----

        public void FetchAppcastFromURL(Uri url) {

            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.UserAgent, UserAgentString);
            client.Encoding = UTF8Encoding.UTF8;
            client.DownloadStringCompleted += AppcastWasDownloaded;
            client.DownloadStringAsync(url);

        }

        private void AppcastWasDownloaded(Object sender, DownloadStringCompletedEventArgs e) {

            if (e.Error != null) {
                ReportError(e.Error);
            } else {

                XmlNodeList xmlItems = null;

                try {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(e.Result);
                    xmlItems = doc.SelectNodes("/rss/channel/item");
                } catch (Exception ex) {
                    ReportError(ex);
                    return;
                }

                ArrayList appcastItems = new ArrayList();

                foreach (XmlNode node in xmlItems) {

                    Dictionary<string, ArrayList> nodesDict = new Dictionary<string, ArrayList>();
                    Dictionary<string, Object> itemDescription = new Dictionary<string, object>();

                    // Create a dictionary of nodes for each name present,
                    // so we can parse by xml:lang later.

                    foreach (XmlNode childNode in node.ChildNodes) {

                        string nodeName = childNode.Name;
                        ArrayList nodesForName = null;
                        if (!nodesDict.TryGetValue(nodeName, out nodesForName)) {
                            nodesForName = new ArrayList();
                            nodesDict.Add(nodeName, nodesForName);
                        }
                        nodesForName.Add(childNode);
                    }


                    foreach (string itemKey in nodesDict.Keys) {

                        ArrayList nodes = null;
                        nodesDict.TryGetValue(itemKey, out nodes);

                        XmlNode bestNodeForKey = BestNodeInNodes(nodes);

                        if (bestNodeForKey.Name.Equals("enclosure")) {
                            // enclosure is flattened as a separate dictionary for some reason
                            Dictionary<string, string> enclosureDict = new Dictionary<string, string>();

                            foreach (XmlAttribute attribute in bestNodeForKey.Attributes) {
                                enclosureDict.SetValueForKey(attribute.InnerText, attribute.Name);
                            }
                            itemDescription.SetValueForKey(enclosureDict, "enclosure");

                        } else if (bestNodeForKey.Name.Equals("pubDate")) {
                            try {
                                DateTime date = DateTime.Parse(bestNodeForKey.InnerText);
                                itemDescription.SetValueForKey(date, bestNodeForKey.Name);
                            } catch {
                                // Nothing
                            }
                        } else {
                            itemDescription.SetValueForKey(bestNodeForKey.InnerText.Trim(), bestNodeForKey.Name);

                        }
                    }

                    try {
                        SUAppcastItem item = new SUAppcastItem(itemDescription);
                        appcastItems.Add(item);

                    } catch {
                    }
                }

                appcastItems.Sort();
                Items = appcastItems;

                if (Delegate != null) {
                    Delegate.AppcastDidFinishLoading(this);
                }
            }
        }

        private XmlNode BestNodeInNodes(ArrayList nodes) {

            if (nodes.Count == 0) {
                return null;
            } else if (nodes.Count == 1) {
                return (XmlNode)nodes[0];
            } else {

                CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

                foreach (XmlNode node in nodes) {
                    foreach (XmlAttribute attrib in node.Attributes) {
                        if (attrib.Name.Equals("xml:lang")) {

                            string lang = attrib.InnerText;
                            if (lang.Equals(currentCulture.Name, StringComparison.CurrentCultureIgnoreCase) ||
                                lang.Equals(currentCulture.EnglishName, StringComparison.CurrentCultureIgnoreCase) ||
                                lang.Equals(currentCulture.TwoLetterISOLanguageName, StringComparison.CurrentCultureIgnoreCase)) {
                                return node;
                            }

                            if (currentCulture.Parent != null) {

                                // Search parent, so for example en-GB or en-US will match en, English, etc.
                                // This algorithm isn't smart, so put your specific languages (en-GB) before
                                // general languages (en).

                                if (lang.Equals(currentCulture.Parent.Name, StringComparison.CurrentCultureIgnoreCase) ||
                                    lang.Equals(currentCulture.Parent.EnglishName, StringComparison.CurrentCultureIgnoreCase) ||
                                    lang.Equals(currentCulture.Parent.TwoLetterISOLanguageName, StringComparison.CurrentCultureIgnoreCase)) {
                                    return node;
                                }
                            }
                        }
                    }
                }

                return (XmlNode)nodes[0];
            }

        }

        private void ReportError(Exception e) {
            if (Delegate != null) {
                Delegate.AppCastFailedToLoadWithError(this, e);
            }
        }

    }
}
