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




using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Diary.CQRS.Reporting;
using Diary.CQRS.Web.Infrastructure;
using ECQRS.Commons.Repositories;
using ECQRS.SqlServer.Repositories;
using System.Data.SqlClient;
using Ionic.Zip;

namespace Diary.CQRS.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static IWindsorContainer _container;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitializeDbFile();
            BootstrapContainer();
        }

        private static void InitializeDbFile()
        {
            var appData = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var mdfFile = Path.Combine(appData, "DiaryCQRSDb.mdf");
            if (!File.Exists(mdfFile))
            {
                var mdfZip = Path.Combine(appData, "DiaryCQRSDb.zip");
                using (var zip = ZipFile.Read(mdfZip))
                {
                    zip.ExtractAll(appData, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        private static void BootstrapContainer()
        {
            _container = new WindsorContainer().Install(FromAssembly.This());
            var reconf = new DapperRepositoryConfiguration(ConfigurationManager.ConnectionStrings["Data"]);
            _container.Register(Component.For<IRepositoryConfiguration>()
                .Instance(reconf)
                .LifestyleSingleton());
 
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel, false));
            var controllerFactory = new WindsorControllerFactory(_container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            InitalizeDb();

            _container.Install(
               FromAssembly.InDirectory(new AssemblyFilter(AssemblyDirectory)));



        }

        private static void InitalizeDb()
        {
            var reconf = _container.Resolve<IRepositoryConfiguration>();
            _container.Register(Component.For<IRepository<DiaryItemDto>>()
                .ImplementedBy<DapperRepository<DiaryItemDto>>()
                .LifestyleTransient());

            using (var cn = new SqlConnection(reconf.ConnectionString))
            {
                DapperRepository.BuildDefaultTable<DiaryItemDto>(cn);
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