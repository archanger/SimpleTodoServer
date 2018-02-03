using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TodoAPI.Config;
using TodoAPI.Controllers.Resources;
using TodoAPI.Core;
using TodoAPI.Core.Models;

namespace TodoAPI.Controllers
{
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserRepository UserRepository;
        private readonly IMapper Mapper;
        private readonly IUnitOfWork UnitOfWork;
        private readonly HostEmailSettings hostEmailSettings;

        public AccountController(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IOptions<HostEmailSettings> hostEmailSettings,
            IMapper mapper)
        {
            this.UnitOfWork = unitOfWork;
            this.hostEmailSettings = hostEmailSettings.Value;
            this.Mapper = mapper;
            this.UserRepository = userRepository;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterResource account)
        {
            var user = Mapper.Map<User>(account);
            var messages = new List<string>();

            if (await UserRepository.FindByName(user.Username) != null) {
                messages.Add($"User with {user.Username} already Exists");
            }

            if (await UserRepository.FindByEmail(user.Email) != null) {
                messages.Add($"User with {user.Email} already Exists");
            }

            if (messages.Count > 0) {
                return BadRequest(new { Error = messages });
            }

            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.Now;
            user.ModifiedAt = DateTime.Now;

            UserRepository.Add(user);

            await UnitOfWork.CompleteAsync();

            SendConfirmationEmail(user);

            return Ok(user);
        }

        [HttpGet("confirm/{confirmationId}")]
        public async Task<IActionResult> Confirm(string confirmationId)
        {
            await UserRepository.Confirm(confirmationId);
            await UnitOfWork.CompleteAsync();
            return Ok(new { Confirmed = true });
        }

        private void SendConfirmationEmail(User user)
        {
            using (var mm = new MailMessage(
                new MailAddress(hostEmailSettings.EmailHost, hostEmailSettings.DisplayName), 
                new MailAddress(user.Email)))
            {
                mm.Subject = "Account Confirmation";
                string body = "<a href='" + GetConfirmationUrl() + "/" + user.EmailConfirmation.ConfiramtionCode + "'>" + "Click here!" + "</a>";
                mm.Body = body;
                mm.IsBodyHtml = true;
                var smtp = new SmtpClient(hostEmailSettings.SmtpHost);
                smtp.EnableSsl = true;
                var creds = new NetworkCredential(
                    hostEmailSettings.CredentialUserName,
                    hostEmailSettings.CredentialPassword
                );
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = creds;
                smtp.Port = 587;
                smtp.Send(mm);
            }
        }

        private string GetConfirmationUrl() 
        {
            return Request.Scheme + "://" + Request.Host + "/api/account/confirm";
        }
    }
}