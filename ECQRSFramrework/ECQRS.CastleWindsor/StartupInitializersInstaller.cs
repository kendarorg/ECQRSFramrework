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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Facilities.TypedFactory;
using ECQRS.Commons.Events;
using ECQRS.Commons.Commands;
using ECQRS.Commons.Bus;

namespace ECQRS.CastleWindsor
{
    public class StartupInitializersInstaller:IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var initializers = GetAllStartupInitializers();
            for (int index = 0; index < initializers.Count; index++)
            {
                var intializer = initializers[index];
                var id = Guid.NewGuid().ToString();
                container.Register(Component.For(intializer).Named(id));
                var instance  = container.Resolve(id,intializer);
                intializer.GetMethod("Initialize").Invoke(instance, new object[] {});
            }
            container.Kernel.AddFacility<TypedFactoryFacility>();

            container.Register(Classes.FromAssemblyInThisApplication()
                .BasedOn<IEventHandler>()
                .WithServiceAllInterfaces()
                .LifestyleSingleton());
            
            container.Register(Classes.FromAssemblyInThisApplication()
                .BasedOn<ICommandHandler>()
                .WithServiceAllInterfaces()
                .LifestyleSingleton());

            var bus = container.Resolve<IBus>();
            bus.CreateQueue("ecqrs.events");
            bus.CreateQueue("ecqrs.commands");

            InitalizeHandlers(container, typeof (ICommandHandler), typeof (Command), bus, "ecqrs.commands");
            InitalizeHandlers(container, typeof(IEventHandler), typeof(Event), bus, "ecqrs.events");

            bus.Start();
        }

        private static void InitalizeHandlers(IWindsorContainer container, Type typeHandler, Type typeSeeked, IBus bus,
            string queueId)
        {
            foreach (var commandHandler in container.ResolveAll(typeHandler))
            {
                var types = commandHandler.GetType().GetMethods()
                    .Where(a => a.Name == "Handle" && a.GetParameters().Count() == 1)
                    .Where(a => typeSeeked.IsAssignableFrom(a.GetParameters()[0].ParameterType))
                    .Select(a => a.GetParameters()[0].ParameterType);
                foreach (var type in types)
                {
                    bus.Subscribe(type, (IBusHandler) commandHandler, queueId);
                }
            }
        }


        private static string AssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        

        private static List<Type> GetAllStartupInitializers()
        {
            foreach (var file in Directory.GetFiles(AssemblyDirectory(), "*.dll"))
            {
                try
                {
                    Assembly.LoadFrom(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(s => s.GetTypes())
                .Where(a=>a.Name=="StartupInitializer").ToList();
        }
    }
}