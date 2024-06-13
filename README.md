# TwitchTTSUser

This project allows for a Twitch user to essentially co-host the stream live. Users can enter with a specialized command, and then any message the user types will be sent to Microsoft TTS to be read out on Stream, until the user is either cleared or a new one is drawn.

## Features

* Simple, straight forward usage
* Easy Twitch Integration
* Relatively small overhead
* GUI with controls
* Configs
* Support for crossplatform (if TTS interface is supported for platform)

## Install

You will need a chat oauth token for your bot account, which [you can get here](https://twitchapps.com/tmi/).

Put in the configs and go.

## Libraries used

* [TwitchLib](https://github.com/TwitchLib/TwitchLib)
* [Avalonia UI](https://avaloniaui.net/)
* [SpeechSynthesizer](https://www.nuget.org/packages/System.Speech)
* [Font Icons and Logos](https://fontawesome.com/) from FontAwesome
