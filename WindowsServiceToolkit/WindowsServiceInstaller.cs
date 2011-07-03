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

using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using AVS.Tools;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Инсталлятор служб Windows.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class WindowsServiceInstaller<TService> : Installer
        where TService : WindowsServiceBase, new()
    {
        /// <summary>
        /// Экземпляр ServiceProcessInstaller
        /// </summary>
        ServiceProcessInstaller m_processInstaller = new ServiceProcessInstaller();
        /// <summary>
        /// Экземпляр AdvancedServiceInstaller
        /// </summary>
        AdvancedServiceInstaller m_installer = new AdvancedServiceInstaller();
        /// <summary>
        /// Класс для управления пользовтелями
        /// </summary>
        SAMUserAccounts m_users = new SAMUserAccounts();
        /// <summary>
        /// Конструктор
        /// </summary>
        public WindowsServiceInstaller()
        {
            Installers.AddRange(new Installer[] {
                m_processInstaller,
                m_installer});
            using (var service = new TService())
            {
                m_installer.DisplayName = service.DisplayedName;
                m_installer.Description = service.Description;
                m_installer.ServiceName = service.ServiceName;
                m_installer.ServicesDependedOn = service.ServicesDependedOn;
            }
            Account = ServiceAccount.LocalSystem;
            StartType = ServiceStartType.Automatic;

            BeforeInstall += BeforeInstallHandler;
            AfterInstall += AfterInstallHandler;
            BeforeUninstall += BeforeUninstallHandler;
        }
        #region Properties
        /// <summary>
        /// Действия при отказе в работе сервиса.
        /// </summary>
        public List<FailureAction> FailureActions
        {
            get
            {
                return m_installer.FailureActions;
            }
        }
        /// <summary>
        /// Тип запуска сервиса.
        /// </summary>
        public ServiceStartType StartType
        {
            get
            {
                switch (m_installer.StartType)
                {
                    case ServiceStartMode.Automatic:
                        if (DelayedAutoStart)
                        {
                            return ServiceStartType.Delayed;
                        }
                        else
                        {
                            return ServiceStartType.Automatic;
                        }
                    case ServiceStartMode.Manual:
                        return ServiceStartType.Manual;
                    default:
                        return ServiceStartType.Disabled;
                }
            }
            set
            {
                DelayedAutoStart = value == ServiceStartType.Delayed;
                switch (value)
                {
                    case ServiceStartType.Delayed:
                    case ServiceStartType.Automatic:
                        m_installer.StartType = ServiceStartMode.Automatic;
                        break;
                    case ServiceStartType.Manual:
                        m_installer.StartType = ServiceStartMode.Manual;
                        break;
                    case ServiceStartType.Disabled:
                        m_installer.StartType = ServiceStartMode.Disabled;
                        break;
                }
            }
        }
        /// <summary>
        /// Учетная запись, используемая сервисом. 
        /// Если значение равно User - требуется указать Username и Password.
        /// </summary>
        public ServiceAccount Account
        {
            get
            {
                return m_processInstaller.Account;
            }
            set
            {
                m_processInstaller.Account = value;
            }
        }
        /// <summary>
        /// Имя пользователя учетной записи сервиса.
        /// </summary>
        public string Username
        {
            get
            {
                return m_processInstaller.Username;
            }
            set
            {
                m_processInstaller.Username = m_users.AppendMachineNamePrefix(value);
            }
        }
        /// <summary>
        /// Пароль учетной записи сервиса.
        /// </summary>
        public string Password
        {
            get
            {
                return m_processInstaller.Password;
            }
            set
            {
                m_processInstaller.Password = value;
            }
        }
        /// <summary>
        /// Через сколько часов с момента последнего отказа 
        /// счетчик отказов будет сброшен
        /// </summary>
        public int FailCountResetTimeHours
        {
            get
            {
                return m_installer.FailCountResetTime / 3600;
            }
            set
            {
                m_installer.FailCountResetTime = value * 3600;
            }
        }

        /// <summary>
        /// Сообщение при перезагрузке системы этой службой (FailureActionType.Reboot).
        /// </summary>
        public string FailureRebootMessage
        {
            get
            {
                return m_installer.FailureRebootMessage;
            }
            set
            {
                m_installer.FailureRebootMessage = value;
            }
        }

        /// <summary>
        /// Команда, запускаемая при отказе службы (FailureActionType.RunCommand).
        /// </summary>
        public SystemCommand FailureRunCommand
        {
            get
            {
                return m_installer.FailureCommandAction;
            }
            set
            {
                m_installer.FailureCommandAction = value;
            }
        }

        /// <summary>
        /// Автоматический запускать сервис после инсталляции.
        /// </summary>
        public bool StartOnInstall
        {
            get
            {
                return m_installer.StartOnInstall;
            }
            set
            {
                m_installer.StartOnInstall = value;
            }
        }

        /// <summary>
        /// Максимальное время ожидания запуска службы.
        /// Если служба не запустилась за это время, она будет считаться отказавшей.
        /// </summary>
        public int StartTimeoutSeconds
        {
            get
            {
                return m_installer.StartTimeoutSeconds;
            }
            set
            {
                m_installer.StartTimeoutSeconds = value;
            }
        }

        #endregion
        /// <summary> 
        /// Очистка используемых ресурсов.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_processInstaller.Dispose();
                m_installer.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// Отложенный запуск.
        /// </summary>
        private bool DelayedAutoStart
        {
            get
            {
                return m_installer.DelayedAutoStart;
            }
            set
            {
                m_installer.DelayedAutoStart = value;
            }
        }
        /// <summary>
        /// Подготовка контекста инсталлятора
        /// </summary>
        /// <param name="sender">отправитель</param>
        /// <param name="e">параметры</param>
        private void BeforeInstallHandler(object sender, InstallEventArgs e)
        {
            Context.Parameters.Remove("username");
            Context.Parameters.Remove("password");
        }
        /// <summary>
        /// Обработчик перед деинсталляцией службы.
        /// </summary>
        /// <param name="sender">отправитель</param>
        /// <param name="e">параметр</param>
        private void BeforeUninstallHandler(object sender, InstallEventArgs e)
        {
            try
            {
                InstallUtilities.StopService(m_installer.ServiceName, 15);
            }
            catch { }
        }
        private void AfterInstallHandler(object sender, InstallEventArgs e)
        {
            if (!EventLog.SourceExists(m_installer.ServiceName))
                EventLog.CreateEventSource(m_installer.ServiceName, "Application");
            InstallUtilities.TestService(m_installer.ServiceName, 30);
        }
    }
}
