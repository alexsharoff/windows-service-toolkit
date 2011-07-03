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
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Diagnostics;

namespace Granch.WindowsServiceToolkit.Samples.Advanced
{
    /// <summary>
    /// Инсталлятор
    /// </summary>
    [RunInstaller(true)]
    public class InstallerClass : Installer
    {
        /// <summary>
        /// Экземпляр WindowsServiceInstaller
        /// </summary>
        WindowsServiceInstaller<AdvancedServiceExample> m_serviceInstaller = new WindowsServiceInstaller<AdvancedServiceExample>();
        /// <summary>
        /// Управление межсетевым экраном
        /// </summary>
        WindowsFirewall m_firewall = new WindowsFirewall();
        /// <summary>
        /// Управление пользователями
        /// </summary>
        SAMUserAccounts m_users = new SAMUserAccounts();
        /// <summary>
        /// Набор правил межсетевого экрана.
        /// </summary>
        WindowsFirewallRule[] m_firewallRules = new WindowsFirewallRule[]{
                new WindowsFirewallRule() {
                    Description = "Первое правило",
                    Direction = Direction.In,
                    Protocol =  Protocol.TCP,
                    Name = "AdvancedServiceExample 1",
                    Ports = Enumerable.Range(5555, 10),
                    Service = "AdvancedServiceExample"
                },
                new WindowsFirewallRule() {
                    Action = FirewallActionType.Block,
                    Description = "Второе правило",
                    Direction = Direction.In,
                    Protocol=  Protocol.TCP,
                    Name = "AdvancedServiceExample 2",
                    Ports = Enumerable.Range(7000, 10),
                    Service = "AdvancedServiceExample"
                }
            };
        /// <summary>
        /// Конструктор
        /// </summary>
        public InstallerClass()
        {
            m_serviceInstaller.Account = ServiceAccount.LocalService;
            m_serviceInstaller.StartOnInstall = true;
            m_serviceInstaller.StartType = ServiceStartType.Delayed;
            // Добавление WindowsServiceInstaller в список инсталляторов.
            Installers.Add(m_serviceInstaller);

            BeforeInstall += delegate
            {
                new UIThreadAction(SetupServiceAccount).Invoke();
            };
            AfterInstall += AddFirewallRules;
            AfterUninstall += RemoveFirewallRules;
        }
        /// <summary>
        /// Настроить учетную запись сервиса
        /// </summary>
        void SetupServiceAccount()
        {
            InstallerWindow window = new InstallerWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Topmost = true;
            if (window.ShowDialog() == true)
            {
                m_serviceInstaller.Account = window.Account;
                if (m_serviceInstaller.Account == ServiceAccount.User)
                {
                    string username = window.Username;
                    string password = window.Password;
                    if (!m_users.Exists(username))
                    {
                        Debugger.Launch();
                        m_users.Create(username, password);
                        m_users.MakeAdmin(username);
                    }
                    m_serviceInstaller.Username = username;
                    m_serviceInstaller.Password = password;
                }
            }
            else
            {
                throw new InstallException("Установка была отменена");
            }
        }
        /// <summary>
        /// Добавть правила межсетевого экрана
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AddFirewallRules(object sender, InstallEventArgs e)
        {
            m_firewall.AddRules(m_firewallRules);
        }
        /// <summary>
        /// Удалить правила межсетевого экрана
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RemoveFirewallRules(object sender, InstallEventArgs e)
        {
            m_firewall.RemoveRules(m_firewallRules);
        }
    }
}
