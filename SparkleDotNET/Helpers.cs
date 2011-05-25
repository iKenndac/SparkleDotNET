using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SparkleDotNET
{
    static class Helpers
    {
        public static bool StringIsNullOrWhiteSpace(string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        public static bool Is64BitOperatingSystem
        {
            get { return Environment.Is64BitOperatingSystem; }
        }
    }
}
