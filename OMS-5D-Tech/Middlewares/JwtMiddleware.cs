using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace OMS_5D_Tech.Middlewares
{
    public class JwtMiddleware : OwinMiddleware
    {
        private readonly JwtService _jwtService;

        public JwtMiddleware(OwinMiddleware next, JwtService jwtService) : base(next)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public override async Task Invoke(IOwinContext context)
        {
            var token = context.Request.Headers.Get("Authorization")?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                await Next.Invoke(context);
                return;
            }

            try
            {
                var principal = _jwtService.ValidateToken(token);

                if (principal?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
                {
                    var claims = new ClaimsIdentity(identity.Claims, "jwt");
                    context.Request.User = new ClaimsPrincipal(claims);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token đã hết hạn.");
                return;
            }
            catch (SecurityTokenException ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Lỗi xác thực JWT: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Lỗi middleware JWT: {ex.Message}");
                return;
            }

            await Next.Invoke(context);
        }
    }
}
