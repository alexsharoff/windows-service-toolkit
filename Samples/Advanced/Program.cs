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
using System.Linq;
using System.Text;

namespace Granch.WindowsServiceToolkit.Samples.Advanced
{
    class Program
    {
        static void Main(string[] args)
        {
            AdvancedServiceExample service = new AdvancedServiceExample();
            service.Run();
        }
    }
}
