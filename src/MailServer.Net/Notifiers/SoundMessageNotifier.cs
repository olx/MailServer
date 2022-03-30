using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MailServer.Net
{
    public class SoundMessageNotifier : IMessageNotifier
    {
        private readonly IOptionsMonitor<NotificationOptions> _options;
        private readonly ILogger<IMessageNotifier> _logger;
        public SoundMessageNotifier(IOptionsMonitor<NotificationOptions> options, ILogger<IMessageNotifier> logger)
        { 
            _options = options;
            _logger = logger;
        }

        public Task<NotifyResult> Notify(MimeMessage message)
        {
            try
            {
                var process = Process.Start(@"powershell", $@"-c (New-Object Media.SoundPlayer '{_options.CurrentValue.AudioFile}').PlaySync()");
                return Task.FromResult(new NotifyResult());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Task.FromResult(new NotifyResult { IsFaulted = true, Exception = ex });
            }             
        }
    }
}
