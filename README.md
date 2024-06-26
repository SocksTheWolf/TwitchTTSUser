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
* [Json.NET](https://www.newtonsoft.com/json)
* [SpeechSynthesizer](https://www.nuget.org/packages/System.Speech)
* [NAudio](https://github.com/naudio/NAudio)
* Font Icons and Logos from [FontAwesome](https://fontawesome.com/)
* Chime Sound Collection © 2024 by [Deckard Holiday](https://github.com/DeckardHoliday) is licensed under [Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/?ref=chooser-v1)
