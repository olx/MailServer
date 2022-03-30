using System;
using NUnit.Framework;
using System.Threading.Tasks;
using MailKit;
using MimeKit;
using System.Linq;
using MimeKit.Text;
using Moq;
using Microsoft.Extensions.Logging;

namespace MailServer.Net.Tests
{
    public class MailMessageNotifierTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Notify()
        {
            var options = new MailForwardingOptions
            {
                Host = "smtp.gmail.com",
                Port = 587,
                Mail = "",
                Password = "",
                DisplayName = "",
                Addresses = new string[] { "" }

            };
            var logger = new Mock<ILogger<MailMessageNotifier>>();

            var notifier = new MailMessageNotifier(options, logger.Object);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(options.Mail));
            email.To.AddRange(options.Addresses.Select(s => MailboxAddress.Parse(s)));
            email.Subject = "Test Email Subject";
            email.Body = new TextPart(TextFormat.Plain) { Text = "Example Plain Text Message Body" };
            
            var result = await notifier.Notify(email);

            Assert.IsFalse(result.IsFaulted);
        }
    }
}