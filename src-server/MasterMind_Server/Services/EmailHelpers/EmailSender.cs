using DotNetEnv;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;

namespace MasterMind_Server.Services.EmailHelpers
{
    internal static class EmailSender
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(EmailSender));

        public static void Send(string email, string subject, string body)
        {
            using (MailMessage emailMessage = BuildMessage(email, subject, body))
            using (SmtpClient smtp = BuildSmtpClient())
            {
                try
                {
                    smtp.Send(emailMessage);
                }
                catch (SmtpFailedRecipientException ex)
                {
                    logger.Warn(ex);
                    throw BuildFault(ErrorMessages.SendEmailError);
                }
                catch (SmtpException ex)
                {
                    logger.Error(ex);
                    throw BuildFault(ErrorMessages.SmtpServerError);
                }
            }
        }

        private static MailMessage BuildMessage(string email, string subject, string body)
        {
            MailAddress to = ParseRecipient(email);
            MailAddress from = ReadSmtpSetting(
                () => new MailAddress(Environment.GetEnvironmentVariable("SMTP_SENDER")));

            return new MailMessage(from, to)
            {
                Subject = subject,
                Body = body
            };
        }

        private static MailAddress ParseRecipient(string email)
        {
            try
            {
                return new MailAddress(email);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
            {
                logger.Warn(ex);
                throw BuildFault(ErrorMessages.SendEmailError);
            }
        }

        private static SmtpClient BuildSmtpClient()
        {
            return ReadSmtpSetting(() => new SmtpClient()
            {
                Host = Environment.GetEnvironmentVariable("SMTP_SERVER"),
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("SMTP_LOGIN"),
                    Environment.GetEnvironmentVariable("SMTP_KEY")),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true
            });
        }

        private static T ReadSmtpSetting<T>(Func<T> build)
        {
            try
            {
                Env.Load();
                return build();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is IOException)
            {
                logger.Error("The SMTP configuration is missing or invalid.", ex);
                throw BuildFault(ErrorMessages.SmtpServerError);
            }
        }

        private static FaultException<ErrorMessage> BuildFault(ErrorMessages reason)
        {
            return new FaultException<ErrorMessage>(new ErrorMessage { Message = reason.ToString() });
        }
    }
}
