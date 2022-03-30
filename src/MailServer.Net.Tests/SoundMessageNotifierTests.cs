using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;
using Moq;

namespace MailServer.Net.Tests
{
    public class SoundMessageNotifierTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Notify()
        {
            var logger = new Mock<ILogger<IMessageNotifier>>();
            var notifier = new SoundMessageNotifier(@"c:\dasita\NavitelContent\Voices\0419~Alenka_2\warning_turn.wav");
            var result = await notifier.Notify(default);
            Assert.IsFalse(result.IsFaulted);
        }
    }
}