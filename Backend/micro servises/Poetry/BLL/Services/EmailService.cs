using BLL.User.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public EmailService(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<bool> SendVerificationEmailAsync(string email, string token, string userId)
        {
            var link = $"{_configuration["AppUrl"]}/verify-email?userId={userId}&token={Uri.EscapeDataString(token)}";
            var body = $"<p>Привет! Подтверди свою почту, перейдя по <a href=\"{link}\">ссылке</a>.</p>";
            return await _emailSender.SendEmailAsync(email, "Подтверждение почты", body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string token)
        {
            var link = $"{_configuration["AppUrl"]}/reset-password?token={token}&email={email}";
            var body = $"<p>Для сброса пароля нажми <a href=\"{link}\">сюда</a>.</p>";
            return await _emailSender.SendEmailAsync(email, "Сброс пароля", body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string email)
        {
            var body = "<p>Добро пожаловать в Poetry Everyday! Мы рады видеть тебя :)</p>";
            return await _emailSender.SendEmailAsync(email, "Добро пожаловать!", body);
        }

        public async Task<bool> SendNotificationEmailAsync(string email, string subject, string message)
        {
            var body = $"<p>{message}</p>";
            return await _emailSender.SendEmailAsync(email, subject, body);
        }
    }

}