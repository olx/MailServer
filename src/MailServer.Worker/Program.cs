using MailServer.Worker;
using MailServer.Net;
using SmtpServer.Storage;
using MailServer.Net.Notifiers;
using NLog.Web;
using NLog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configurationRoot = context.Configuration;
        services.Configure<MailServerOptions>(configurationRoot.GetSection("MailServer"));
        services.Configure<NotificationOptions>(configurationRoot.GetSection("Notifications"));
        services.Configure<MailForwardingOptions>(configurationRoot.GetSection("MailForwarding"));
        services.Configure<TelegramOptions>(configurationRoot.GetSection("Telegram"));

        services.AddSingleton<MailMessageNotifier>();
        services.AddSingleton<SoundMessageNotifier>();
        services.AddSingleton<TelegramMessageNotifier>();

        services.AddSingleton<IMessageStore, MessageService>(x => new MessageService(x.GetRequiredService<ILogger<IMessageStore>>(), new List<IMessageNotifier>
        {
            x.GetRequiredService<MailMessageNotifier>(),
            x.GetRequiredService<SoundMessageNotifier>(),
            x.GetRequiredService<TelegramMessageNotifier>(),
        }));

        services.AddSingleton<MailServer.Net.MailServer>();
        services.AddHostedService<Worker>();

    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        logging.AddConsole();
        LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
    })
    .UseNLog()
    .Build();

await host.RunAsync();
