using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailServer.Net
{
    public class ConsoleMessageNotifier: IMessageNotifier
    {
        public Task<NotifyResult> Notify(MimeMessage message)
        {
            Console.WriteLine(message.Body.ToString());
            return Task.FromResult(new NotifyResult());
        }
    }
}
