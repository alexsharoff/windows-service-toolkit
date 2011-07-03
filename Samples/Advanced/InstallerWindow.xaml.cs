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
using System.ServiceProcess;
using System.Windows;

namespace Granch.WindowsServiceToolkit.Samples.Advanced
{
    /// <summary>
    /// Окно настройки службы.
    /// </summary>
    public partial class InstallerWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Событие из INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
       /// <summary>
       /// Учетная запись, используемая сервисом
       /// </summary>
        ServiceAccount m_serviceAccount;
        public ServiceAccount Account
        {
            get
            {
                return m_serviceAccount;
            }
            set
            {
                m_serviceAccount = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Account"));
                }
            }
        }
        /// <summary>
        /// Имя пользователя (Если Account = User)
        /// </summary>
        public string Username
        {
            get;
            set;
        }
        /// <summary>
        /// Пароль (Если Account = User)
        /// </summary>
        public string Password
        {
            get;
            set;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public InstallerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        /// <summary>
        /// Кнопка ОК нажата
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkClicked(object sender, RoutedEventArgs e)
        {
            Password = passwordBox.Password;
            DialogResult = true;
            Close();
        }
        /// <summary>
        /// Кнопка Отмена нажата
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
