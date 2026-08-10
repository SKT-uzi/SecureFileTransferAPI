using ResumableFileTransfer.API.Interfaces;
using ResumableFileTransfer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ResumableFileTransfer.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Base.ApiControllerBase
    {
        private readonly IAPIConfiguration _configuration;
        private readonly IDBContext _dbContext;

        public AuthController(IAPIConfiguration configuration, IDBContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        /// <summary>Get jwt token and other info.</summary>
        /// <param name="body">Software token.</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] AuthRequestBody body)
        {
            var token = body.Token;
            var tokenInfo = _dbContext.GetTokenInfo(token);

            // a 401 code will be returned only if jwt token validation failed
            // in other case, a 403 code will be returned
            if (tokenInfo == null || tokenInfo.AccountID == 0)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new BaseResponseResult { Message = "Software token is invalid" });
            }

            var response = new AuthResponseResult()
            {
                JWTToken = GenerateJWTToken(token),
                CompanyName = tokenInfo.AccountName,
                PhoneNumber = tokenInfo.PhoneNumber,
                AllowedExtensions = tokenInfo.FileFormat.Split(",")
            };

            return new JsonResult(response);
        }

        private string GenerateJWTToken(string softwareToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.JWTSecretKey);

            var claims = new List<Claim>();
            claims.Add(new Claim(Consts.ENCRYPTED_SOFTWARE_TOKEN, CommonHelper.Encrypt(softwareToken)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_configuration.JWTExpireMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
