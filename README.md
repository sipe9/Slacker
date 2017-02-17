# Slacker

Slacker is command line tool written in C# that sends formatted messages to Slack.

## TODO
1. Take P4 ticket property into use
2. Add support to Slack incoming webhook actions

# How to use
Slacker parses commanline arguments and executes desired operations. Arguments can be passed individially or with json configuration file. When both file and arguments are used, config file is first loaded and then individual arguments may override any configuration.

## Peforce changelist
Argument "p4cl" is used to send formatted message of the perforce changelist. Message contains information about changelist such as changelist owner, description and all of the file changes.

**SlackerCmd.exe config=config.json p4cl=123**

#### Perforce post-submit announcement
Perforce changelist message can be used with perforce post-submit triggers and send messages automatically after changelist submit.

**How to integrate with P4**

1. Create slack bot
2. Deploy slacker cmd tool to your perforce machine
3. Configure slacker config.json file with your slack (name, channel and token) and perforce (port, username, password) settings
4. Add new p4 trigger to **post-submit** (Note! Only users with p4 admin rights can edit triggers)
   1. SlackerCmd.exe config=config.json p4cl=%changelist%
 
## Peforce changelist validation
With 'p4validatecl' argument you can integrate pre-submit validation for changelists. Slacker will validate changelist content and either approves or interrupts submit by sending 0 or -1 return code.

1. Create slack bot (optional)
2. Deploy slacker cmd tool to your perforce machine
3. Configure slacker config.json file with your slack (name, channel and token) and perforce (port, username, password) settings
4. Add new p4 trigger to **pre-submit**
   1. SlackerCmd.exe config=config.json p4validatecl=%changelist%

## Slack message
Argument 'message' can be used to send messages directly. Remember to use quotation marks around message.

**SlackerCmd.exe config=config.json message="Hello World!"**

Or without configuration file.

**SlackerCmd.exe name=yourslackname channel=general bottoken=yourslacktoken message="Hello World!"**

## Slack message with attachments and fields
Argument 'messagefile' can be used to send messages directly.

**SlackerCmd.exe config=config.json messagefile=MessagePayload.json**

Argument 'message' and 'channel' can be used with message file to override these properties.

## List of commanline arguments

**config=filename.json**
*Load configurations from file*

**configtemplate=output.json**
*Generate configuration template file*

**name=slack name**
*Slack name [xxx.slack.com]*

**channel=slack channel**
*Slack channel*

**bottoken=slack bot token**
*Slack bot token*

**incomingwebhookurl=url**
*Slack incoming webhook url (required if rich formatting used)*

**userichformat=true or false**
*Use rich formatting (slack incoming webhook) or simple message with slack bot.*

**message="your slack message here"**
*Slack message (message inside quotation marks)*

**messagefile=messagePayload.json**
*Slack rich formatted message with attachment and fields*

**debugmessage**
*Only prints message to console output and doesn't send it to Slack.*
            
**p4cl=changelistnumber**
*Sends formatted slack message about changelist. Useful with P4 post-submit trigger.*

**p4filelimit=8**
*Limit how many files will be displayed in messages for each P4 file action.*

**p4showfiles=true**
*Flag if changelist files should be included slack messages.*
            
**p4validatecl=1234**
*Validates changelist based on configuration rules. Useful with P4 pre-submit trigger.*

**p4username=admin**
*P4 username used to login.*

**p4port=127.0.0.1:1667**
*P4 server IP address and port.*

**p4password=0123456789**
*P4 password used to login.*

**p4ticket=0123456789ABCDEF**
*P4 ticket. (Currently not used, use password instead)*

## Config template
Commandline argument 'configtemplate' can be used to generate config template.

**SlackerCmd.exe configtemplate=output.json**

Output is formatted json config file.

```
{
  "P4": {
    "Port": "127.0.0.1:1666",
    "Username": "admin",
    "Password": "",
    "Ticket": "",
    "PostSubmit": {
      "ShowFileChanges": true,
      "FileActionLimit": 8
    },
    "PreSubmitValidation": {
      "IllegalPaths": [],
      "Rules": [
        {
          "ContentRequired": true,
          "StartWith": "[",
          "EndstWith": "]",
          "ContentStrings": []
        }
      ]
    }
  },
  "Slack": {
    "Name": "yourslackname",
    "Channel": "general",
    "BotToken": "yourslacktoken",
    "IncomingWebHookUrl": "",
    "UseRichFormatting": false
  }
}
```

## Slack message payload template
Commandline argument 'payloadtemplate' can be used to generate slack message payload template file.

**SlackerCmd.exe payloadtemplate=MessagePayload.json**

```
Output is formatted json file for slack message payloads.
{
  "text": "Main text",
  "username": "Username",
  "channel": null,
  "icon_url": null,
  "attachments": [
    {
      "fallback": null,
      "title": "Title",
      "title_link": null,
      "pretext": "Pretext",
      "author_name": "Author",
      "author_icon": null,
      "author_link": null,
      "text": "Attachment text",
      "color": "#7CD197",
      "footer": "Footer",
      "footer_icon": null,
      "image_url": "",
      "thumb_url": null,
      "ts": null,
      "fields": [
        {
          "title": "Field text",
          "value": "High",
          "short": true
        },
        {
          "title": "Field text 2",
          "value": "High",
          "short": true
        }		
      ]
    }
  ]
}
```

## External libraries
* Json.NET
* p4api.net
