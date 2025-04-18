﻿using Microsoft.AspNetCore.Cors;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Cors;
using EnableCorsAttribute = System.Web.Http.Cors.EnableCorsAttribute;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Cấu hình CORS cho toàn bộ API
        var cors = new EnableCorsAttribute("*", "*", "*");
        config.EnableCors(cors);
        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        // Cấu hình Web API routes
        config.MapHttpAttributeRoutes();
        config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );
    }
}
