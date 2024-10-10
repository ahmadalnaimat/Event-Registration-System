using Event_Registration_System.Servcies.Interface;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace Event_Registration_System.Servcies
{
    public class EmailService : IEmail
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            string apiKey = _configuration["Mailjet:ApiKey"];
            string secretKey = _configuration["Mailjet:SecretKey"];

            var client = new MailjetClient(apiKey, secretKey);
            var request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray {
            new JObject {
                {
                    "From",
                    new JObject {
                        {"Email", "your_sender_email@example.com"},
                        {"Name", "Event Registration System"}
                    }
                },
                {
                    "To",
                    new JArray {
                        new JObject {
                            {"Email", email},
                            {"Name", "Participant"}
                        }
                    }
                },
                {
                    "Subject",
                    subject
                },
                {
                    "HTMLPart",
                    message
                }
            }
            });

            await client.PostAsync(request);
        }
    }
}
