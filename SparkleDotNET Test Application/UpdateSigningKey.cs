using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using KNFoundation.KNKVC;

namespace SparkleDotNET_Test_Application {
    public class UpdateSigningKey {

        private string name;
        private byte[] publicKey;
        private byte[] privateKey;

        public UpdateSigningKey(Dictionary<string, object> plistRep) {

            Name = (string)plistRep.ValueForKey("Name");
            PublicKey = Convert.FromBase64String((string)plistRep.ValueForKey("PublicKey"));
            PrivateKey = Convert.FromBase64String((string)plistRep.ValueForKey("PrivateKey"));
        }

        public UpdateSigningKey() {

            DSACryptoServiceProvider provider = new DSACryptoServiceProvider();
            PrivateKey = provider.ExportCspBlob(true);
            PublicKey = provider.ExportCspBlob(false);

            Name = "New Key";
        }

        public string Name {
            get { return name; }
            set {
                this.WillChangeValueForKey("Name");
                name = value;
                this.DidChangeValueForKey("Name");
            }
        }

        public byte[] PrivateKey {
            get { return privateKey; }
            set {
                this.WillChangeValueForKey("PrivateKey");
                privateKey = value;
                this.DidChangeValueForKey("PrivateKey");
            }
        }

        public byte[] PublicKey {
            get { return publicKey; }
            set {
                this.WillChangeValueForKey("PublicKey");
                publicKey = value;
                this.DidChangeValueForKey("PublicKey");
            }
        }

        public Dictionary<string, object> PlistRepresentation() {

            Dictionary<string, object> dict = new Dictionary<string, object>();

            dict.SetValueForKey(Name, "Name");
            dict.SetValueForKey(Convert.ToBase64String(PublicKey), "PublicKey");
            dict.SetValueForKey(Convert.ToBase64String(PrivateKey), "PrivateKey");

            return dict;

        }

    }
}
