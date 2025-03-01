using MailKit.Net.Smtp;
using MimeKit;

namespace Group03_Kindergarten_Suggestion_System_Project.Services.Email
{
    public class EmailSender
    {
        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Admin", "vulqhe170481@fpt.edu.vn"));
                email.To.Add(new MailboxAddress("", toEmail));
                email.Subject = subject;

                email.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    smtpClient.Authenticate("vulqhe170481@fpt.edu.vn", "oltj gdlc hhah umbd");
                    smtpClient.Send(email);
                    smtpClient.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Send email fail: " + ex.Message);
            }
        }
    }
}
