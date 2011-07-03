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
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace Granch.WindowsServiceToolkit {
    /// <summary>
    /// Набор методов, упрощающих некоторые задачи при установке служб.
    /// </summary>
    static class InstallUtilities {
        /// <summary>
        /// Get services installed by specified assembly.
        /// </summary>
        /// <param name="assemblyPath">Assembly path</param>
        /// <returns>service names</returns>
        public static IEnumerable<string> GetServicesInAssembly(string assemblyPath) {
            AssemblyInstaller assemblyInstaller = new AssemblyInstaller(assemblyPath, new string[] { });
            return GetServices(assemblyInstaller);
        }
        /// <summary>
        /// Get services installed by specified installer.
        /// </summary>
        /// <param name="installer">installer</param>
        /// <returns>service names</returns>
        static IEnumerable<string> GetServices(Installer installer) {
            foreach (var i in installer.Installers) {
                if (i is ServiceInstaller) {
                    yield return (i as ServiceInstaller).ServiceName;
                }
            }
        }
        /// <summary>
        /// Uninstall the service that is contained in specified assembly.
        /// </summary>
        /// <param name="pathToAssembly">Path to service assembly.</param>
        /// <param name="arguments">Parameters, that are passed to assembly.</param>
        public static void UninstallAssembly(string pathToAssembly, string[] arguments) {
            arguments = arguments.Concat(new string[] {
                string.Format("/LogFile={0}_uninstall.log", 
                Path.GetFileNameWithoutExtension(pathToAssembly))}).ToArray();
            using (AssemblyInstaller assemblyInstaller = new AssemblyInstaller(pathToAssembly, arguments)) {
                assemblyInstaller.Uninstall(new Hashtable());
            }
        }
        /// <summary>
        /// Test service:
        /// 1. Stop if it's running.
        /// 2. Start.
        /// 3. Stop again.
        /// 4. Return service to it's initial state (running or stopped).
        /// If any of these steps fails, test is not passed.
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="timeoutSeconds">Service operation timeout</param>
        public static void TestService(string service, int timeoutSeconds) {
            var status = GetServiceStatus(service);
            bool start = status == ServiceControllerStatus.Running ||
                status == ServiceControllerStatus.StartPending;
            RestartService(service, timeoutSeconds);
            StopService(service, timeoutSeconds);
            if (start) {
                StartService(service, timeoutSeconds);
            }
        }

        /// <summary>
        /// Install the service that is contained in specified assembly.
        /// </summary>
        /// <param name="pathToAssembly">Path to service assembly.</param>
        /// <param name="commandLineArguments">Parameters, that are passed to assembly.</param>
        public static void InstallAssembly(string pathToAssembly, string[] commandLineArguments) {
            List<string> argList = new List<string>(commandLineArguments);

            argList.Add(string.Format("/LogFile={0}_install.log",
                                        Path.GetFileNameWithoutExtension(pathToAssembly)));
            using (AssemblyInstaller installer = new AssemblyInstaller(pathToAssembly, argList.ToArray())) {
                var state = new Hashtable();
                installer.Install(state);
                installer.Commit(state);
            }
        }
        /// <summary>
        /// Get service status.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <returns>Service status</returns>
        public static ServiceControllerStatus GetServiceStatus(string serviceName) {
            using (ServiceController controller = new ServiceController(serviceName)) {
                return controller.Status;
            }
        }

        /// <summary>
        /// Stop service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="timeoutSeconds">Service operation timeout.</param>
        public static void StopService(string serviceName, int timeoutSeconds) {
            using (ServiceController controller = new ServiceController(serviceName)) {
                if (controller.Status != ServiceControllerStatus.Stopped &&
                    controller.Status != ServiceControllerStatus.StopPending) {
                    controller.Stop();
                }
                controller.WaitForStatus(ServiceControllerStatus.Stopped,
                            TimeSpan.FromSeconds(timeoutSeconds));
            }
        }
        /// <summary>
        /// Start the service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="timeoutSeconds">Service operation timeout.</param>
        public static void StartService(string serviceName, int timeoutSeconds) {
            using (ServiceController controller = new ServiceController(serviceName)) {
                if (controller.Status != ServiceControllerStatus.Running &&
                    controller.Status != ServiceControllerStatus.StartPending) {
                    controller.Start();
                }
                controller.WaitForStatus(ServiceControllerStatus.Running,
                            TimeSpan.FromSeconds(timeoutSeconds));
            }
        }
        /// <summary>
        /// Restart the service.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        /// <param name="timeoutSeconds">Service operation timeout.</param>
        public static void RestartService(string serviceName, int timeoutSeconds) {
            try {
                StopService(serviceName, timeoutSeconds);
                StartService(serviceName, timeoutSeconds);
            }
            catch (Exception e) {
                throw new Exception(
                    "Ошибка на этапе тестирования (запуск-остановка-запуск): " +
                    e.Message);
            }
        }
    }
}
