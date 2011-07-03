/// <summary>
/// Авторские права
/// Содержащаяся здесь информация является собственностью ООО НПФ "Гранч" и
/// представляет собой коммерческую тайну ООО НПФ "Гранч", или его лицензией,
/// и является предметом ограничений на использование и раскрытие информации.

/// Copyright (c) 2009, 2010 ООО НПФ "Гранч". Все права защищены.

/// Уведомления об авторских правах, указанные выше, не являются основанием и не
/// дают права для публикации данного материала.
/// </summary>
/// <author>Шаров Александр</author>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Granch.WindowsServiceToolkit
{
    #region Win32 Interop structs
    [StructLayout(LayoutKind.Sequential)]
    struct ProcessInformation
    {
        // These members must match PROCESS_BASIC_INFORMATION
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;
    }
    #endregion
    /// <summary>
    /// A utility class to determine the process' parent. Don't edit this.
    /// </summary>
    /// 
    static class ProcessUtilities
    {
        #region Win32 interop methods
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref ProcessInformation processInformation, int processInformationLength, out int returnLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetCurrentProcessId();
        #endregion
        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess()
        {
            return GetParentProcess(Process.GetCurrentProcess().Handle);
        }
        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(int id)
        {
            Process process = Process.GetProcessById(id);
            return GetParentProcess(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(IntPtr handle)
        {
            ProcessInformation pi = new ProcessInformation();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pi, Marshal.SizeOf(pi), out returnLength);
            if (status != 0)
            {
                throw new Win32Exception(status);
            }

            try
            {
                return Process.GetProcessById(pi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
    }
}
