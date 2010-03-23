using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Reflection;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUSystemProfiler {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX() {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public static ArrayList SystemProfileForHost(SUHost host) {

            ArrayList profile = new ArrayList();

            // App name

            profile.Add(DictionaryForProfileItem("appName", SULocalizedStrings.StringForKey("Application Name"), host.Name, host.Name));

            // App version

            profile.Add(DictionaryForProfileItem("appVersion", SULocalizedStrings.StringForKey("Application Version"), host.Version, host.Version));

            // System version

            string version = Environment.OSVersion.ToString();
            profile.Add(DictionaryForProfileItem("osVersion", SULocalizedStrings.StringForKey("OS Version"), version, version));

            //.NET version

            string frameworkVersion = Environment.Version.ToString();
            profile.Add(DictionaryForProfileItem("dotNetVersion", SULocalizedStrings.StringForKey(".NET Version"), frameworkVersion, frameworkVersion));

            // 64-bit?

            if (Environment.Is64BitOperatingSystem) {
                profile.Add(DictionaryForProfileItem("cpu64bit", SULocalizedStrings.StringForKey("CPU is 64-Bit?"), "true", SULocalizedStrings.StringForKey("Yes")));
            } else {
                profile.Add(DictionaryForProfileItem("cpu64bit", SULocalizedStrings.StringForKey("CPU is 64-Bit?"), "false", SULocalizedStrings.StringForKey("No")));
            }

            // CPU Count

            profile.Add(DictionaryForProfileItem("ncpu", SULocalizedStrings.StringForKey("Number of CPUs"), Environment.ProcessorCount.ToString(), Environment.ProcessorCount.ToString()));

            // RAM

            ulong installedMemory = 0;
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus)) {
                installedMemory = memStatus.ullTotalPhys / 1024 / 1024;
            }

            profile.Add(DictionaryForProfileItem("ramMB", SULocalizedStrings.StringForKey("Memory (MB)"), installedMemory.ToString(), installedMemory.ToString()));

            return profile;
        }


        private static Dictionary<string, string> DictionaryForProfileItem(string key, string displayKey, string value, string displayValue) {

            Dictionary<string, string> item = new Dictionary<string, string>();

            item.SetValueForKey(key, SUConstants.SUProfileItemKeyKey);
            item.SetValueForKey(displayKey, SUConstants.SUProfileItemDisplayKeyKey);
            item.SetValueForKey(value, SUConstants.SUProfileItemValueKey);
            item.SetValueForKey(displayValue, SUConstants.SUProfileItemDisplayValueKey);

            return item;
        }

    }
}
