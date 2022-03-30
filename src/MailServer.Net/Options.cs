using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailServer.Net
{
    public class MailServerOptions
    {
        public string Name { get; set; } = "localhost";
        public int SmtpPort { get; set; }

    }

    public class NotificationOptions
    {
        public string AudioFile { get; set; }
    }

    public class MailForwardingOptions
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string[] Addresses { get; set; }
    }

    public class TelegramOptions
    { 
        public int ApiId { get; set; }
        public string ApiHash { get; set; }
        public string Phone { get; set; }
        public long ChatId { get; set; }
        public string Code { get; set; }
        public int LogLevel { get; set; }
        public bool Enable { get; set; }
    }
}
