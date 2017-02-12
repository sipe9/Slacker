using Newtonsoft.Json;
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
        public static SlackRichPayload BuildPostSubmitPayload(Perforce.P4.Changelist Changelist, Configuration Config)
        {
            if (Changelist == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to build post-submit payload becuase changelist {0} is null."));
                return null;
            }

            var FileString = new List<string>();
            if (Config.ShowPostSubmitFileChanges)
            {
                FileString = MessageHelper.BuildFileActionString(Changelist, Config.FileActionLimit);
            }

            var Message = String.Format("{0} has submitted changelist #{1}.", Changelist.OwnerName, Changelist.Id);

            var FileGroupSB = new StringBuilder();
            foreach (var Group in FileString)
            {
                if (!String.IsNullOrEmpty(Group))
                {
                    FileGroupSB.AppendLine(Group);
                }
            }

            var Payload = new SlackRichPayload()
            {
                Username = "Perforce",
                IconUrl = "http://comparegithosting.com/_media/Git-Icon.png",
                Text = Message,
                Attachments = new List<SlackRichPayloadAttachment>
                {
                    new SlackRichPayloadAttachment()
                    {
                        Title = Changelist.Description,
                        Text = FileGroupSB.ToString(),
                    }                    
                }
            };

            if (!String.IsNullOrEmpty(Config.Slack.Channel))
            {
                Payload.Channel = Config.Slack.Channel;
            }

            return Payload;
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
                        MessageLine += String.Format("[{0} : ", Action.Key);

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
                        MessageLine += String.Format("[{0} {1} files]", Action.Key, Action.Value.Count);
                    }

                    RetStrings.Add(MessageLine);
                }
            }

            return RetStrings;
        }

        // Send slack message
        public static async Task<string> SendSlackMessage(SlackRichPayload Payload, Configuration Config)
        {
            if (Payload == null)
            {
                Console.WriteLine(String.Format("[Slacker] Failed to send slack message because message is empty or invalid."));
                return string.Empty;
            }

            if (String.IsNullOrEmpty(Config.Slack.SlackBotUrl) || String.IsNullOrEmpty(Config.Slack.Channel))
            {
                Console.WriteLine(String.Format("[Slacker] Failed to send slack message because slack url {0} or channel {1} is empty or null.", Config.Slack.SlackBotUrl, Config.Slack.Channel));
                return String.Empty;
            }

            if (Config.DebugMessage)
            {
                var SerializePayload = JsonConvert.SerializeObject(Payload);
                Console.WriteLine(String.Format("[Slacker] Debug message: {0}", SerializePayload));
                return String.Empty;
            }
            else
            {
                using (var Client = new HttpClient())
                {
                    if (Config.Slack.UseRichFormatting)
                    {
                        if (String.IsNullOrEmpty(Config.Slack.WebHookUrl))
                        {
                            Console.WriteLine(String.Format("[Slacker] Failed to send rich formatted message because webhook URL is empty or null!."));
                            return String.Empty;
                        }

                        var SerializePayload = JsonConvert.SerializeObject(Payload);

                        Console.WriteLine(String.Format("[Slacker] Sending slack payload to {0}.", Config.Slack.WebHookUrl));

                        var Response = await Client.PostAsync(Config.Slack.WebHookUrl, new StringContent(SerializePayload, new UTF8Encoding(), "application/json"));
                        var ResponseString = await Response.Content.ReadAsStringAsync();
                        Console.WriteLine(String.Format("[Slacker] HTTP response: {0}", ResponseString));
                        return ResponseString;
                    }
                    else
                    {
                        Console.WriteLine(String.Format("[Slacker] Sending slack bot message to {0}.", Config.Slack.SlackBotUrl));

                        var Response = await Client.PostAsync(Config.Slack.SlackBotUrl, new StringContent(Payload.Text));
                        var ResponseString = await Response.Content.ReadAsStringAsync();
                        Console.WriteLine(String.Format("[Slacker] HTTP response: {0}", ResponseString));
                        return ResponseString;
                    }
                }
            }
        }
    }

    public class SlackRichPayloadAttachmentField
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("short")]
        public bool Short { get; set; }

        public SlackRichPayloadAttachmentField()
        {
        }
    }

    public class SlackRichPayloadAttachment
    {
        [JsonProperty("fallback")]
        public string Fallback { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_ink")]
        public string TitleLink { get; set; }

        [JsonProperty("pretext")]
        public string PreText { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_icon")]
        public string AuthorIcon { get; set; }

        [JsonProperty("author_link")]
        public string AuthorLink { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("footer")]
        public string Footer { get; set; }

        [JsonProperty("footer_icon")]
        public string FooterIcon { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; set; }

        [JsonProperty("ts")]
        public string Ts { get; set; }

        [JsonProperty("fields")]
        public List<SlackRichPayloadAttachmentField> Fields { get; set; }

        public SlackRichPayloadAttachment()
        {
        }
    }

    public class SlackRichPayload
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("attachments")]
        public List<SlackRichPayloadAttachment> Attachments { get; set; }

        public SlackRichPayload()
        {
            this.Attachments = new List<SlackRichPayloadAttachment>();
        }
    }
}
