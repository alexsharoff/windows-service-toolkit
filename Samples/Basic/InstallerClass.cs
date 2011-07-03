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


using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;


namespace Granch.WindowsServiceToolkit.Samples.Basic
{
    /// <summary>
    /// Инсталлятор
    /// </summary>
    [RunInstaller(true)]
    public partial class InstallerClass : Installer
    {
        /// <summary>
        /// Экземпляр WindowsServiceInstaller
        /// </summary>
        WindowsServiceInstaller<BasicServiceExample> m_serviceInstaller = new WindowsServiceInstaller<BasicServiceExample>();
        /// <summary>
        /// Конструктор
        /// </summary>
        public InstallerClass()
        {
            ConfigureServiceInstaller();
            // Добавление WindowsServiceInstaller в список инсталляторов.
            Installers.Add(m_serviceInstaller);
        }
        /// <summary>
        /// Настройка WindowsServiceInstaller
        /// </summary>
        void ConfigureServiceInstaller()
        {
            m_serviceInstaller.Account = ServiceAccount.LocalSystem;
            m_serviceInstaller.FailCountResetTimeHours = 24 * 7;
            m_serviceInstaller.StartOnInstall = true;
            m_serviceInstaller.StartTimeoutSeconds = 15;
            m_serviceInstaller.StartType = ServiceStartType.Automatic;
            m_serviceInstaller.FailureActions.AddRange(
                new FailureAction[]{
                    new FailureAction(){
                        DelaySeconds = 60,
                        Type = FailureActionType.Restart
                    },
                    new FailureAction(){
                        DelaySeconds = 300,
                        Type = FailureActionType.Reboot
                    },
                    new FailureAction(){
                        DelaySeconds = 300,
                        Type = FailureActionType.RunCommand
                    },
                }
            );
            m_serviceInstaller.FailureRunCommand = new SystemCommand()
            {
                ExecutablePath = "C:\\granch utilities\\sendmail.exe",
                Parameters = "sharov@granch.ru topic=\"оно сломалось\""
            };
            m_serviceInstaller.FailureRebootMessage = "Из-за многократных ошибок в работе службы требуется перезагрузка компьютера.";
        }
    }
}
