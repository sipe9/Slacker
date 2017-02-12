﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace SlackerCmd
{
    public class IllegalPaths
    {
        public string Extension { get; set; }

        public string DepotDirectory { get; set; }

        public IllegalPaths(string extension, string depotDirectory)
        {
            this.Extension = extension;
            this.DepotDirectory = depotDirectory;
        }
    }

    public class P4Configuration
    {
        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Ticket { get; set; }

        public P4Configuration()
        {
            this.Port = "127.0.0.1:1666";
            this.Username = "admin";
            this.Password = String.Empty;
            this.Ticket = String.Empty;
        }
    }

    public class SlackConfiguration
    {
        public string Name { get; set; }
        public string Channel { get; set; }
        public string Token { get; set; }

        public List<IllegalPaths> IllegalPaths { get; set; }

        public string WebHookUrl { get; set; }
        public bool UseRichFormatting { get; set; }

        public SlackConfiguration()
        {
            this.Name = "yourslackname";
            this.Channel = "general";
            this.Token = "yourslacktoken";
            this.IllegalPaths = new List<IllegalPaths>();
            this.WebHookUrl = string.Empty;
            this.UseRichFormatting = false;
        }

        public bool IsValid()
        {
            if (this.Name.Equals("yourslackname"))
            {
                return false;
            }
            else if (this.Token.Equals("yourslacktoken"))
            {
                return false;
            }

            return true;
        }

        [JsonIgnore]
        public string SlackBotUrl
        {
            get
            {
                return string.Format(@"https://{0}.slack.com/services/hooks/slackbot?token={1}&channel=%23{2}", Name, Token, Channel);
            }
        }
    }

    public class Configuration
    {
        public P4Configuration P4 { get; set; }

        public SlackConfiguration Slack { get; set; }

        public int FileActionLimit { get; set; }

        public List<IllegalPaths> IllegalPaths { get; }

        public bool ShowPostSubmitFileChanges { get; set; }

        public List<P4DescriptionRule> P4DescriptionRules { get; }

        [JsonIgnore]
        public bool DebugMessage { get; set; }

        public Configuration()
        {
            this.P4 = new P4Configuration();
            this.Slack = new SlackConfiguration();

            this.FileActionLimit = 8;
            this.IllegalPaths = new List<IllegalPaths>();
            this.ShowPostSubmitFileChanges = true;
            this.P4DescriptionRules = new List<P4DescriptionRule>();

            var RequireDecription = new P4DescriptionRule();
            RequireDecription.ContentRequired = true;
            this.P4DescriptionRules.Add(RequireDecription);
        }

        public static Configuration LoadConfigFile(string FilePath)
        {
            if (String.IsNullOrEmpty(FilePath))
            {
                Console.WriteLine("[Slacker] Failed to load configurations from file because path is null or empty.");
                return null;
            }

            var FileContent = System.IO.File.ReadAllText(FilePath);

            if (String.IsNullOrEmpty(FileContent))
            {
                Console.WriteLine(String.Format("[Slacker] Configuration file is not in valid format. Please check config file {0} formatting and try again.", FilePath));

                return null;
            }

            return JsonConvert.DeserializeObject<Configuration>(FileContent);
        }

        public void LoadConfigArgumentOverrides(string[] Args)
        {
            if (Args.Length == 0)
                return;

            var Username = ArgumentHelper.GetValue(Args, "p4username");
            if (!String.IsNullOrEmpty(Username))
            {
                P4.Username = Username;
            }

            var Password = ArgumentHelper.GetValue(Args, "p4password");
            if (!String.IsNullOrEmpty(Password))
            {
                P4.Password = Password;
            }

            var Port = ArgumentHelper.GetValue(Args, "p4port");
            if (!String.IsNullOrEmpty(Port))
            {
                P4.Port = Port;
            }

            var Ticket = ArgumentHelper.GetValue(Args, "p4ticket");
            if (!String.IsNullOrEmpty(Ticket))
            {
                P4.Ticket = Ticket;
            }

            var Channel = ArgumentHelper.GetValue(Args, "channel");
            if (!String.IsNullOrEmpty(Channel))
            {
                Slack.Channel = Channel;
            }

            var Token = ArgumentHelper.GetValue(Args, "token");
            if (!String.IsNullOrEmpty(Token))
            {
                Slack.Token = Token;
            }

            var Name = ArgumentHelper.GetValue(Args, "name");
            if (!String.IsNullOrEmpty(Name))
            {
                Slack.Name = Name;
            }

            var WebHookUrl = ArgumentHelper.GetValue(Args, "webhookurl");
            if (!String.IsNullOrEmpty(WebHookUrl))
            {
                Slack.WebHookUrl = WebHookUrl;
            }

            if (ArgumentHelper.Has(Args, "userichformat"))
            {
                Slack.UseRichFormatting = ArgumentHelper.GetValueBoolean(Args, "userichformat");
            }

            var FileLimit = ArgumentHelper.GetValueInt(Args, "p4filelimit");
            if (FileLimit > 0)
            {
                FileActionLimit = FileLimit;
            }

            if (ArgumentHelper.Has(Args, "p4showfiles"))
            {
                ShowPostSubmitFileChanges = ArgumentHelper.GetValueBoolean(Args, "p4showfiles");
            }

            DebugMessage = ArgumentHelper.Has(Args, "debugmessage");
        }

        public static void SaveDefaultConfigFile(string FilePath)
        {
            if (String.IsNullOrEmpty(FilePath))
            {
                Console.Write(String.Format("[Slacker] Failed to write configuration file because path is empty or null."));
                return;
            }

            Configuration Config = new Configuration();
            string ConfigJsonFormatted = JsonConvert.SerializeObject(Config, Formatting.Indented);
            System.IO.File.WriteAllText(FilePath, ConfigJsonFormatted);

            Console.Write(String.Format("[Slacker] Config template saved to {0}.", FilePath));
        }
    }

    public class P4DescriptionRule
    {
        public bool ContentRequired { get; set; }
        public string StartWith { get; set; }
        public string EndstWith { get; set; }
        public List<string> ContentStrings { get; }

        public P4DescriptionRule()
        {
            this.ContentRequired = true;
            this.StartWith = "[";
            this.EndstWith = "]";
            this.ContentStrings = new List<string>();
        }
    }
}
