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
using AVS.Tools;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Класс для работы с межсетевым экраном в Windows старше XP.
    /// </summary>
    public class WindowsFirewall
    {
        /// <summary>
        /// Добавить правила
        /// </summary>
        /// <param name="rules">Набор правил</param>
        public void AddRules(IEnumerable<WindowsFirewallRule> rules)
        {
            foreach (WindowsFirewallRule rule in rules)
            {
                string ruleString = string.Format("name=\"{0}\" protocol={1} localport={2} action={3} dir={4}",
                    rule.Name, rule.Protocol.ToString(), rule.PortString, rule.Action, rule.Direction);
                string parameter = "advfirewall firewall add rule " + ruleString;

                if (rule.Description != null && rule.Description.Length > 0)
                {
                    parameter += string.Format(
                        " description=\"{0}\"", rule.Description.Replace('"', '\''));
                }
                if (rule.Program != null && rule.Program.Length > 0)
                {
                    parameter += string.Format(
                        " program=\"{0}\"", rule.Program.Replace('"', '\''));
                }
                if (rule.Service != null && rule.Service.Length > 0)
                {
                    parameter += string.Format(
                        " service=\"{0}\"", rule.Service.Replace('"', '\''));
                }

                SystemCommand cmd = new SystemCommand()
                {
                    ExecutablePath = "netsh.exe",
                    Parameters = parameter
                };
                int r = cmd.Execute();
                if (r != 0)
                {
                    throw new Exception(
                            string.Format(
                            "Невозможно добавить правило Брандмауэра: {0}.\r\nПричина: {1}.",
                            ruleString,
                            new Win32Exception(r).Message));
                }
            }
        }
        /// <summary>
        /// Удалить правила
        /// </summary>
        /// <param name="rules">набор правил</param>
        public void RemoveRules(IEnumerable<WindowsFirewallRule> rules)
        {
            foreach (WindowsFirewallRule rule in rules)
            {
                string ruleString = string.Format("name=\"{0}\"",
                    rule.Name, rule.Protocol.ToString(), rule.PortString);
                SystemCommand cmd = new SystemCommand()
                {
                    ExecutablePath = "netsh.exe",
                    Parameters = "advfirewall firewall delete rule " + ruleString
                };
                int r = cmd.Execute();
                if (r != 0)
                {
                    throw new Exception(
                            string.Format(
                            "Невозможно удалить правило Брандмауэра: {0}.\r\nПричина: {1}.",
                            ruleString,
                            new Win32Exception(r).Message));
                }
            }
        }
    }
}
