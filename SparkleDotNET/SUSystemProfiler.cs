using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Management;
using System.Reflection;
using KNFoundation;
using KNFoundation.KNKVC;

namespace SparkleDotNET {
    class SUSystemProfiler {

        public static List<Dictionary<string, string>> SystemProfileForHost(SUHost host) {

            List<Dictionary<string, string>> profile = new List<Dictionary<string, string>>();

            // App name

            profile.Add(DictionaryForProfileItem("appName", SULocalizedStrings.StringForKey("Application Name"), host.Name, host.Name));

            // App version

            profile.Add(DictionaryForProfileItem("appVersion", SULocalizedStrings.StringForKey("Application Version"), host.Version, host.Version));

            // System version

            string version = Environment.OSVersion.ToString();
            profile.Add(DictionaryForProfileItem("osVersion", SULocalizedStrings.StringForKey("OS Version"), version, version));

            // .NET version

            string frameworkVersion = Environment.Version.ToString();
            profile.Add(DictionaryForProfileItem("dotNetVersion", SULocalizedStrings.StringForKey(".NET Version"), frameworkVersion, frameworkVersion));

            // 64-bit?

            if (Helpers.Is64BitOperatingSystem) {
                profile.Add(DictionaryForProfileItem("cpu64bit", SULocalizedStrings.StringForKey("CPU is 64-Bit?"), "1", SULocalizedStrings.StringForKey("Yes")));
            } else {
                profile.Add(DictionaryForProfileItem("cpu64bit", SULocalizedStrings.StringForKey("CPU is 64-Bit?"), "0", SULocalizedStrings.StringForKey("No")));
            }

            // CPU Count

            profile.Add(DictionaryForProfileItem("ncpu", SULocalizedStrings.StringForKey("Number of CPUs"), Environment.ProcessorCount.ToString(), Environment.ProcessorCount.ToString()));

            using (ManagementObject mObj = new ManagementObject("Win32_Processor.DeviceID='CPU0'")) {

                // CPU Speed
                uint sp = (uint)mObj["CurrentClockSpeed"];
                profile.Add(DictionaryForProfileItem("cpuFreqMHz", SULocalizedStrings.StringForKey("CPU Speed (GHz)"), sp.ToString(), ((double)sp / 1000.0).ToString()));


                // CPU Type

                // A problem with this is that Mac OS X and Windows have different values for this; on Windows this is
                // the processor architecture, and on OS X it is the CPU type.
                // Windows: http://msdn.microsoft.com/en-us/library/aa394373%28VS.85%29.aspx
                // OS X: http://www.opensource.apple.com/source/xnu/xnu-1228.12.14/osfmk/mach/machine.h

                ushort arch = (ushort)mObj["Architecture"];
                int archSparkle = -1;
                string archDisplay = "Unknown";
                switch (arch)
                {
                    case 0:
                        archSparkle = 7;
                        archDisplay = "x86";
                        break;
                    case 1:
                        archSparkle = 8;
                        archDisplay = "MIPS";
                        break;
                    case 2:
                        archSparkle = 16;
                        archDisplay = "Alpha";
                        break;
                    case 3:
                        archSparkle = 3;
                        archDisplay = "PowerPC";
                        break;
                    case 9:
                        archSparkle = 7;
                        archDisplay = "x86_64";
                        break;
                }

                profile.Add(DictionaryForProfileItem("cputype", SULocalizedStrings.StringForKey("CPU Type"), archSparkle.ToString(), archDisplay));


                // CPU Subtype

                // Until someone wants to write something that will work this out, let's just send the equivalent to
                // the OS X _ALL. Actually, that's probably accurate, since .NET was designed to be portable.

                profile.Add(DictionaryForProfileItem("cpusubtype", SULocalizedStrings.StringForKey("CPU Subtype"), "0", "0"));
            }

            // RAM

            ulong installedMemory = 0;
            var memStatus = new NativeMethods.MEMORYSTATUSEX();
            if (NativeMethods.GlobalMemoryStatusEx(memStatus)) {
                installedMemory = memStatus.ullTotalPhys / 1024 / 1024;
            }

            profile.Add(DictionaryForProfileItem("ramMB", SULocalizedStrings.StringForKey("Memory (MB)"), installedMemory.ToString(), installedMemory.ToString()));

            // User preferred language

            profile.Add(DictionaryForProfileItem("lang", SULocalizedStrings.StringForKey("Preferred Language"), CultureInfo.CurrentCulture.TwoLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName));

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
