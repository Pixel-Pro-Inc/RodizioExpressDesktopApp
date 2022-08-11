using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class SendGridTester
    {
        public async Task<bool> SendEmail(string e)//UnhandledExceptionEventArgs e)
        {
            var apiKey = "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";

            var client = new SendGridClient(apiKey);

            string userCode = "Rodizio Express Error Logger";

            var recipients = new List<string>() { "pixelprocompanyco@gmail.com",
                "yewotheu123456789@gmail.com","apexmachine2@gmail.com"};

            string _subject = "POS Terminal Error" + System.DateTime.Now.ToString();

            foreach (var reciepient in recipients)
            {
                var from = new EmailAddress("corecommunications2022@gmail.com", userCode);
                var subject = _subject;
                var to = new EmailAddress(reciepient);
                var plainTextContent = _subject;
                var htmlContent = System.DateTime.Now.ToString() + "_" + e;//e.ExceptionObject.ToString();
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                await client.SendEmailAsync(msg).ConfigureAwait(false);
            }

            return true;
        }
    }
}
