using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Diagnostics;

using Perforce.P4;
using Newtonsoft.Json;

namespace SlackerCmd
{
    public class Program
    {
        static int Main(string[] Args)
        {
            if (Args.Length == 0)
            {
                PrintHelp();
                return 0;
            }

            var ConfigTemplate = ArgumentHelper.GetValue(Args, "configtemplate");
            if (!String.IsNullOrEmpty(ConfigTemplate))
            {
                Configuration.SaveDefaultConfigFile(ConfigTemplate);
                return 0;
            }

            Console.WriteLine(String.Format("[Slacker] Running slacker command line."));

            var Config = new Configuration();

            // Load configurations from file if defined in by arguments
            var FilePath = ArgumentHelper.GetValue(Args, "config");

            if (!String.IsNullOrEmpty(FilePath))
            {
                // Try to load configurations from file
                var LoadedConfig = Configuration.LoadConfigFile(FilePath);
                if (LoadedConfig != null)
                {
                    Config = LoadedConfig;
                    Console.WriteLine(String.Format("[Slacker] Loaded configuration file {0} successfully.", FilePath));
                }
            }

            Config.LoadConfigArgumentOverrides(Args);

            int RetCode = 0;

            // Perforce operations
            if (P4Instance.Instance.UsePerforce(Args))
            {
                P4Instance.Instance.Initialize(Config.P4);

                if (!P4Instance.Instance.Connect(Config.P4))
                {
                    RetCode = -1;
                    goto End;
                }

                // P4 change list message
                var p4cl = ArgumentHelper.GetValueInt(Args, "p4cl");
                if (p4cl > 0)
                {
                    var Changelist = P4Instance.Instance.GetChangeList(p4cl, Config);
                    if (!P4Instance.Instance.HandlePostSubmit(Changelist, Config))
                    {
                        Console.WriteLine(String.Format("[Slacker] Post submit message failed!"));
                        RetCode = -1;
                        goto End;
                    }
                }

                // P4 validate change list
                var p4validatecl = ArgumentHelper.GetValueInt(Args, "p4validatecl");
                if (p4validatecl > 0)
                {
                    var Changelist = P4Instance.Instance.GetChangeList(p4cl, Config);
                    if (!P4Instance.Instance.HandlePreSubmit(Changelist, Config))
                    {
                        Console.WriteLine(String.Format("[Slacker] Pre-submit validation failed on changelist #{0}!", p4validatecl));
                        RetCode = -1;
                        goto End;
                    }
                }

                P4Instance.Instance.Shutdown();
            }
            else
            {
                var Message = ArgumentHelper.GetValue(Args, "message");
                if (!String.IsNullOrEmpty(Message))
                {
                    Task.Run(async () =>
                    {
                        var Payload = new SlackRichPayload()
                        {
                            Username = "Slack bot",
                            Text = Message
                        };

                        if (!String.IsNullOrEmpty(Config.Slack.Channel))
                        {
                            Payload.Channel = Config.Slack.Channel;
                        }

                        var response = await MessageHelper.SendSlackMessage(Payload, Config);

                    }).Wait();
                }
            }

            End:

            Console.WriteLine(String.Format("[Slacker] Exit code {0}.", RetCode));

            return RetCode;
        }

        static void PrintHelp()
        {
            Console.WriteLine(String.Format(" With Slacker you can interact with your Slack channel.          "));
            Console.WriteLine(String.Format("                                                                 "));
            Console.WriteLine(String.Format(" Supported arguments :                                           "));
            Console.WriteLine(String.Format("                                                                 "));
            Console.WriteLine(String.Format(" config=filename.json            Load configurations from file.  "));
            Console.WriteLine(String.Format("                                                                 "));
            Console.WriteLine(String.Format(" name=slackname                  Slack name [xxx.slack.com]      "));
            Console.WriteLine(String.Format(" channel=slackchannel            Slack channel                   "));
            Console.WriteLine(String.Format(" bottoken=slacktoken             Slack bot token                 "));
            Console.WriteLine(String.Format(" message=\"message here\"        Slack message (message inside quotation marks)"));
            Console.WriteLine(String.Format(" debugmessage=true/false         Slack message is displayed in console but not forwarded to slack."));
            Console.WriteLine(String.Format(" incomingwebhookurl=url          Slack incoming webhook url (used with rich messages)."));
            Console.WriteLine(String.Format(" userichformat=true/false        Slack message formatting.       "));
            Console.WriteLine(String.Format("                                                                 "));
            Console.WriteLine(String.Format(" p4cl=1234                       Sends formatted slack message about changelist. Useful with P4 post-submit trigger."));
            Console.WriteLine(String.Format(" p4filelimit=8                   Limit how many files will be displayed in messages for each P4 file action."));
            Console.WriteLine(String.Format(" p4showfiles=true                Flag if changelist files should be included slack messages."));
            Console.WriteLine(String.Format(" p4validatecl=1234               Validates changelist based on configuration rules. Optionally send error message to channels. Useful with P4 pre-submit trigger."));
            Console.WriteLine(String.Format("                                                                 "));
            Console.WriteLine(String.Format(" p4username=admin                P4 username used to login.      "));
            Console.WriteLine(String.Format(" p4port=127.0.0.1:1667           P4 server IP address and port.  "));
            Console.WriteLine(String.Format(" p4password=0123456789           P4 password used to login.      "));
            Console.WriteLine(String.Format(" p4ticket=1234567890ABCDEF       P4 ticket.                      "));
            Console.WriteLine(String.Format("                                                                 "));
            Console.WriteLine(String.Format(" Examples:                                                       "));
            Console.WriteLine(String.Format(" SlackerCmd.exe config=config.json p4cl=1234                     "));
            Console.WriteLine(String.Format(" SlackerCmd.exe config=config.json message=\"Hello World!\"      "));
        }
    }
}
