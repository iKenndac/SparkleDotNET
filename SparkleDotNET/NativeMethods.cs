using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SparkleDotNET
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
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
        }

        /// <summary>
        /// To retrieve accurate information for an application running on WOW64, call the GetNativeSystemInfo function.
        /// </summary>
        /// <param name="lpSystemInfo">A reference to a SYSTEM_INFO structure that receives the information.</param>
        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);
        
        /// <summary>
        /// Retrieves information about the current system to an application running under WOW64.
        /// If the function is called from a 64-bit application, it is equivalent to the GetSystemInfo function.
        /// </summary>
        /// <param name="lpSystemInfo">A reference to a SYSTEM_INFO structure that receives the information.</param>
        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        /// <summary>
        /// Determines whether the specified process is running under WOW64.
        /// </summary>
        /// <param name="processHandle">A handle to the process.
        /// The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right.</param>
        /// <param name="wow64Process">A pointer to a value that is set to TRUE if the process is running under WOW64.
        /// If the process is running under 32-bit Windows, the value is set to FALSE.
        /// If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr processHandle,
             [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        /// <summary>
        /// Abstracts the retrieval of a SYSTEM_INFO structure.
        /// </summary>
        /// <param name="lpSystemInfo">A reference to a SYSTEM_INFO structure that receives the information.</param>
        public static void GetSystemInfoAbstracted(ref SYSTEM_INFO lpSystemInfo)
        {
            if (System.Environment.OSVersion.Version.Major > 5 || 
                (System.Environment.OSVersion.Version.Major == 5 && System.Environment.OSVersion.Version.Minor >= 1))
            {
                GetNativeSystemInfo(ref lpSystemInfo);
            }
            else
            {
                GetSystemInfo(ref lpSystemInfo);
            }
        }
    }
}
