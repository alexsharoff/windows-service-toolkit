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
using System.DirectoryServices.AccountManagement;

namespace Granch.WindowsServiceToolkit
{
    /// <summary>
    /// Класс для управления локальными пользователями (из базы данных SAM).
    /// </summary>
    public class SAMUserAccounts
    {
        /// <summary>
        /// Префикс текущей машины
        /// </summary>
        static string MachinePrefix = Environment.MachineName + "\\";
        /// <summary>
        /// Проверить, существует ли пользователь
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <returns>True, если существует, иначе false.</returns>
        public bool Exists(string username)
        {
            var user = FindUserPrincipal(username);
            bool r = user != null;
            if (user != null)
            {
                user.Dispose();
            }
            return r;
        }

        /// <summary>
        /// Создать пользователя, с указанным именем и паролем.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        public void Create(string username, string password)
        {
            using (UserPrincipal user = new UserPrincipal(new PrincipalContext(ContextType.Machine)))
            {
                user.SamAccountName = username;
                if (password.Length == 0)
                {
                    user.PasswordNotRequired = true;
                }
                else
                {
                    user.SetPassword(password);
                    user.PasswordNeverExpires = true;
                }
                user.Enabled = true;
                user.Save();
            }
        }
        /// <summary>
        /// Сделать пользователя администратором.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        public void MakeAdmin(string username)
        {
            using (var user = FindUserPrincipal(username))
            {
                if (user != null)
                {
                    using (var group = FindAdminGroupPrincipal())
                    {
                        group.Members.Add(user);
                        group.Save();
                    }
                }
                else
                {
                    throw new Exception("User not found.");
                }
            }
        }

        /// <summary>
        /// Удалить пользователя из базы данных.
        /// </summary>
        /// <param name="username">Имя пользователя.</param>
        public void Remove(string username)
        {
            using (var user = FindUserPrincipal(username))
            {
                if (user != null)
                {
                    user.Delete();
                }
                else
                {
                    throw new Exception("User not found.");
                }
            }
        }
        /// <summary>
        /// Добавить префикс машины к имени пользователя (если его еще нет).
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <returns>Имя пользователя с префиксов (если его не было).</returns>
        public string AppendMachineNamePrefix(string username)
        {
            if (!username.Contains("\\"))
            {
                username = MachinePrefix + username;
            }
            return username;
        }

        /// <summary>
        /// Найти UserPrincipal по имени пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <returns>UserPrincipal</returns>
        UserPrincipal FindUserPrincipal(string username)
        {
            username = AppendMachineNamePrefix(username);
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                return UserPrincipal.FindByIdentity(
                    context, IdentityType.SamAccountName, username);
            }
        }

        GroupPrincipal FindAdminGroupPrincipal()
        {
            PrincipalContext context = new PrincipalContext(ContextType.Machine);
            //Некрасиво, но лучшего способа пока не нашел.
            var adminGroup = GroupPrincipal.FindByIdentity(context, "Administrators");
            if (adminGroup == null)
                adminGroup = GroupPrincipal.FindByIdentity(context, "Администраторы");
            return adminGroup;
        }
    }
}
