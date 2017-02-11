# Slacker

C# commandline tool to send formatted messages to your Slack channel. Currently build around Perforce integration but can be used also to send simple messages.

# How to use
Executable parses passed commanline arguments and executes operations. Common arguments such as slack and perforce settings can be passed directly or with json configuration file.

## Peforce changelist message
Tool has argument "p4cl" for sending formatted messages about perforce changelists. Formatted message contains information about the submitted changelist such as changelist owner, description and file changes.

**SlackerCmd.exe config=config.json p4cl=123**

#### Perforce post-submit announcement
With 'p4cl' argument you can integrate perforce post-submit announcement to you slack channels by adding custom p4 trigger hook.

1. Create slack bot
2. Deploy slacker to your perforce machine
3. Configure slacker config.json file with your slack and perforce settings
4. Add p4 trigger to post-submit
 1. **SlackerCmd.exe config=config.json p4cl=%changelist%**
 
## Peforce changelist validation
With 'p4validatecl' argument you can integrate pre-submit validation for changelists. Slacker will validate changelist content and either approves or interrupts submit by sending 0 or -1 return code.

1. Create slack bot (optional)
2. Deploy slacker to your perforce machine
3. Configure slacker config.json file with your slack and perforce settings
4. Add p4 trigger to pre-submit
 1. **SlackerCmd.exe config=config.json p4validatecl=%changelist%**

## Direct message
You can also send messages directly with message argument.

**SlackerCmd.exe config=config.json message="Hello World!"**

Or without configuration file.

**SlackerCmd.exe name=yourslackname channel=general token=yourslacktoken message="Hello World!"**

## List of commanline arguments

**config=filename.json**
*Load configurations from file*

**configtemplate=output.json**
*Generate configuration template file*

**name=slackname**
*Slack name [xxx.slack.com]*

**channel=slackchannel**
*Slack channel*

**token=slacktoken**
*Slack token*

**message="your slack message here"**
*Slack message (message inside quotation marks)*
            
**p4cl=changelist#**
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

**p4password=123456789**
*P4 password used to login.*

## Config template
Commandline argument 'configtemplate' can be used to generate config template.

**SlackerCmd.exe configtemplate=output.json**

Output is formatted json config file.

```
{
  "P4": {
    "Port": "127.0.0.1:1666",
    "Username": "admin",
    "Password": ""
  },
  "Slack": {
    "Name": "yourslackname",
    "Channel": "general",
    "Token": "yourslacktoken",
    "IllegalPaths": []
  },
  "FileActionLimit": 8,
  "IllegalPaths": [],
  "ShowPostSubmitFileChanges": true,
  "P4DescriptionRules": [
    {
      "ContentRequired": true,
      "StartWith": "[",
      "EndstWith": "]",
      "ContentStrings": []
    }
  ]
}
```

## External libraries
* Json.NET
* p4api.net
