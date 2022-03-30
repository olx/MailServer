using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.Json;
using TdLib;

namespace MailServer.Net.Notifiers
{
    public class TelegramMessageNotifier : IMessageNotifier
    {
        private readonly TdClient _client;
        private readonly IOptionsMonitor<TelegramOptions> _options;
        private readonly ILogger _logger;
        private TdApi.Chats? _chats;
        public TelegramMessageNotifier(IOptionsMonitor<TelegramOptions> options, ILogger<IMessageNotifier> logger)
        {
            _options = options;
            _logger = logger;
            _client = CreateTdClient();

            if (!_options.CurrentValue.Enable)
                _logger.LogInformation("Telegram is not enabled");            
        }

        TdClient CreateTdClient()
        {
            var client = new TdClient();
            client.Bindings.SetLogVerbosityLevel(_options.CurrentValue.LogLevel);
            client.UpdateReceived += async (_, update) => { await TryProcessUpdates(update); };
            return client;
        }

        async Task TryProcessUpdates(TdApi.Update update)
        {
            if (!_options.CurrentValue.Enable)
                return;
            try
            {
                await ProcessUpdates(update);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        async Task ProcessUpdates(TdApi.Update update)
        {

            switch (update)
            {
                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters }:
                    await _client.ExecuteAsync(new TdApi.SetTdlibParameters
                    {
                        Parameters = new TdApi.TdlibParameters
                        {
                            DatabaseDirectory = "tdlib",
                            UseMessageDatabase = true,
                            UseSecretChats = true,
                            ApiId = _options.CurrentValue.ApiId,
                            ApiHash = _options.CurrentValue.ApiHash,
                            DeviceModel = "Desktop",
                            SystemLanguageCode = "en",
                            ApplicationVersion = "1.0",
                            EnableStorageOptimizer = true
                        }
                    });
                    break;

                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitEncryptionKey }:
                    await _client.ExecuteAsync(new TdApi.CheckDatabaseEncryptionKey());
                    break;

                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber }:
                    await _client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber { PhoneNumber = _options.CurrentValue.Phone });
                    break;

                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitCode }:
                    while (string.IsNullOrEmpty(_options.CurrentValue.Code))
                    {
                        _logger.LogInformation("Enter authorization code into configuration json file to 'telegram:code'");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    await _client.ExecuteAsync(new TdApi.CheckAuthenticationCode { Code = _options.CurrentValue.Code });
                    break;
            }
        }

        public async Task<NotifyResult> Notify(MimeMessage message)
        {
            if (!_options.CurrentValue.Enable)
                return new NotifyResult();

            var content = new TdApi.InputMessageContent.InputMessageText()
            {
                Text = new TdApi.FormattedText()
                {
                    Text = $"{message.Subject}",
                }
            };

            try
            {
                if (_chats == null)
                    _chats = await _client.GetChatsAsync(null, 1000);

                await _client.SendMessageAsync(chatId: _options.CurrentValue.ChatId, inputMessageContent: content);
                return await Task.FromResult(new NotifyResult());
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Chat not found", StringComparison.OrdinalIgnoreCase))
                {
                    _chats = null;
                    _logger.LogInformation("Next try will update chat list");
                }
                    
                _logger.LogError(ex, ex.Message);
                return await Task.FromResult(new NotifyResult() { IsFaulted = true, Exception = ex });
            }
        }
    }
}
