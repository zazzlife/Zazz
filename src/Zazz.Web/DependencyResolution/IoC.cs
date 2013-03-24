// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Web.Hosting;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Data;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Services;

namespace Zazz.Web.DependencyResolution 
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            var rootDirectory = HostingEnvironment.MapPath("/");

            ObjectFactory.Initialize(x =>
                        {
                            x.Scan(scan =>
                                    {
                                        scan.TheCallingAssembly();
                                        scan.WithDefaultConventions();
                                    });

                            x.For<IStaticDataRepository>().Singleton().Use<StaticDataRepository>();
                            x.For<IUoW>().Use<UoW>();
                            
                            // Services
                            x.For<IAlbumService>().Use<AlbumService>();
                            x.For<IAuthService>().Use<AuthService>();
                            x.For<ICryptoService>().Use<CryptoService>();
                            x.For<IFacebookService>().Use<FacebookService>();
                            x.For<IFileService>().Use<FileService>();
                            x.For<IFollowService>().Use<FollowService>();
                            x.For<IPostService>().Use<PostService>();

                            x.For<IPhotoService>().Use<PhotoService>()
                             .Ctor<string>("rootPath").Is(rootDirectory);

                            x.For<IEventService>().Use<EventService>();
                            x.For<IUserService>().Use<UserService>();

                            // Helpers
                            x.For<IErrorHandler>().Use<ErrorHandler>();
                            x.For<IFacebookHelper>().Use<FacebookHelper>();
                            x.For<ILogger>().Use<Logger>();

#if DEBUG
                            x.For<IEmailService>().Use<FakeEmailService>();
#else
#endif

                        });
            return ObjectFactory.Container;
        }
    }
}