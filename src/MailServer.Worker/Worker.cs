namespace MailServer.Worker
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MailServer.Net.MailServer _server;

        public Worker(MailServer.Net.MailServer server, ILogger<Worker> logger)
        {
            _server = server;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _server.RunAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}