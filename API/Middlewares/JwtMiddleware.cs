using ResumableFileTransfer.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumableFileTransfer.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtOption _jwtOption;

        public JwtMiddleware(RequestDelegate next, JwtOption jwtOption)
        {
            _next = next;
            _jwtOption = jwtOption;
        }

        public async Task Invoke(HttpContext context)
        {
            //Get the upload token, which can be customized and extended
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                AttachTokenToContext(context, token);

            await _next(context);
        }

        private void AttachTokenToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtOption.SecretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // attach token to context on successful jwt validation
                context.Items[Consts.ENCRYPTED_SOFTWARE_TOKEN] = jwtToken.Claims.First(x => x.Type == Consts.ENCRYPTED_SOFTWARE_TOKEN).Value;
            }
            catch
            {
            }
        }
    }
}
