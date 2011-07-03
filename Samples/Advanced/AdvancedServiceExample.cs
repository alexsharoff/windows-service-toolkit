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
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Granch.WindowsServiceToolkit.Samples.Advanced
{
    /// <summary>
    /// Пример сервиса
    /// </summary>
    public class AdvancedServiceExample : WindowsServiceBase
    {
        const string HTTPHeader =
@"HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Connection: close
Content-Length: {1}

{0}
";
        const string ResponseFormat =
@"<html><body>
<h2>Hello!</h2>
Your endpoint is: {0}.
</body></html>";

        TcpListener m_server;
        object m_lock = new object();
        // Пока m_run = true, сервис работает.
        volatile bool m_run;
        // Метод, выполняющий всю работу сервиса.
        // Должен иметь возможность прекратить свою работу по вызову RequestStop. 
        protected override void RunService()
        {
            m_run = true;
            lock (m_lock)
            {
                m_server = new TcpListener(IPAddress.Any, 5555);
                m_server.Start();
				Log.Write("HTTP server is running on port 5555");
            }
            while (m_run)
            {
                try
                {
                    TcpClient client = m_server.AcceptTcpClient();
                    string ep = client.Client.RemoteEndPoint.ToString();
                    var clientStream = client.GetStream();
                    clientStream.ReadTimeout = 100;

                    Byte[] bytes = new Byte[1024];
                    // no need to parse request stream, responce is always the same
                    while (true)
                    {
                        if (clientStream.DataAvailable)
                        {
                            clientStream.Read(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            break;
                        }
                    }
                    string body = string.Format(ResponseFormat, ep);
                    string responce = string.Format(HTTPHeader, body, body.Length);
                    byte[] r = System.Text.Encoding.ASCII.GetBytes(responce);
                    clientStream.Write(r, 0, r.Length);
                    Log.Write("Served client " + ep);
                    client.Close();
                }
                catch (SocketException)
                {
                }
                catch (Exception e)
                {
                    Log.Write(e);
                }
            }
            try
            {
                m_server.Stop();
            }
            catch { }
            m_server = null;
        }
        // Запрос остановки сервиса. Должен прекращать работу метода RunService.
        protected override void RequestStop()
        {
            Log.Write(EventLogEntryType.Information, "Stopping AdvancedServiceExample...");
            lock (m_lock)
            {
                if (m_server != null)
                {
                    try
                    {
                        m_server.Stop();
                    }
                    catch (Exception e)
                    {
                        Log.Write(e);
                    }
                }
                m_run = false;
            }
        }
        // Системное имя сервиса. Не должно содержать пробелов. 
        public override string ServiceName
        {
            get { return "AdvancedServiceExample"; }
        }
        // Отображаемое имя сервиса. Ограничений на набор символов нет.
        public override string DisplayedName
        {
            get { return "WindowsServiceToolkit - Advanced Example"; }
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
            get { return null; }
        }
    }
}
