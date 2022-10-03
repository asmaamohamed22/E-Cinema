using E_Cinema.Services;
using Microsoft.AspNetCore.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web;
using System;

namespace E_Cinema.Services
{
    public static class SendGridAPI
    {
        public static async Task<bool> Execute(string userEmail, string userName, string plainTextContent, string htmlContent, string subject)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "Asmaa");
            var to = new EmailAddress(userEmail, userName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return await Task.FromResult(true);
        }
    }
}


//if (result.Succeeded)
//{
//    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//    var confirmLink = Url.Action("RegistrationConfirm", "Account", new
//    { ID = user.Id, Token = HttpUtility.UrlEncode(token) }, Request.Scheme);
//    var txt = "Please Confirm your registration at our site!";
//    var link = "<a href =\"" + confirmLink + "\"> Confirm Registration </a>";
//    var title = "Registration Confirm";
//    if (await SendGridAPI.Execute(user.Email, user.UserName, txt, link, title))
//    {
//        return Ok("Registration Complete");
//    }