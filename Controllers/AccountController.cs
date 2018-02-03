using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Config;
using TodoAPI.Controllers.Resources;
using TodoAPI.Core;
using TodoAPI.Core.Models;
using TodoAPI.Helpers;

namespace TodoAPI.Controllers
{
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserRepository UserRepository;
        private readonly IMapper Mapper;
        private readonly IUnitOfWork UnitOfWork;
        private readonly JwtSettings jwtOptions;
        private readonly HostEmailSettings hostEmailSettings;

        public AccountController(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IOptions<HostEmailSettings> hostEmailSettings,
            IOptions<JwtSettings> jwtOptions,
            IMapper mapper)
        {
            this.UnitOfWork = unitOfWork;
            this.jwtOptions = jwtOptions.Value;
            this.hostEmailSettings = hostEmailSettings.Value;
            this.Mapper = mapper;
            this.UserRepository = userRepository;
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpGet("confirm/{confirmationId}")]
        public async Task<IActionResult> Confirm(string confirmationId)
        {
            await UserRepository.Confirm(confirmationId);
            await UnitOfWork.CompleteAsync();
            return Ok(new { Confirmed = true });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] AuthorizationResource authorization)
        {
            var user = await UserRepository.FindByName(authorization.UserName);
            if (user == null || user.PasswordHash != Hashing.HashPassword(authorization.Password)) {
                return Unauthorized();
            }
            
            var token = BuildToken(user);
            return Ok(new { Token = token });
        }

        private string BuildToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                // new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
            };

            var token = new JwtSecurityToken(
                jwtOptions.Issuer,
                jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
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