using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KNFoundation;
using System.Reflection;

namespace SparkleDotNET {
    class SULocalizedStrings {

        static private KNBundle bundle = KNBundle.BundleWithAssembly(Assembly.GetAssembly(typeof(SULocalizedStrings)));

        public static string StringForKey(string key) {
            return KNBundleGlobalHelpers.KNLocalizedStringFromTableInBundle(key, "SparkleDotNET.Strings.SparkleStrings", bundle, "");  
        }

    }
}
