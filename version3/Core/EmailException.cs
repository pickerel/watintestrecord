using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;

namespace TestRecorder
{
    class EmailException
    {
        public static string EMAILUSER = "watinrecorder@gmail.com";
        private const string EMAILPASS = "w@t1n1";

        public void SendMail(string Body, string Subject, string FromEmail, bool CopyUser, bool Async)
        {
            var msg = new MailMessage();
            msg.To.Add(EMAILUSER);
            msg.Bcc.Add("daarond@gmail.com");

            if (CopyUser && !string.IsNullOrEmpty(FromEmail)) msg.CC.Add(FromEmail);
            
            if (string.IsNullOrEmpty(FromEmail)) FromEmail = EMAILUSER;
            
            msg.From = new MailAddress(FromEmail);
            if (Subject.StartsWith("Exception"))
            {
                try
                {
                    msg.Subject = "Recorder " + Application.ProductVersion + " " + Subject;
                }
                catch (Exception)
                {
                    msg.Subject = "Recorder " + Application.ProductVersion + " Other Exception";
                }
            }
            else msg.Subject = "Recorder " + Application.ProductVersion + " " + Subject + " " + Environment.MachineName + " " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            msg.SubjectEncoding = Encoding.UTF8;

            msg.Body = "From: "+FromEmail+Environment.NewLine+Body;
            msg.BodyEncoding = Encoding.UTF8;
            msg.IsBodyHtml = false;
            msg.Priority = MailPriority.High;

            //Add the Creddentials
            var client = new SmtpClient
                             {
                                 Credentials = new System.Net.NetworkCredential(EMAILUSER, EMAILPASS),
                                 Port = 587,
                                 Host = "smtp.gmail.com",
                                 EnableSsl = true
                             };
            client.SendCompleted += client_SendCompleted;
            object userState = msg;

            try
            {
                if (Async)
                {
                    client.SendAsync(msg, userState);
                }
                else
                {
                    client.Send(msg);
                }
            }
            catch (SmtpException ex)
            {
                MessageBox.Show(ex.Message, "Email Send Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SendMail(Exception exc, string FromEmail, string Comment, bool CopyUser, bool Async)
        {
            var sbBody = new StringBuilder();
            sbBody.AppendLine("Exception:");
            sbBody.AppendLine(exc.Message);
            sbBody.AppendLine("");

            if (Comment.Length > 0)
            {
                sbBody.AppendLine("Reproduction:");
                sbBody.AppendLine(Comment);
                sbBody.AppendLine("");
            }
            
            sbBody.AppendLine("Stack Trace:");
            sbBody.AppendLine(exc.StackTrace);

            SendMail(sbBody.ToString(), "Exception-"+exc.Message, FromEmail, CopyUser, Async);
        }

        static void client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var mail = (MailMessage)e.UserState;
            string subject = mail.Subject;

            if (e.Cancelled)
            {
                return;
            }
            if (e.Error != null)
            {
                string error = string.Format("[{0}] {1}", subject, e.Error);
                MessageBox.Show(error);                
            }
            else
            {
                MessageBox.Show("Message sent successfully");
            }
        }
    }
}
