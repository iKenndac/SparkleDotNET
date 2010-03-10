using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SparkleDotNET {
    class SUDSAVerifier {


        public static bool ValidatePathWithEncodedDSASignatureAndPublicDSAKey(string path, string base64Signature, string publicKey) {

            try {

                byte[] signature = Convert.FromBase64String(base64Signature);
                byte[] data = File.ReadAllBytes(path);
                SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
                byte[] sha1Hash = cryptoTransformSHA1.ComputeHash(data);
                string cleanKey = "";

                string[] lines = publicKey.Split(new char[] {'\n', '\r'});

                foreach (string line in lines) {
                        cleanKey += line.Trim();
                }

                byte[] publicKeyData = Convert.FromBase64String(cleanKey);

                DSACryptoServiceProvider provider = new DSACryptoServiceProvider();
                provider.ImportCspBlob(publicKeyData);
                DSASignatureDeformatter formatter = new DSASignatureDeformatter(provider);
                formatter.SetHashAlgorithm("SHA1");
                return formatter.VerifySignature(sha1Hash, signature);

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
            }

        }


        public static byte[] DSASignHash(byte[] HashToSign, DSAParameters DSAKeyInfo, string HashAlg) {
            try {
                //Create a new instance of DSACryptoServiceProvider.
                DSASignatureFormatter DSAFormatter;
                using (DSACryptoServiceProvider DSA = new DSACryptoServiceProvider()) {


                    //Import the key information.   
                    DSA.ImportParameters(DSAKeyInfo);

                    //Create an DSASignatureFormatter object and pass it the 
                    //DSACryptoServiceProvider to transfer the private key.
                    DSAFormatter = new DSASignatureFormatter(DSA);
                }

                //Set the hash algorithm to the passed value.
                DSAFormatter.SetHashAlgorithm(HashAlg);

                //Create a signature for HashValue and return it.
                return DSAFormatter.CreateSignature(HashToSign);
            } catch (CryptographicException e) {
                Console.WriteLine(e.Message);

                return null;
            }

        }

    }
}
