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
            return String.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        public static bool Is64BitOperatingSystem
        {
            get { return GetPlatform() == Platform.X64; }
        }

        enum Platform
        {
            X86,
            X64,
            Unknown
        }

        const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        static Platform GetPlatform()
        {
            SYSTEM_INFO sysInfo = new SYSTEM_INFO();

            if (System.Environment.OSVersion.Version.Major > 5 ||
          (System.Environment.OSVersion.Version.Major == 5 && System.Environment.OSVersion.Version.Minor >= 1))
            {
                GetNativeSystemInfo(ref sysInfo);
            }
            else
            {
                GetSystemInfo(ref sysInfo);
            }

            switch (sysInfo.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_IA64:
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return Platform.X64;

                case PROCESSOR_ARCHITECTURE_INTEL:
                    return Platform.X86;

                default:
                    return Platform.Unknown;
            }
        }
    }
}
