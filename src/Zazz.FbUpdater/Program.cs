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

            Console.WriteLine("* Specified Web Root Directory is: {0}", _webRootDirectory);

            RegisterIoC();

            var pageUpdateTimer = new Timer(MINUTE*10);
            pageUpdateTimer.Elapsed += RunPageUpdate;
            pageUpdateTimer.Start();

            Console.WriteLine("* Type \"stop\" to exit.");
            Console.WriteLine("* Type \"run\" to force run.");
            Console.Write("\r" + new String('=', Console.WindowWidth) + "\r");

            Console.WriteLine();

            Console.WriteLine("Status:");

            SetStatus("Waiting...");

            Console.WriteLine();
            Console.WriteLine();
            Console.Write("\r" + new String('=', Console.WindowWidth) + "\r");

            var input = "";
            while (!input.Equals("stop", StringComparison.InvariantCultureIgnoreCase))
            {
                input = Console.ReadLine();
                if (input.Equals("run", StringComparison.InvariantCultureIgnoreCase))
                {
                    RunPageUpdate(null, null);
                }

                Console.SetCursorPosition(0, 9);
                Console.Write("\r" + new String(' ', Console.WindowWidth) + "\r");
                Console.SetCursorPosition(0, 9);
            }
                
        }

        public static void SetStatus(string message)
        {
            var currentCursorTop = Console.CursorTop;
            var currentCursorLeft = Console.CursorLeft;

            Console.SetCursorPosition(0, 6);
            Console.Write("\r" + new String(' ', Console.WindowWidth));

            Console.SetCursorPosition(0, 6);
            Console.Write("{0} - {1}", DateTime.Now.ToShortTimeString(), message);

            Console.SetCursorPosition(currentCursorLeft, currentCursorTop);
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

                                           x.For<IEmailService>().Use<FakeEmailService>();
                                       });
        }

        private static async void RunPageUpdate(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if(_isPageUpdateTaskRunning)
                return;

            _isPageUpdateTaskRunning = true;
            SetStatus("Updating");


            var updater = _container.GetInstance<FbPageUpdater>();
            await updater.StartUpdate();

            _isPageUpdateTaskRunning = false;
            SetStatus("awaiting next run.");
        }
    }
}
