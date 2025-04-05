using System.Web.Mvc;
using OMS_5D_Tech.Interfaces;
using OMS_5D_Tech.Services;
using Unity;
using Unity.Lifetime;
using Unity.Mvc5;

namespace OMS_5D_Tech
{
    public static class UnityConfig                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<ICategoryService, CategoryService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IOrderService, OrderService>();
            container.RegisterType<IProductService, ProductService>(new HierarchicalLifetimeManager());
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<ICartService, CartService>();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}