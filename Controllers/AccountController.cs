using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TodoAPI.Config;
using TodoAPI.Controllers.Resources;

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly JWTSettings optionsAccessor;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> sighInManager,
            IOptions<JWTSettings> optionsAccessor
            )
        {
            this.userManager = userManager;
            this.signInManager = sighInManager;
            this.optionsAccessor = optionsAccessor.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Credentials credentials)
        {
            if (ModelState.IsValid) {
                var user = new IdentityUser { UserName = credentials.Email, Email = credentials.Email};
                var result = await userManager.CreateAsync(user, credentials.Password);
                if (result.Succeeded) {
                    await signInManager.SignInAsync(user, isPersistent:false);
                    return new JsonResult(
                        new Dictionary<string, object>{
                            { "access_token", GetAccessToken(credentials.Email) },
                            { "id_token", GetIdToken(user) }
                        }
                    );
                }

                return Errors(result);
            }

            return Error("Unexpected error");
        }

        private string GetIdToken(IdentityUser user)
        {
            var payload = new Dictionary<string, object> {
                { "id", user.Id },
                { "sub", user.Email },
                { "email", user.Email },
                { "emailConfirmed", user.EmailConfirmed }
            };
            return GetToken(payload);
        }

        private object GetAccessToken(string email)
        {
            var payload = new Dictionary<string, object> {
                { "sub", email },
                { "email", email }
            };

            return GetToken(payload);
        }

        private string GetToken(Dictionary<string, object> payload)
        {
            var secret = optionsAccessor.SecretKey;

            payload.Add("iss", optionsAccessor.Issuer);
            payload.Add("aud", optionsAccessor.Audience);
            payload.Add("nbf", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("iat", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("exp", ConvertToUnixTimestamp(DateTime.Now.AddDays(7)));
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder.Encode(payload, secret);
        }

        private object ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        private IActionResult Errors(IdentityResult result)
        {
            var items = result.Errors
            .Select(x => x.Description)
            .ToArray();

            return new JsonResult(items) { StatusCode = 400 };
        }

        private IActionResult Error(string v)
        {
            return new JsonResult(v) { StatusCode = 400 };
        }
    }
}