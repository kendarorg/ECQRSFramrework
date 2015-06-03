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


using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ECQRS.Commons;
using ECQRS.Commons.Repositories;
using ECQRS.SqlServer.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ionic.Zip;
using UserManager.Castle;
using UserManager.Core.Applications.ReadModel;
using UserManager.Core.Users.ReadModel;
using UserManager.Core.Organizations.ReadModel;

namespace UserManager
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer _container;

        protected void Application_Start()
        {
            InitializeDbFile();

            _container = new WindsorContainer().Install(FromAssembly.This());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(a => WebApiConfig.Register(a, _container));
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BootstrapContainer();

        }

        private static void InitializeDbFile()
        {
            var appData = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            var mdfFile = Path.Combine(appData, "UserManagerDb.mdf");
            if (!File.Exists(mdfFile))
            {
                var mdfZip = Path.Combine(appData, "UserManagerDb.zip");
                using (var zip = ZipFile.Read(mdfZip))
                {
                    zip.ExtractAll(appData, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        private static void BootstrapContainer()
        {
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel, false));
            var controllerFactory = new WindsorControllerFactory(_container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            InitializeDbFile();
            InitalizeDb();

            _container.Install(
               FromAssembly.InDirectory(new AssemblyFilter(ReflectionHelper.AssemblyDirectory)));
        }

        private static void InitalizeDb()
        {
            var reconf = new DapperRepositoryConfiguration(ConfigurationManager.ConnectionStrings["Data"]);
            _container.Register(Component.For<IRepositoryConfiguration>()
                .Instance(reconf)
                .LifestyleSingleton());

            using (var cn = new SqlConnection(reconf.ConnectionString))
            {
                RegisterRepository<UserListItem>(cn);
                RegisterRepository<UserDetailItem>(cn);

                RegisterRepository<ApplicationDetailItem>(cn);
                RegisterRepository<ApplicationListItem>(cn);
                RegisterRepository<ApplicationPermissionItem>(cn);
                RegisterRepository<ApplicationRoleItem>(cn);
                RegisterRepository<ApplicationRolePermissionItem>(cn);

                RegisterRepository<OrganizationDetailItem>(cn);
                RegisterRepository<OrganizationListItem>(cn);
                RegisterRepository<OrganizationRoleItem>(cn);
                RegisterRepository<OrganizationGroupItem > (cn);
                RegisterRepository<OrganizationGroupRoleItem>(cn);
                RegisterRepository<OrganizationUserItem>(cn);
                RegisterRepository<OrganizationGroupUserItem>(cn);
            }
        }

        private static void RegisterRepository<T>(SqlConnection cn) where T : IEntity, new()
        {
            _container.Register(Component.For<IRepository<T>>()
                .ImplementedBy<DapperRepository<T>>()
                .LifestyleTransient());
            DapperRepository.BuildDefaultTable<T>(cn);
        }

        protected void Application_End()
        {
            _container.Dispose();
        }
    }
}
