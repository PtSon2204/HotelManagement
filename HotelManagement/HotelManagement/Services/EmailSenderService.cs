using System.Net;
using System.Net.Mail;

namespace HotelManagement.Services
{
    public class EmailSenderService
    {
        private readonly IConfiguration _configuration;

        public EmailSenderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpAsync(string toEmail, string username, string otpCode)
        {
            await SendEmailAsync(
                toEmail,
                "Ma OTP kich hoat tai khoan",
                $"""
Xin chao {username},

Ma OTP kich hoat tai khoan cua ban la: {otpCode}

Ma nay co hieu luc trong 10 phut.
Neu ban khong thuc hien dang ky, vui long bo qua email nay.
""");
        }

        public async Task SendNewPasswordAsync(string toEmail, string username, string newPassword)
        {
            await SendEmailAsync(
                toEmail,
                "Mat khau moi cua ban",
                $"""
Xin chao {username},

He thong da tao mat khau moi cho tai khoan cua ban.

Mat khau moi: {newPassword}

Vui long dang nhap lai va doi mat khau ngay sau khi vao he thong.
""");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSection = _configuration.GetSection("Smtp");
            var host = smtpSection["Host"];
            var portValue = smtpSection["Port"];
            var senderEmail = smtpSection["SenderEmail"];
            var senderName = smtpSection["SenderName"] ?? "Hotel Management";
            var password = smtpSection["Password"];
            var enableSslValue = smtpSection["EnableSsl"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portValue) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Cấu hình SMTP chưa đầy đủ. Hãy cập nhật phần Smtp trong appsettings.json.");
            }

            if (!int.TryParse(portValue, out var port))
            {
                throw new InvalidOperationException("Cổng SMTP không hợp lệ.");
            }

            var enableSsl = true;
            if (!string.IsNullOrWhiteSpace(enableSslValue))
            {
                bool.TryParse(enableSslValue, out enableSsl);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = enableSsl
            };

            await client.SendMailAsync(message);
        }
    }
}
