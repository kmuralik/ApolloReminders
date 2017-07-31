using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ApolloReminders
{
    public class MailSender
    {
        public MailSender()
        {
            //
        }

        public int SendReminder(string toName, string toEmail,
            string ccName, string ccEmail, string subject, string body)
        {
            //
            var fromName = ConfigurationManager.AppSettings["DefaultSenderName"].ToString();
            var fromEmail = ConfigurationManager.AppSettings["DefaultSenderEmail"].ToString();
            //
            var from = new MailAddress(fromEmail, fromName);
            var to = new MailAddress(toEmail, toName);
            var cc = new MailAddress(ccEmail, ccName);
            var msg = new MailMessage();
            msg.From = from;
            msg.To.Add(to);
            msg.CC.Add(cc);
            msg.Subject = subject;
            msg.Body = body;
            //
            msg.IsBodyHtml = true;
            msg.BodyEncoding = Encoding.UTF8;
            msg.Priority = MailPriority.Normal;
            //
            if (SendReminder(msg))
                return 1;
            return 0;
        }

        private bool SendReminder(MailMessage msg)
        {
            bool retval = false;
            try
            {
                //
                if (ConfigurationManager.AppSettings["NotificationMode"].ToString() == "LOCAL")
                {
                    // instanciate smtp client
                    using (var client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"].ToString()))
                    {
                        client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                        client.PickupDirectoryLocation = ConfigurationManager.AppSettings["MailPickUpLocation"].ToString();
                        //
                        client.Send(msg);
                        retval = true;
                    }
                }
                else if (ConfigurationManager.AppSettings["NotificationMode"].ToString() == "PROD")
                {
                    // reading SMTP details from web.config
                    var SmtpServer = ConfigurationManager.AppSettings["SmtpServer"].ToString();
                    var SmtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"].ToString());
                    // instanciate smtp client
                    var client = new SmtpClient();
                    client.Host = SmtpServer;
                    client.Port = SmtpPort;
                    // specify smtp options
                    client.EnableSsl = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = true;
                    client.Send(msg);
                    retval = true;
                }
            }
            catch (Exception ex)
            {
                var x = ex.Message;
                retval = false;
            }
            //
            return retval;
        }
    }
}
