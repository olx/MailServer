using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using System.Buffers;
using System.Runtime.CompilerServices;
using MimeKit;

namespace MailServer.Net
{
    
    public class MailServer
    {
        private readonly MailServerOptions _options;
        private readonly IMessageStore _messageStore;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private SmtpServer.SmtpServer _smtpServer;
        public MailServer(IOptions<MailServerOptions> options, IMessageStore messageStore, ILogger<MailServer> logger)
        { 
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
            _logger = logger;
        }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            var smtpOptions = new SmtpServerOptionsBuilder().ServerName(_options.Name).Port(_options.SmtpPort).Build();
            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(_messageStore);
            _smtpServer = new SmtpServer.SmtpServer(smtpOptions, serviceProvider);
            return _smtpServer.StartAsync(cancellationToken);
        }
    }

    public class MessageService : MessageStore
    {
        private readonly IList<IMessageNotifier> _messageNotifiers = new List<IMessageNotifier>();
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        public MessageService(ILogger<IMessageStore> logger)
        { 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public MessageService(ILogger<IMessageStore> logger, IList<IMessageNotifier> messageNotifiers)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageNotifiers = messageNotifiers ?? throw new ArgumentNullException(nameof(messageNotifiers));
        }
        public MessageService Add(IMessageNotifier messageNotifier) 
        { 
            _messageNotifiers.Add(messageNotifier); 
            return this; 
        }
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                var message = await HandleMessage(buffer, cancellationToken);

                _logger.LogInformation("Message recieved: {message}", message);
                
                return await Notify(message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                return SmtpResponse.TransactionFailed;
            }
        }
        static async Task<MimeMessage> HandleMessage(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }

            stream.Position = 0;

            return await MimeMessage.LoadAsync(stream, cancellationToken);
        }
        async Task<SmtpResponse> Notify(MimeMessage message)
        {
            for (int i = 0; i < _messageNotifiers.Count; i++)
                await _messageNotifiers[i].Notify(message);

            return SmtpResponse.Ok;
        }
    }
}
