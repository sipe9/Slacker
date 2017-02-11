using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Perforce.P4;

namespace SlackerCmd
{
    public sealed class P4Instance
    {
        #region Singleton
        private static volatile P4Instance _instance;
        private static object _syncRoot = new Object();

        public static P4Instance Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new P4Instance();
                        }
                    }
                }

                return _instance;
            }
        }

        // Hide constructor
        private P4Instance() {}
        #endregion

        public Perforce.P4.Server Server { get; private set; }
        public Perforce.P4.Repository Repository { get; private set; }
        public Perforce.P4.Connection Connection { get; private set; }

        private bool _intiailized = false;
        public bool Initialized
        {
            get { return _intiailized; }
        }

        public bool UsePerforce(string[] Args)
        {
            foreach (var Arg in Args)
            {
                var ArgLower = Arg.ToLower();

                if (ArgLower.StartsWith("p4cl") ||
                    ArgLower.StartsWith("p4validatecl"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Connect()
        {
            return Connect(String.Empty);
        }

        public bool Connect(string Password)
        {
            if (Connection == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to connect Perforce because connection is null!"));
                return false;
            }

            try
            {
                if (!Connection.Connect(null))
                {
                    Console.WriteLine(String.Format("[Slacker] Failed to connect Perforce. username={0} port={1}", Connection.UserName, Server.Address));
                    return false;
                }
            }
            catch (Perforce.P4.P4Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            if (!String.IsNullOrEmpty(Password))
            {
                var Cred = Connection.Login(Password);
                if (Cred == null)
                {
                    Console.WriteLine(String.Format("[Slacker] Failed to login perforce server. username={0} port={1}", Connection.UserName, Server.Address));
                    return false;
                }
            }

            Console.WriteLine(String.Format("[Slacker] Connected to perforce server {0} as {1}.", Server.Address, Connection.UserName));

            return true;
        }

        public void Initialize(P4Configuration Config)
        {
            if (_intiailized)
            {
                Shutdown();
            }

            Server = new Server(new ServerAddress(Config.Port));
            Repository = new Repository(Server);

            Connection = Repository.Connection;
            Connection.UserName = Config.Username;
            Connection.Client = new Client();
            Connection.Client.Name = Config.Username;

            _intiailized = true;
        }

        public void Shutdown()
        {
            if (Connection != null)
            {
                Connection.Disconnect();
                Connection.Dispose();
            }

            if (Repository != null)
            {
                Repository.Dispose();
            }

            _intiailized = false;
        }

        public bool HandlePostSubmit(Perforce.P4.Repository Repository, int Changelist, Configuration Config)
        {
            if (Repository == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to handle post-submit because repository is null. Please check your P4 configurations and try again."));
                return false;
            }

            if (Config == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to handle post-submit because configuration is null."));
                return false;
            }

            if (!Config.Slack.IsValid())
            {
                Console.WriteLine(String.Format("[Slacker] Invalid Slack configurations detected. Please review your Slack configuration and try again."));
                Console.WriteLine(String.Format("[Slacker] Slack configurations: Name '{0}', Channel '#{1}' and Token '{2}'.", Config.Slack.Name, Config.Slack.Channel, Config.Slack.Token));
                return false;
            }

            var SubmitMessage = MessageHelper.BuildPostSubmitString(Repository, Changelist, Config);
            if (String.IsNullOrEmpty(SubmitMessage))
            {
                return false;
            }

            Task.Run(async () =>
            {
                var response = await MessageHelper.SendSlackMessage(SubmitMessage, Config.Slack);

            }).Wait();

            return true;
        }

        public bool HandlePreSubmit(Perforce.P4.Repository Repository, int Changelist, Configuration Config)
        {
            if (Repository == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to handle pre-submit because repository is null. Please check your P4 configurations and try again."));
                return false;
            }

            if (Config == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to handle pre-submit because configuration is null."));
                return false;
            }

            var Error = String.Empty;

            // Check for illegal paths and extensions
            if (Validator.HasIllegalPaths(Repository, Changelist, Config, out Error))
            {
                return false;
            }

            // Validate description based on rules
            if (Config.P4DescriptionRules.Count > 0 && !Validator.IsValidDescription(Repository, Changelist, Config, out Error))
            {
                return false;
            }

            return true;
        }
    }
}
