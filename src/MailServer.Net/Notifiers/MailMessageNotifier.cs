using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace MailServer.Net
{
    public class MailMessageNotifier : IMessageNotifier
    {

        private readonly IOptionsMonitor<MailForwardingOptions> _options;
        private readonly ILogger<IMessageNotifier> _logger;
        public MailMessageNotifier(IOptionsMonitor<MailForwardingOptions> options, ILogger<IMessageNotifier> logger)
        { 
            _options = options;
            _logger = logger;
        }

        public async Task<NotifyResult> Notify(MimeMessage message)
        {
            try
            {
                var client = new SmtpClient();
                await client.ConnectAsync(_options.CurrentValue.Host, _options.CurrentValue.Port, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_options.CurrentValue.Mail, _options.CurrentValue.Password);
                var result = await client.SendAsync(CreateForwardMessage(message));
                
                _logger.LogInformation(result);
                
                await client.DisconnectAsync(true);
                
                return new NotifyResult();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                return new NotifyResult { IsFaulted = true, Exception = ex };
            }
        }

        MimeMessage CreateForwardMessage(MimeMessage message)
        {
            message.To.Clear();
            message.To.AddRange(_options.CurrentValue.Addresses.Select(s => MailboxAddress.Parse(s)));

            message.From.Clear();
            message.From.Add(MailboxAddress.Parse(_options.CurrentValue.Mail));

            return message;
        }
    }
}
