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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using AVS.Tools;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// ServiceInstaller that supports failure actions and start on install.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    class AdvancedServiceInstaller : ServiceInstaller
    {
        #region Win32 Interop
        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_FAILURE_ACTIONS_FLAG
        {
            public bool bFailureAction;
        }

        // The struct for setting the service description
        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_DESCRIPTION
        {
            public string lpDescription;
        }

        // The struct for setting the service failure actions
        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_FAILURE_ACTIONS
        {
            public int dwResetPeriod;
            public string lpRebootMsg;
            public string lpCommand;
            public int cActions;
            public int lpsaActions;
        }

        // Win32 function to open the service control manager
        [DllImport("advapi32.dll")]
        static extern
            IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        // Win32 function to open a service instance
        [DllImport("advapi32.dll")]
        static extern IntPtr
            OpenService(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

        // Win32 function to lock the service database to perform write operations.
        [DllImport("advapi32.dll")]
        static extern IntPtr
            LockServiceDatabase(IntPtr hSCManager);

        // Win32 function to change the service config for the failure actions.
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        static extern bool
            ChangeServiceFailureActions(IntPtr hService, int dwInfoLevel,
            [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS lpInfo);

        // Win32 function to change the service config for the service description
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        static extern bool
            ChangeServiceDescription(IntPtr hService, int dwInfoLevel,
            [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION lpInfo);

        // Win32 function to close a service related handle.
        [DllImport("advapi32.dll")]
        static extern bool CloseServiceHandle(IntPtr hSCObject);

        // Win32 function to unlock the service database.
        [DllImport("advapi32.dll")]
        static extern bool UnlockServiceDatabase(IntPtr hSCManager);

        // The infamous GetLastError() we have all grown to love
        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        // Some Win32 constants

        const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        const int SERVICE_ALL_ACCESS = 0xF01FF;
        const int SERVICE_CONFIG_DESCRIPTION = 0x1;
        const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
        const int SERVICE_NO_CHANGE = -1;
        const int ERROR_ACCESS_DENIED = 5;

        #endregion

        #region Shutdown Privilege Interop
        // Struct required to set shutdown privileges
        [StructLayout(LayoutKind.Sequential)]
        struct LUID_AND_ATTRIBUTES
        {
            public long Luid;
            public int Attributes;
        }

        // Struct required to set shutdown privileges. The Pack attribute specified here
        // is important. We are in essence cheating here because the Privileges field is
        // actually a variable size array of structs.  We use the Pack=1 to align the Privileges
        // field exactly after the PrivilegeCount field when marshalling this struct to
        // Win32. You do not want to know how many hours I had to spend on this alone!!!

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        // This method adjusts privileges for this process which is needed when
        // specifying the reboot option for a service failure recover action.
        [DllImport("advapi32.dll")]
        static extern bool
            AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength,
           IntPtr PreviousState, ref int ReturnLength);


        // Looks up the privilege code for the privilege name
        [DllImport("advapi32.dll")]
        static extern bool
            LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        // Opens the security/privilege token for a process handle
        [DllImport("advapi32.dll")]
        static extern bool
            OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        // Gets the current process handle
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        // Close a windows handle
        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hndl);

        // Token adjustment stuff
        const int TOKEN_ADJUST_PRIVILEGES = 32;
        const int TOKEN_QUERY = 8;
        const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        const int SE_PRIVILEGE_ENABLED = 2;
        #endregion

        #region For Windows Server 2008
        const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x4;

        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]
        static extern bool
        ChangeServiceFailureActionFlag(IntPtr hService, int dwInfoLevel,
        [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS_FLAG lpInfo);

        #endregion
        /// <summary>
        /// Constructor
        /// </summary>
        public AdvancedServiceInstaller()
            : base()
        {
            FailureActions = new List<FailureAction>();
            FailCountResetTime = SERVICE_NO_CHANGE;
            StartTimeoutSeconds = 15;
            StartOnInstall = true;

            Committed += new InstallEventHandler(UpdateServiceConfig);
            Committed += new InstallEventHandler(StartIfNeeded);
        }

        #region Properties
        /// <summary>
        /// List of failure actions.
        /// Services.msc displays only first 3, but there can be as many as you like.
        /// </summary>
        public List<FailureAction> FailureActions
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property for setting the service description
        /// </summary>
        public new string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set fail count reset time 
        /// </summary>
        public int FailCountResetTime
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set fail reboot msg
        /// </summary>
        public string FailureRebootMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set fail run command.
        /// </summary>
        public SystemCommand FailureCommandAction
        {
            get;
            set;
        }

        /// <summary>
        /// Property style access to configure the service to start on install
        /// </summary>
        public bool StartOnInstall
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set the start timeout for the service.
        /// </summary>
        public int StartTimeoutSeconds
        {
            get;
            set;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Format service message (uses LogFormat).
        /// </summary>
        /// <param name="msg">Message string</param>
        /// <returns></returns>
        string Format(Exception e)
        {
            return string.Format("{0}:\r\n{1}", e.Message, e.StackTrace);
        }
        /// <summary>
        /// Log to console and OS Event log.
        /// </summary>
        /// <param name="logLevel">log level</param>
        /// <param name="msg">message</param>
        void LogInstallMessage(EventLogEntryType logLevel, string msg)
        {
            Debug.WriteLine(msg);
            try
            {
                EventLog.WriteEntry(GetType().Name, msg, logLevel);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// Request privileges for creating shutdown on failure actions.
        /// </summary>
        /// <returns></returns>
        bool GrantShutdownPrivilege()
        {
            // This code mimics the MSDN defined way to adjust privilege for shutdown
            // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/shutting_down.asp

            bool success = false;
            IntPtr processToken = IntPtr.Zero;
            try
            {
                var currentProcess = GetCurrentProcess();
                if (!OpenProcessToken(currentProcess, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref processToken))
                {
                    return false;
                }
                long Luid = 0;
                LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref Luid);
                var tkp = new TOKEN_PRIVILEGES();
                tkp.PrivilegeCount = 1;
                tkp.Privileges.Luid = Luid;
                tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
                int retLen = 0;
                AdjustTokenPrivileges(processToken, false, ref tkp, 0, IntPtr.Zero, ref retLen);
                if (GetLastError() != 0)
                {
                    throw new Exception("Failed to grant shutdown privilege");
                }
                success = true;
            }
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, Format(ex));
            }
            finally
            {
                if (processToken != IntPtr.Zero)
                {
                    CloseHandle(processToken);
                }
            }
            return success;
        }

        #endregion

        #region Post-install handlers
        /// <summary>
        /// Update installed service's config accordinly.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void UpdateServiceConfig(object sender, InstallEventArgs e)
        {
            IntPtr scmHndl = IntPtr.Zero;
            IntPtr svcHndl = IntPtr.Zero;
            IntPtr tmpBuf = IntPtr.Zero;
            IntPtr svcLock = IntPtr.Zero;

            try
            {
                // Open the service control manager
                scmHndl = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
                if (scmHndl.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error,
                        "Failed to Open Service Control Manager");
                    return;
                }

                // Lock the Service Database
                svcLock = LockServiceDatabase(scmHndl);
                if (svcLock.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error,
                        "Failed to Lock Service Database for Write");
                    return;
                }

                // Open the service
                svcHndl = OpenService(scmHndl, ServiceName, SERVICE_ALL_ACCESS);
                if (svcHndl.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Information,
                        "Failed to Open Service ");
                    return;
                }

                // Need to set service failure actions. Note that the API lets us set as many as
                // we want, yet the Service Control Manager GUI only lets us see the first 3.
                // Also note that the API allows granularity of seconds whereas GUI only shows days and minutes.
                if (FailureActions.Count > 0)
                {
                    int[] actions = new int[FailureActions.Count * 2];
                    int i = 0;
                    bool needShutdownPrivilege = false;
                    foreach (FailureAction fa in FailureActions)
                    {
                        actions[i] = (int)fa.Type;
                        actions[++i] = fa.DelaySeconds * 1000;
                        i++;
                        if (fa.Type == FailureActionType.Reboot)
                        {
                            needShutdownPrivilege = true;
                        }
                    }
                    // If we need shutdown privilege, then grant it to this process
                    if (needShutdownPrivilege)
                    {
                        GrantShutdownPrivilege();
                    }

                    tmpBuf = Marshal.AllocHGlobal(FailureActions.Count * 8);
                    Marshal.Copy(actions, 0, tmpBuf, FailureActions.Count * 2);
                    SERVICE_FAILURE_ACTIONS sfa = new SERVICE_FAILURE_ACTIONS();
                    sfa.cActions = FailureActions.Count;
                    sfa.dwResetPeriod = FailCountResetTime;
                    if (FailureCommandAction != null)
                    {
                        sfa.lpCommand = FailureCommandAction.ToString();
                    }
                    sfa.lpRebootMsg = FailureRebootMessage;
                    sfa.lpsaActions = tmpBuf.ToInt32();

                    SERVICE_FAILURE_ACTIONS_FLAG sfaf = new SERVICE_FAILURE_ACTIONS_FLAG();
                    sfaf.bFailureAction = true;
                    if (!ChangeServiceFailureActionFlag(svcHndl, SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, ref sfaf))
                    {
                        throw new Win32Exception(GetLastError());
                    }

                    if (!ChangeServiceFailureActions(svcHndl, SERVICE_CONFIG_FAILURE_ACTIONS, ref sfa))
                    {
                        if (GetLastError() == ERROR_ACCESS_DENIED)
                        {
                            throw new Exception(
                                "Access Denied while setting Failure Actions");
                        }
                    }

                    Marshal.FreeHGlobal(tmpBuf); tmpBuf = IntPtr.Zero;
                    LogInstallMessage(EventLogEntryType.Information,
                        "Successfully configured Failure Actions");
                }

                if (Description != null && Description.Length > 0)
                {
                    SERVICE_DESCRIPTION sd = new SERVICE_DESCRIPTION();
                    sd.lpDescription = Description;
                    if (!ChangeServiceDescription(svcHndl, SERVICE_CONFIG_DESCRIPTION, ref sd))
                    {
                        throw new Exception("Failed to set description");
                    }
                    LogInstallMessage(EventLogEntryType.Information,
                        "Successfully set description");
                }
            }
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, Format(ex));
            }
            finally
            {
                if (scmHndl != IntPtr.Zero)
                {
                    // Unlock the service database
                    if (svcLock != IntPtr.Zero)
                    {
                        UnlockServiceDatabase(svcLock);
                        svcLock = IntPtr.Zero;
                    }
                    CloseServiceHandle(scmHndl);
                    scmHndl = IntPtr.Zero;
                }
                // Close the service handle
                if (svcHndl != IntPtr.Zero)
                {
                    CloseServiceHandle(svcHndl);
                    svcHndl = IntPtr.Zero;
                }
                // Free the memory
                if (tmpBuf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tmpBuf);
                    tmpBuf = IntPtr.Zero;
                }
            }
        }
        /// <summary>
        /// Start service, if StartOnInstall was specified.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void StartIfNeeded(object sender, InstallEventArgs e)
        {
            if (!StartOnInstall) { return; }
            try
            {
                InstallUtilities.StartService(ServiceName, StartTimeoutSeconds);
                LogInstallMessage(EventLogEntryType.Information,
                    "Service Started");
            }
            catch (Exception exc)
            {
                LogInstallMessage(EventLogEntryType.Error, Format(exc));
            }
        }
        #endregion
    }
}
