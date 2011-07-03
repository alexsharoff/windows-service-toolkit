
namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Режим запуска сервиса
    /// </summary>
    public enum ServiceStartType
    {
        ///<summary>
        /// Автоматический, при загрузке системы
        ///</summary>
        Automatic,
        ///<summary>
        /// Вручную
        ///</summary>
        Manual,
        ///<summary>
        /// Отключен
        ///</summary>
        Disabled,
        ///<summary>
        /// Отложенный автозапуск при загрузке системы.
        /// Используется, если по какой-либо причине служба
        /// запускается быстрее её зависимостей.
        ///</summary>
        Delayed
    }
}
