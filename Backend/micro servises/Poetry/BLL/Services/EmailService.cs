using BLL.User.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;

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
            // Get backend and frontend URLs from configuration
            string backendUrl = _configuration["AppUrl"] ?? "https://localhost:7001";
            string frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

            // Create both backend and frontend links
            var backendLink = $"{backendUrl}/api/identity/verify-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";

            var emailBody = new StringBuilder();
            emailBody.AppendLine("<html><body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;'>");
            emailBody.AppendLine("<div style='background-color: #8b5e3c; padding: 20px; text-align: center;'>");
            emailBody.AppendLine("<h1 style='color: white; margin: 0;'>Poetry Everyday</h1>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='padding: 20px; background-color: #f8f8f8; border: 1px solid #e0e0e0;'>");
            emailBody.AppendLine("<h2>Email Verification</h2>");
            emailBody.AppendLine("<p>Hello,</p>");
            emailBody.AppendLine("<p>Thank you for registering at Poetry Everyday. To complete your registration and activate your account, please verify your email address by clicking the button below:</p>");
            emailBody.AppendLine($"<div style='text-align: center; margin: 30px 0;'>");
            emailBody.AppendLine($"<a href='{backendLink}' style='background-color: #8b5e3c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Verify Email Address</a>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<p>Alternatively, you can copy and paste the following link into your browser:</p>");
            emailBody.AppendLine($"<p style='word-break: break-all;'><a href='{backendLink}'>{backendLink}</a></p>");
            emailBody.AppendLine("<p>This link will expire in 24 hours.</p>");
            emailBody.AppendLine("<p>If you did not create an account, you can safely ignore this email.</p>");
            emailBody.AppendLine("<p>Best regards,<br>Poetry Everyday Team</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='text-align: center; padding: 15px; font-size: 12px; color: #666;'>");
            emailBody.AppendLine("<p>This is an automated message. Please do not reply to this email.</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("</body></html>");

            return await _emailSender.SendEmailAsync(email, "Verify Your Email Address - Poetry Everyday", emailBody.ToString());
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string token)
        {
            // Get backend and frontend URLs from configuration
            string backendUrl = _configuration["AppUrl"] ?? "https://localhost:7001";
            string frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

            // Create reset password link
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

            var emailBody = new StringBuilder();
            emailBody.AppendLine("<html><body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;'>");
            emailBody.AppendLine("<div style='background-color: #8b5e3c; padding: 20px; text-align: center;'>");
            emailBody.AppendLine("<h1 style='color: white; margin: 0;'>Poetry Everyday</h1>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='padding: 20px; background-color: #f8f8f8; border: 1px solid #e0e0e0;'>");
            emailBody.AppendLine("<h2>Reset Your Password</h2>");
            emailBody.AppendLine("<p>Hello,</p>");
            emailBody.AppendLine("<p>We received a request to reset your password. Click the button below to create a new password:</p>");
            emailBody.AppendLine($"<div style='text-align: center; margin: 30px 0;'>");
            emailBody.AppendLine($"<a href='{resetLink}' style='background-color: #8b5e3c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Reset Password</a>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<p>Alternatively, you can copy and paste the following link into your browser:</p>");
            emailBody.AppendLine($"<p style='word-break: break-all;'><a href='{resetLink}'>{resetLink}</a></p>");
            emailBody.AppendLine("<p>This link will expire in 24 hours.</p>");
            emailBody.AppendLine("<p>If you did not request a password reset, you can safely ignore this email.</p>");
            emailBody.AppendLine("<p>Best regards,<br>Poetry Everyday Team</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='text-align: center; padding: 15px; font-size: 12px; color: #666;'>");
            emailBody.AppendLine("<p>This is an automated message. Please do not reply to this email.</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("</body></html>");

            return await _emailSender.SendEmailAsync(email, "Reset Your Password - Poetry Everyday", emailBody.ToString());
        }

        public async Task<bool> SendWelcomeEmailAsync(string email)
        {
            var emailBody = new StringBuilder();
            emailBody.AppendLine("<html><body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;'>");
            emailBody.AppendLine("<div style='background-color: #8b5e3c; padding: 20px; text-align: center;'>");
            emailBody.AppendLine("<h1 style='color: white; margin: 0;'>Poetry Everyday</h1>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='padding: 20px; background-color: #f8f8f8; border: 1px solid #e0e0e0;'>");
            emailBody.AppendLine("<h2>Welcome to Poetry Everyday!</h2>");
            emailBody.AppendLine("<p>Hello,</p>");
            emailBody.AppendLine("<p>Thank you for verifying your email and joining our community of poetry enthusiasts!</p>");
            emailBody.AppendLine("<p>With Poetry Everyday, you can:</p>");
            emailBody.AppendLine("<ul>");
            emailBody.AppendLine("<li>Create and publish your own poetry</li>");
            emailBody.AppendLine("<li>Read and discover works from other poets</li>");
            emailBody.AppendLine("<li>Save your favorite poems to collections</li>");
            emailBody.AppendLine("<li>Comment and engage with the community</li>");
            emailBody.AppendLine("</ul>");
            emailBody.AppendLine("<p>We hope you enjoy your journey with us!</p>");
            emailBody.AppendLine("<div style='text-align: center; margin: 30px 0;'>");

            // Add link to frontend
            string frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            emailBody.AppendLine($"<a href='{frontendUrl}' style='background-color: #8b5e3c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold;'>Visit Poetry Everyday</a>");

            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<p>Best regards,<br>Poetry Everyday Team</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='text-align: center; padding: 15px; font-size: 12px; color: #666;'>");
            emailBody.AppendLine("<p>This is an automated message. Please do not reply to this email.</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("</body></html>");

            return await _emailSender.SendEmailAsync(email, "Welcome to Poetry Everyday", emailBody.ToString());
        }

        public async Task<bool> SendNotificationEmailAsync(string email, string subject, string message)
        {
            var emailBody = new StringBuilder();
            emailBody.AppendLine("<html><body style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto;'>");
            emailBody.AppendLine("<div style='background-color: #8b5e3c; padding: 20px; text-align: center;'>");
            emailBody.AppendLine("<h1 style='color: white; margin: 0;'>Poetry Everyday</h1>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='padding: 20px; background-color: #f8f8f8; border: 1px solid #e0e0e0;'>");
            emailBody.AppendLine("<p>Hello,</p>");
            emailBody.AppendLine($"<p>{message}</p>");
            emailBody.AppendLine("<p>Best regards,<br>Poetry Everyday Team</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("<div style='text-align: center; padding: 15px; font-size: 12px; color: #666;'>");
            emailBody.AppendLine("<p>This is an automated message. Please do not reply to this email.</p>");
            emailBody.AppendLine("</div>");
            emailBody.AppendLine("</body></html>");

            return await _emailSender.SendEmailAsync(email, subject, emailBody.ToString());
        }
    }
}