using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Configuration;
using System.Data.SqlClient;

namespace OMS_5D_Tech
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            UnityConfig.RegisterComponents();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);              
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            try
            {
                var context = HttpContext.Current;
                var username = context.User?.Identity?.Name ?? "Guest";
                var ip = context.Request.UserHostAddress;
                var url = context.Request.Url.ToString();
                var method = context.Request.HttpMethod;
                var userAgent = context.Request.UserAgent;

                LogAccess(username, ip, url, method, userAgent);
            }
            catch
            {

            }
        }

        private void LogAccess(string username, string ip, string url, string method, string userAgent)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString))
            {
                var cmd = new SqlCommand(@"
                INSERT INTO AccessLogs (Username, IPAddress, UrlAccessed, Method, UserAgent)
                VALUES (@Username, @IPAddress, @UrlAccessed, @Method, @UserAgent)", conn);

                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@IPAddress", ip);
                cmd.Parameters.AddWithValue("@UrlAccessed", url);
                cmd.Parameters.AddWithValue("@Method", method);
                cmd.Parameters.AddWithValue("@UserAgent", userAgent);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}