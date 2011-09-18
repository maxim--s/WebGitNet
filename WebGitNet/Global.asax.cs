﻿//-----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="(none)">
//  Copyright © 2011 John Gietzen. All rights reserved.
// </copyright>
// <author>John Gietzen</author>
//-----------------------------------------------------------------------

namespace WebGitNet
{
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using Castle.MicroKernel.SubSystems.Configuration;

    public partial class WebGitNetApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer container;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Browse Index",
                "browse",
                new { controller = "Browse", action = "Index" });

            routes.MapRoute(
                "View Repo",
                "browse/{repo}",
                new { controller = "Browse", action = "ViewRepo" });

            routes.MapRoute(
                "View Repo Impact",
                "browse/{repo}/impact",
                new { controller = "Browse", action = "ViewRepoImpact" });

            routes.MapRoute(
                "View Tree",
                "browse/{repo}/tree/{object}/{*path}",
                new { controller = "Browse", action = "ViewTree", path = UrlParameter.Optional });

            routes.MapRoute(
                "View Blob",
                "browse/{repo}/blob/{object}/{*path}",
                new { controller = "Browse", action = "ViewBlob", path = UrlParameter.Optional });

            routes.MapRoute(
                "View Commit",
                "browse/{repo}/commit/{object}",
                new { controller = "Browse", action = "ViewCommit" });

            routes.MapRoute(
                "View Commits",
                "browse/{repo}/commits",
                new { controller = "Browse", action = "ViewCommits" });

            routes.MapRoute(
                "Get */info/refs",
                "git/{*url}",
                new { controller = "File", action = "GetInfoRefs" },
                new { url = @"(.*?)/info/refs" });

            routes.MapRoute(
                "Post */git-upload-pack",
                "git/{*url}",
                new { controller = "ServiceRpc", action = "UploadPack" },
                new { url = @"(.*?)/git-upload-pack" });

            routes.MapRoute(
                "Post */git-receive-pack",
                "git/{*url}",
                new { controller = "ServiceRpc", action = "ReceivePack" },
                new { url = @"(.*?)/git-receive-pack" });

            routes.MapRoute(
                "File Access",
                "git/{*url}",
                new { controller = "File", action = "Fetch" });

            routes.MapRoute(
                "Default",
                "{controller}/{action}",
                new { controller = "Browse", action = "Index" });
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            Bootstrap();
        }

        protected void Application_End()
        {
            container.Dispose();
        }

        private static void Bootstrap()
        {
            var directoryFilter = new AssemblyFilter(HostingEnvironment.MapPath("~/Plugins"));

            container = new WindsorContainer()
                        .Install(new AssemblyInstaller())
                        .Install(FromAssembly.InDirectory(directoryFilter));
            var controllerFactory = new WindsorControllerFactory(container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        }

        private class AssemblyInstaller : IWindsorInstaller
        {
            public void Install(IWindsorContainer container, IConfigurationStore configurationStore)
            {
                container.Register(AllTypes.FromThisAssembly()
                                           .BasedOn<IController>()
                                           .Configure(c => c.LifeStyle.Transient));
            }
        }
    }
}
