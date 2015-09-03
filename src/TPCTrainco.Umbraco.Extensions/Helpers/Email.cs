﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TPCTrainco.Umbraco.Extensions.Helpers
{
    public class Email
    {
        public string EmailFrom { get; set; }
        public List<string> EmailToList { get; set; }
        public List<string> EmailCCList { get; set; }
        public List<string> EmailBccList { get; set; }
        public bool IsBodyHtml { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public bool SendEmail()
        {
            bool success = false;

            var message = new MailMessage();

            message.From = new MailAddress(this.EmailFrom);

            if (this.EmailToList != null && this.EmailToList.Count > 0)
            {
                foreach (string emailTo in this.EmailToList)
                {
                    message.To.Add(new MailAddress(emailTo));
                }
            }

            if (this.EmailCCList != null && this.EmailCCList.Count > 0)
            {
                foreach (string emailCC in this.EmailCCList)
                {
                    message.CC.Add(new MailAddress(emailCC));
                }
            }

            if (this.EmailBccList != null && this.EmailBccList.Count > 0)
            {
                foreach (string emailBcc in this.EmailBccList)
                {
                    message.Bcc.Add(new MailAddress(emailBcc));
                }
            }

            message.Subject = this.Subject;
            message.Body = this.Body;
            message.IsBodyHtml = this.IsBodyHtml;

            using (var smtp = new SmtpClient())
            {
                smtp.Send(message);

                success = true;
            }

            return success;
        }
    }
}