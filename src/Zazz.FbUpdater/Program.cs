using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using StructureMap;
using Zazz.Core.Interfaces;
using Zazz.Data;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Services;

namespace Zazz.FbUpdater
{
    class Program
    {
        private const int SECOND = 1000;
        private const int MINUTE = 1000*60;
        private static bool _isPageUpdateTaskRunning = false;
        private static IContainer _container;
        private static string _webRootDirectory;

        static void Main(string[] args)
        {
            _webRootDirectory = ConfigurationManager.AppSettings["WebRootDirectory"];
            if (String.IsNullOrEmpty(_webRootDirectory))
            {
                Console.WriteLine("Web Root Directory is not specified.\r\n press enter to exit");
                Console.ReadLine();
            }

            Console.WriteLine("Specified Web Root Directory is: {0}", _webRootDirectory);


            RegisterIoC();

            var pageUpdateTimer = new Timer(MINUTE*10);
            pageUpdateTimer.Elapsed += RunPageUpdate;
            pageUpdateTimer.Start();

            Console.WriteLine("Type exit to exit!");
            var input = "";
            while (!input.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                input = Console.ReadLine();
        }

        private static void RegisterIoC()
        {
            _container = new Container(x =>
                                       {
                                           x.For<IUoW>().Use<UoW>();

                                           // Services
                                           x.For<IAlbumService>().Use<AlbumService>();
                                           x.For<IAuthService>().Use<AuthService>();
                                           x.For<ICryptoService>().Use<CryptoService>();
                                           x.For<IFacebookService>().Use<FacebookService>();
                                           x.For<IFileService>().Use<FileService>();
                                           x.For<IFollowService>().Use<FollowService>();
                                           x.For<IPostService>().Use<PostService>();
                                           x.For<IFacebookService>().Use<FacebookService>();
                                           x.For<IPhotoService>().Use<PhotoService>()
                                            .Ctor<string>("rootPath").Is(_webRootDirectory);

                                           x.For<IEventService>().Use<EventService>();
                                           x.For<IUserService>().Use<UserService>();

                                           // Helpers
                                           x.For<IErrorHandler>().Use<ErrorHandler>();
                                           x.For<IFacebookHelper>().Use<FacebookHelper>();
                                           x.For<ILogger>().Use<Logger>();
                                       });
        }

        private static async void RunPageUpdate(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if(_isPageUpdateTaskRunning)
                return;

            _isPageUpdateTaskRunning = true;



            _isPageUpdateTaskRunning = false;
        }
    }
}
