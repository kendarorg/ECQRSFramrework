// ===========================================================
// Copyright (c) 2014-2015, Enrico Da Ros/kendar.org
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ===========================================================


using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CQRSGui.Infrastructure;
using ECQRS.Commons.Repositories;
using ECQRS.SqlServer.Repositories;
using SimpleCQRS;
using System;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Ionic.Zip;
using ECQRS.MassTransit.Bus;
using MassTransit;

namespace CQRSGui
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static IWindsorContainer _container;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        private static void InitializeDbFile()
        {
            var appData = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var mdfFile = Path.Combine(appData, "CQRSGuiDb.mdf");
            if (!File.Exists(mdfFile))
            {
                var mdfZip = Path.Combine(appData, "CQRSGuiDb.zip");
                using (var zip = ZipFile.Read(mdfZip))
                {
                    zip.ExtractAll(appData, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            InitializeDbFile();
            RegisterRoutes(RouteTable.Routes);
            BootstrapContainer();
        }

        private static void BootstrapContainer()
        {
            _container = new WindsorContainer().Install(FromAssembly.This());
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel, false));

            //http://www.codewrecks.com/blog/index.php/2012/08/03/quick-start-on-mass-transit-and-msmq-on-windows/
            var msmq = ConfigurationManager.AppSettings["MSMQRoot"];
            var massTransitConfiguration = new MassTransitConfiguration(msmq, (sbc) =>
            {
                sbc.UseMsmq(
                    bc =>
                    {
                        bc.UseMulticastSubscriptionClient();
                        bc.UseSubscriptionService("msmq://localhost/mt_subscriptions");
                    }

                 );
                sbc.UseXmlSerializer();
                //sbc.UseSubscriptionService("msmq://localhost/mt_subscriptions");
                //sbc.VerifyMsmqConfiguration();
                //sbc.UseMulticastSubscriptionClient();
            });
            _container.Register(Component.For<IMassTransitConfiguration>()
                .Instance(massTransitConfiguration)
                .LifestyleSingleton());

            var controllerFactory = new WindsorControllerFactory(_container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            InitalizeDb();



            _container.Install(
                FromAssembly.InDirectory(new AssemblyFilter(AssemblyDirectory)));
        }

        private static void InitalizeDb()
        {

            var reconf = new DapperRepositoryConfiguration(ConfigurationManager.ConnectionStrings["Data"]);
            _container.Register(Component.For<IRepositoryConfiguration>()
                .Instance(reconf)
                .LifestyleSingleton());

            _container.Register(Component.For<IRepository<InventoryItemListDto>>()
                .ImplementedBy<DapperRepository<InventoryItemListDto>>()
                .LifestyleTransient());
            _container.Register(Component.For<IRepository<InventoryItemDetailsDto>>()
                .ImplementedBy<DapperRepository<InventoryItemDetailsDto>>()
                .LifestyleTransient());

            using (var cn = new SqlConnection(reconf.ConnectionString))
            {
                DapperRepository.BuildDefaultTable<InventoryItemListDto>(cn);
                DapperRepository.BuildDefaultTable<InventoryItemDetailsDto>(cn);
            }
        }

        protected void Application_End()
        {
            _container.Dispose();
        }

        static public string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;

                var uri = new UriBuilder(codeBase);

                var path = Uri.UnescapeDataString(uri.Path);

                return Path.GetDirectoryName(path);
            }
        }
    }
}