using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.AspNet.Identity;
using System.Configuration;

[assembly: OwinStartup(typeof(OMS_5D_Tech.Startup))]

namespace OMS_5D_Tech
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
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
        }
    }
}
