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
using System.Threading;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Позволяет выполнять действия, отображающие окна в установочных классах.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    public class UIThreadAction<T>
    {
        /// <summary>
        /// Поток с STA атрибутом.
        /// </summary>
        private Thread m_invokerThread;
        /// <summary>
        /// Действие
        /// </summary>
        Func<T> m_action;
        /// <summary>
        /// Результат выполнения
        /// </summary>
        T result;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">действие</param>
        public UIThreadAction(Func<T> action)
        {
            m_action = action;
            m_invokerThread = new Thread(new ThreadStart(() => result = action.Invoke()));
            m_invokerThread.SetApartmentState(ApartmentState.STA);
        }
        /// <summary>
        /// Выполнить действие
        /// </summary>
        /// <returns>возвращенное значение</returns>
        public T Invoke()
        {
            m_invokerThread.Start();
            m_invokerThread.Join();
            return result;
        }
    }

    /// <summary>
    /// Позволяет выполнять действия, отображающие окна в установочных классах.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    public class UIThreadAction : UIThreadAction<int>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">действие</param>
        public UIThreadAction(Action action)
            : base(() => { action.Invoke(); return 0; })
        {
        }
        /// <summary>
        /// Выполнить действие
        /// </summary>
        public new void Invoke()
        {
            base.Invoke();
        }
    }
}
