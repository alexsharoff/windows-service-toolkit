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
using System.Diagnostics;

namespace Granch.WindowsServiceToolkit.Samples.Basic
{
    /// <summary>
    /// Пример сервиса
    /// </summary>
    public class BasicServiceExample : WindowsServiceBase
    {
        // Пока m_run = true, сервис работает.
        volatile bool m_run;
        // Системное имя сервиса. Не должно содержать пробелов. 
        public override string ServiceName
        {
            get { return "BasicServiceExample"; }
        }
        // Отображаемое имя сервиса. Ограничений на набор символов нет.
        public override string DisplayedName
        {
            get { return "WindowsServiceToolkit - Basic Example"; }
        }
        // Описание сервиса.
        public override string Description
        {
            get { return "Это пример из WindowsServiceToolkit."; }
        }
        //Список сервисов, без которых этот сервис работать не может.
        // В списке нужно указывать системные имена сервисов.
        // ОС автоматически запустить все сервисы из этого списка 
        // перед запуском вашего сервиса.
        public override string[] ServicesDependedOn
        {
            get { return new string[] { "WebClient" }; }
        }
        // Метод, выполняющий работу сервиса.
        // Должен иметь возможность прекратить свою работу по вызову RequestStop. 
        protected override void RunService()
        {
            m_run = true;
            while (m_run)
            {
                Log.Write(EventLogEntryType.Information, "BasicServiceExample is working...");
                Thread.Sleep(1000);
            }
        }
        // Запрос остановки сервиса. Должен прекращать работу метода RunService.
        protected override void RequestStop()
        {
            Log.Write(EventLogEntryType.Information, "Stopping BasicServiceExample...");
            m_run = false;
        }
    }
}
