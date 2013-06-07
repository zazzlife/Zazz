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


using System.Configuration;
using System.Web.Hosting;
using System.Web.Http.Filters;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Data;
using Zazz.Data.Repositories;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Infrastructure.Services;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;

namespace Zazz.Web.DependencyResolution 
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            var rootDirectory = HostingEnvironment.MapPath("/");
            var baseBlobAddress = ConfigurationManager.AppSettings["BaseBlobUrl"];
            var websiteAddress = ConfigurationManager.AppSettings["WebsiteAddress"];
            var blobConnString = ConfigurationManager.AppSettings["BlobConnectionString"];

            ObjectFactory.Initialize(x =>
                        {
                            x.Scan(scan =>
                                    {
                                        scan.TheCallingAssembly();
                                        scan.WithDefaultConventions();
                                    });

                            x.For<ICacheService>().Singleton().Use<CacheService>();
                            x.For<IStringHelper>().Singleton().Use<StringHelper>();
                            x.For<IStaticDataRepository>().Singleton().Use<StaticDataRepository>();
                            x.For<ICryptoService>().Singleton().Use<CryptoService>();
                            x.For<IApiAppRepository>().Singleton().Use<InMemoryApiAppRepository>();
                            x.For<IImageValidator>().Singleton().Use<ImageValidator>();
                            x.For<IQRCodeService>().Singleton().Use<QRCodeService>();

                            x.For<IDefaultImageHelper>().Singleton()
                             .Use<DefaultImageHelper>()
                             .Ctor<string>("baseAddress").Is(websiteAddress);

                            x.For<IStorageService>().Singleton()
                             .Use<AzureService>()
                             .Ctor<string>("storageConnString").Is(blobConnString);


                            x.For<IUoW>().HttpContextScoped().Use<UoW>();

                            // Services
                            x.For<IAlbumService>().Use<AlbumService>();
                            x.For<IAuthService>().Use<AuthService>();
                            x.For<IFacebookService>().Use<FacebookService>();
                            x.For<IFileService>().Use<FileService>();
                            x.For<IFollowService>().Use<FollowService>();
                            x.For<IPostService>().Use<PostService>();
                            x.For<IFacebookService>().Use<FacebookService>();
                            x.For<IEventService>().Use<EventService>();
                            x.For<IUserService>().Use<UserService>();
                            x.For<INotificationService>().Use<NotificationService>();
                            x.For<ICommentService>().Use<CommentService>();
                            x.For<ITagService>().Use<TagService>();
                            x.For<IWeeklyService>().Use<WeeklyService>();
                            x.For<IVoteService>().Use<VoteService>();

                            x.For<IPhotoService>().Use<PhotoService>()
                             .Ctor<string>("rootPath").Is(rootDirectory)
                             .Ctor<string>("baseBlobUrl").Is(baseBlobAddress);

                            // Helpers
                            x.For<IErrorHandler>().Use<ErrorHandler>();
                            x.For<IFacebookHelper>().Use<FacebookHelper>();
                            x.For<IFeedHelper>().Use<FeedHelper>();

                            x.For<ILogger>().Use<Logger>();

                            x.For<IFilterProvider>().Use<StructureMapFilterProvider>();
                            
                            x.For<IObjectMapper>().Use<ObjectMapper>();

#if DEBUG
                            x.For<IEmailService>().Use<FakeEmailService>();
#else
#endif

                        });
            return ObjectFactory.Container;
        }
    }
}