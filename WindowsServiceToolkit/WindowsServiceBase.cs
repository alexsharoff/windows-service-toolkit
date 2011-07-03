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
/// 
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

using AVS.Tools;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Базовый класс для служб Windows
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public abstract class WindowsServiceBase : ServiceBase
    {
        protected UniversalLog Log
        {
            get;
            private set;
        }

        public WindowsServiceBase()
        {
            Log = new UniversalLog();
        }

        /// <summary>
        /// Поток, в котором работает служба
        /// </summary>
        Thread m_serviceThread;
        /// <summary>
        /// Блокировщик для критических секций в коде
        /// </summary>
        object m_lock = new object();
        /// <summary>
        /// Начать работу службы.
        /// </summary>
        public void Run()
        {
            Log.RedirectToEventLog = true;
            Log.EventSource = ServiceName;

            Process parentProc = ProcessUtilities.GetParentProcess();
            if (parentProc != null && parentProc.ProcessName == "services")
            {
                IsInConsoleMode = false;
                System.ServiceProcess.ServiceBase[] ServicesToRun;
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { this };
                System.ServiceProcess.ServiceBase.Run(ServicesToRun);
            }
            else
            {
                IsInConsoleMode = true;
                if (ServicesDependedOn != null)
                {
                    foreach (string serviceName in ServicesDependedOn)
                    {
                        if (InstallUtilities.GetServiceStatus(serviceName) != ServiceControllerStatus.Running)
                        {
                            try
                            {
                                InstallUtilities.StartService(serviceName, 30);
                                Log.Write(EventLogEntryType.Information, "Запуск сервиса {0} прошел успешно.", serviceName);
                            }
                            catch (InvalidOperationException)
                            {
                                Log.Write(EventLogEntryType.Warning, "Отказано в запуске сервиса {0}: недостаточно прав.", serviceName);
                            }
                            catch (Exception e)
                            {
                                Log.Write(e);
                            }
                        }
                    }
                }
                Console.CancelKeyPress += delegate
                {
                    Console.WriteLine("Получен сигнал прерывания.");
                    Stop();
                };
                OnStart(new string[] { });
            }
        }
        /// <summary>
        /// Метод, выполняющий работу службы.
        /// Должен иметь возможность прекратить свою работу по вызову RequestStop. 
        /// </summary>
        protected abstract void RunService();
        /// <summary>
        /// Запрос остановки службы. Должен прекращать работу метода RunService.
        /// </summary>
        protected abstract void RequestStop();
        /// <summary>
        /// Системное имя службы. Не должно содержать пробелов. 
        /// </summary>
        public abstract new string ServiceName
        {
            get;
        }
        /// <summary>
        /// Отображаемое имя службы. Ограничений на набор символов нет.
        /// </summary>
        public abstract string DisplayedName
        {
            get;
        }
        /// <summary>
        /// Описание службы.
        /// </summary>
        public abstract string Description
        {
            get;
        }
        /// <summary>
        /// Список служб, без которых эта служба работать не может.
        /// В списке нужно указывать их системные имена.
        /// ОС автоматически запустит все службы из этого списка 
        /// перед запуском вашей.
        /// </summary>
        public abstract string[] ServicesDependedOn
        {
            get;
        }
        /// <summary>
        /// Запущена ли служба в консольном режиме.
        /// </summary>
        public bool IsInConsoleMode
        {
            get;
            protected set;
        }
        /// <summary>
        /// Обработка запроса от ОС на запуск службы
        /// </summary>
        /// <param name="args">параметры</param>
        protected override void OnStart(string[] args)
        {
            lock (m_lock)
            {
                m_serviceThread = new Thread(RunService);
                m_serviceThread.Start();
            }
        }
        /// <summary>
        /// Обработка запроса  от ОС на остановку службы
        /// </summary>
        protected override void OnStop()
        {
            lock (m_lock)
            {
                RequestStop();
                if (m_serviceThread != null)
                {
                    m_serviceThread.Join();
                }
            }
        }
    }
}
