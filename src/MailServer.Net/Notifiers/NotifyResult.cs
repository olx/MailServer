using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailServer.Net
{
    public class NotifyResult
    {
        public bool IsFaulted { get; set; }
        public Exception Exception { get; set; }
    }
}
