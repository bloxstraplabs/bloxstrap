# <img src="https://github.com/pizzaboxer/bloxstrap/raw/main/Images/Bloxstrap.png" width="48"/> Bloxstrap
[![License](https://img.shields.io/github/license/pizzaboxer/bloxstrap)](https://github.com/pizzaboxer/bloxstrap/blob/main/LICENSE)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/pizzaboxer/bloxstrap/ci.yml?branch=main&label=builds)](https://github.com/pizzaboxer/bloxstrap/actions)
[![Downloads](https://img.shields.io/github/downloads/pizzaboxer/bloxstrap/total)](https://github.com/pizzaboxer/bloxstrap/releases)
[![Version](https://img.shields.io/github/v/release/pizzaboxer/bloxstrap?color=4d3dff)](https://github.com/pizzaboxer/bloxstrap/releases/latest)
[![lol](https://img.shields.io/badge/mom%20made-pizza%20rolls-orange)](https://media.tenor.com/FIkSGbGycmAAAAAd/manly-roblox.gif)
[![Stars](https://img.shields.io/github/stars/pizzaboxer/bloxstrap?style=social)](https://github.com/pizzaboxer/bloxstrap/stargazers)

An open-source, feature-packed alternative bootstrapper for Roblox.

This a drop-in replacement for the stock Roblox bootstrapper, working more or less how you'd expect it to, while providing additional useful features. This does not touch or modify the game client itself, it's just a launcher!

If you encounter a bug, or would like to suggest a feature, please [submit an issue](https://github.com/pizzaboxer/bloxstrap/issues). If you're looking for help on how to use Bloxstrap, please see the [help topics](https://github.com/pizzaboxer/bloxstrap/wiki), or open an issue.
 
Bloxstrap is only supported for PCs running Windows.
 
## Features
Here's some of the features that Bloxstrap provides over the stock Roblox bootstrapper:

* Support for [ReShade](https://reshade.me) with [Extravi's ReShade Presets](https://bloxshade.com/)
* Painless support for Discord Rich Presence - no auth cookie needed!
* Silent automatic FPS unlocking with [rbxfpsunlocker](https://github.com/axstin/rbxfpsunlocker)
* Persistent file modifications - re-adds the old death sound!
* Ability to disable the Desktop app
* A customizable launcher - includes dark theme!
* Support for opting in to pre-release testing channels
* Seeing what region your current server is located in

## Installing
Download the [latest release of Bloxstrap](https://github.com/pizzaboxer/bloxstrap/releases/latest), and run it. Configure your preferences if needed, and install. That's about it!

Alternatively, you can install Bloxstrap via [Winget](https://winstall.app/apps/pizzaboxer.Bloxstrap):
```
> winget install bloxstrap
```

You will also need the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.14-windows-x64-installer). If you don't already have it installed, you'll be prompted to install it anyway.

It's not unlikely that Windows Smartscreen will show a popup when you run Bloxstrap for the first time. This happens because it's an unknown program, not because it's actually detected as being malicious. To dismiss it, just click on "More info" and then "Run anyway".

Once installed, Bloxstrap is added to your Start Menu, where you can access the menu and reconfigure your preferences if needed.

## Screenshots
<p float="left">
    <img src="https://user-images.githubusercontent.com/41478239/219782012-11581578-c80b-419b-b027-733561e3e493.png" width="620" />
    <img src="https://user-images.githubusercontent.com/41478239/219783594-976a3442-2ca2-4940-81db-948528375551.png" width="205" />
    <img src="https://user-images.githubusercontent.com/41478239/224803230-c51551d3-f08d-4abc-80d4-92e4f3114848.png" width="419" />
    <img src="https://user-images.githubusercontent.com/41478239/224809793-9a42c9bf-fdfc-435c-819a-0827b8136ae8.png" width="406" />
<p>

## Special thanks
* [Multako](https://www.roblox.com/users/2485612194/profile) - Designing the Bloxstrap logo.
* [@1011025m](https://github.com/1011025m) - Providing a method for disabling the Roblox desktop app.
* taskmanager ([@Mantaraix](https://github.com/Mantaraix)) - Helping with designing the new menu look and layout.
* [@Extravi](https://github.com/Extravi) - Offering to make his presets a part of Bloxstrap, and helping with improving UX.
