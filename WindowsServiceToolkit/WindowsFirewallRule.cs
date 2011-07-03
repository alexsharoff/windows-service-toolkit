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
using System.Linq;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Класс, представляющий собой правило для межсетевого экрана.
    /// </summary>
    public class WindowsFirewallRule
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public WindowsFirewallRule()
        {
            Ports = new List<int>();
        }

        /// <summary>
        /// Название правила. Должно быть уникальным
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Действие
        /// </summary>
        public FirewallActionType Action
        {
            get;
            set;
        }
        /// <summary>
        /// Описание правила.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Тип сетевого протокола для этого правила.
        /// </summary>
        public Protocol Protocol
        {
            get;
            set;
        }
        /// <summary>
        /// Направление соединения.
        /// </summary>
        public Direction Direction
        {
            get;
            set;
        }

        /// <summary>
        /// Список портов.
        /// </summary>
        public IEnumerable<int> Ports
        {
            get;
            set;
        }

        /// <summary>
        /// Путь к целевой программе.
        /// </summary>
        public string Program
        {
            get;
            set;
        }
        /// <summary>
        /// Название целевой службы.
        /// </summary>
        public string Service
        {
            get;
            set;
        }
        /// <summary>
        /// Текстовое представление списка портов.
        /// </summary>
        public string PortString
        {
            get
            {
                int[] ports = Ports.OrderBy(p => p).ToArray();
                string portsString = string.Empty;
                for (int i = 0; i < ports.Length; i++)
                {
                    int j = i;
                    for (; j < ports.Length - 1; j++)
                    {
                        if ((ports[j + 1] - ports[j]) > 1)
                        {
                            break;
                        }
                    }
                    if (portsString.Length > 0)
                    {
                        portsString += ",";
                    }
                    if (i != j)
                    {
                        portsString += ports[i].ToString() + "-" + ports[j].ToString();
                    }
                    else
                    {
                        portsString += ports[i].ToString();
                    }
                    i = j + 1;
                }
                return portsString;
            }
        }
    }
}
