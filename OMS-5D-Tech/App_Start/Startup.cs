using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using System.Text;
using System;
using Microsoft.Owin.Security;
using OMS_5D_Tech.Middlewares;

[assembly: OwinStartup(typeof(OMS_5D_Tech.Startup))]

namespace OMS_5D_Tech
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var key = Encoding.UTF8.GetBytes("NBTrvShWqPMJCjwKht3gucS7gTM4TY1vsBXW6dM588ViBMwqLnlsX5nFTf67jS_vTaDzidy6HlUza4rmBb67Lg==");
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/auth/login")
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["GoogleClientId"],
                ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"],
                CallbackPath = new PathString("/signin-google"),
                Scope = { "email", "profile", "openid" },
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie 
            });

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
            });

            var jwtService = new JwtService();
            app.Use<JwtMiddleware>(jwtService);
        }
    }
}
