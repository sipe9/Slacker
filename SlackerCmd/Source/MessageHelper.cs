using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SlackerCmd
{
    class MessageHelper
    {
        public static string BuildPostSubmitString(Perforce.P4.Repository Repository, int ChangelistNumber, Configuration Config)
        {
            if (Repository == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to build post submit string because repository is null. Please check your P4 configurations and try again."));
                return String.Empty;
            }

            Perforce.P4.Changelist Changelist = null;

            try
            {
                Changelist = Repository.GetChangelist(ChangelistNumber);

                if (Changelist == null)
                {
                    Console.WriteLine(String.Format("[Slacker] Failed to find changelist {0} from repository. Please check changelist number and try again.", ChangelistNumber));
                    return String.Empty;
                }
            }
            catch (Perforce.P4.P4Exception ex)
            {
                Console.WriteLine(ex.Message);
                return String.Empty;
            }

            var FileString = new List<string>();
            if (Config.ShowPostSubmitFileChanges)
            {
                FileString = MessageHelper.BuildFileActionString(Changelist, Config.FileActionLimit);
            }

            return MessageHelper.CreatePostSubmitMessage(Changelist.OwnerName, Changelist.Id, Changelist.Description, FileString);
        }

        // Build string list of all of the file actions and files in specified change list
        public static List<string> BuildFileActionString(Perforce.P4.Changelist Changelist, int FileLimit)
        {
            if (Changelist == null)
            {
                return new List<string>();
            }

            // Create lists for each P4 file action type
            var FileActions = new Dictionary<Perforce.P4.FileAction, List<string>>();
            foreach (Perforce.P4.FileAction Action in Enum.GetValues(typeof(Perforce.P4.FileAction)))
            {
                FileActions.Add(Action, new List<string>());
            }

            // Add each file in change list to corresponding action list
            foreach (var File in Changelist.Files)
            {
                FileActions[File.Action].Add(File.GetFileName());
            }

            // Build list of actions and file merged into single line
            var RetStrings = new List<string>();

            // Go through all of the actions types and construct message line for each type that has files
            foreach (var Action in FileActions)
            {
                if (Action.Value.Count > 0)
                {
                    var MessageLine = String.Empty;

                    // If amount of files is less than the limit append each filename into the message
                    if (Action.Value.Count < FileLimit)
                    {
                        MessageLine += String.Format("[*{0}* : ", Action.Key);

                        for (int i = 0; i < Action.Value.Count; i++)
                        {
                            MessageLine += String.Format("{0}", Action.Value[i]);
                            if (i < Action.Value.Count - 1)
                            {
                                MessageLine += ", ";
                            }
                        }
                        MessageLine += "]";
                    }
                    // Otherwise just append number of files and the action type
                    else
                    {
                        MessageLine += String.Format("[*{0}* {1} files]", Action.Key, Action.Value.Count);
                    }

                    RetStrings.Add(MessageLine);
                }
            }

            return RetStrings;
        }

        // Build post-submit message
        public static string CreatePostSubmitMessage(string Username, int Changelist, string Description, List<string> FileGroups)
        {
            var SB = new StringBuilder();

            SB.AppendLine(String.Format("*[Perforce]* {0} has submitted changelist #{1}.", Username, Changelist));

            if (!String.IsNullOrEmpty(Description))
            {
                SB.AppendLine(String.Format("- {0}", Description));
            }

            foreach (var Group in FileGroups)
            {
                if (!String.IsNullOrEmpty(Group))
                {
                    SB.AppendLine(Group);
                }
            }

            return SB.ToString();
        }

        // Send slack message
        public static async Task<string> SendSlackMessage(string Message, SlackConfiguration SlackConfig)
        {
            if (String.IsNullOrEmpty(Message))
            {
                Console.WriteLine(String.Format("[Slacker] Failed to send slack message because message is empty or invalid."));
                return string.Empty;
            }

            if (String.IsNullOrEmpty(SlackConfig.Url) || String.IsNullOrEmpty(SlackConfig.Channel))
            {
                Console.WriteLine(String.Format("[Slacker] Failed to send slack message because slack url {0} or channel {1} is empty or null.", SlackConfig.Url, SlackConfig.Channel));
                return String.Empty;
            }

            Console.WriteLine(String.Format("[Slacker] Sending slack message to {0}.", SlackConfig.Url));
            Console.WriteLine(String.Format("[Slacker] {0}.", Message));

            using (var Client = new HttpClient())
            {
                var Response = await Client.PostAsync(SlackConfig.Url, new StringContent(Message));
                var ResponseString = await Response.Content.ReadAsStringAsync();
                Console.WriteLine(String.Format("[Slacker] HTTP response: {0}", ResponseString));
                return ResponseString;
            }
        }
    }
}
