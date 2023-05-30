# <img src="https://github.com/pizzaboxer/bloxstrap/raw/main/Images/Bloxstrap.png" width="48"/> Bloxstrap
[![License](https://img.shields.io/github/license/pizzaboxer/bloxstrap)](https://github.com/pizzaboxer/bloxstrap/blob/main/LICENSE)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/pizzaboxer/bloxstrap/ci.yml?branch=main&label=builds)](https://github.com/pizzaboxer/bloxstrap/actions)
[![Downloads](https://img.shields.io/github/downloads/pizzaboxer/bloxstrap/latest/total)](https://github.com/pizzaboxer/bloxstrap/releases)
[![Version](https://img.shields.io/github/v/release/pizzaboxer/bloxstrap?color=4d3dff)](https://github.com/pizzaboxer/bloxstrap/releases/latest)
[![lol](https://img.shields.io/badge/mom%20made-pizza%20rolls-orange)](https://media.tenor.com/FIkSGbGycmAAAAAd/manly-roblox.gif)
[![Stars](https://img.shields.io/github/stars/pizzaboxer/bloxstrap?style=social)](https://github.com/pizzaboxer/bloxstrap/stargazers)

An open-source, feature-packed alternative bootstrapper for Roblox.

This a drop-in replacement for the stock Roblox bootstrapper, working more or less how you'd expect it to, while providing additional useful features. This does not touch or modify the game client itself, it's just a launcher! So don't worry, there's practically no risk of being banned for using this.

Running into a problem or need help with something? [Check out the Wiki](https://github.com/pizzaboxer/bloxstrap/wiki). If you can't find anything, or would like to suggest something, please [submit an issue](https://github.com/pizzaboxer/bloxstrap/issues) or report it in our [Discord server](https://discord.gg/nKjV3mGq6R).
 
Bloxstrap is only supported for PCs running Windows.
 
 ## Installing
Download the [latest release of Bloxstrap](https://github.com/pizzaboxer/bloxstrap/releases/latest), and run it. Configure your preferences if needed, and install. That's about it!

Alternatively, you can install Bloxstrap via [Winget](https://winstall.app/apps/pizzaboxer.Bloxstrap) by running this in the Command Prompt:
```
> winget install bloxstrap
```

You will also need the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.16-windows-x64-installer). If you don't already have it installed, you'll be prompted to install it anyway. Be sure to install Bloxstrap after you've installed this.

It's not unlikely that Windows Smartscreen will show a popup when you run Bloxstrap for the first time. This happens because it's an unknown program, not because it's actually detected as being malicious. To dismiss it, just click on "More info" and then "Run anyway".

Once installed, Bloxstrap is added to your Start Menu, where you can access the menu and reconfigure your preferences if needed.

If you would like to build Bloxstrap's source code, see the [guide for building from source](https://github.com/pizzaboxer/bloxstrap/wiki/Building-Bloxstrap-from-source).
 
## Features
Here's some of the features that Bloxstrap provides over the stock Roblox bootstrapper:

* Persistent file modifications - re-adds the old death sound!
* Support for FastFlag editing, with FPS unlimiting and more!
* Painless and seamless support for Discord Rich Presence - no auth cookie needed!
* A customizable launcher look
* Lets you opt into non-production Roblox release channels
* Lets you see what region your current server is located in
* Lets you have multiple Roblox game instances open simultaneously

All the available features are browsable through the Bloxstrap menu. There's not too many, but it's recommended to look through all of them.

Bloxstrap also only runs whenever necessary, so it doesn't stay running in the background when you're not playing.

## Screenshots

<p float="left">
    <img src="https://user-images.githubusercontent.com/41478239/236173030-cab73a81-21c4-416e-bcba-a1854f3ecce6.png" width="620" />
    <img src="https://user-images.githubusercontent.com/41478239/219783594-976a3442-2ca2-4940-81db-948528375551.png" width="205" />
    <img src="https://user-images.githubusercontent.com/41478239/236173107-8da468ff-905e-45d0-af0c-5e433d29b9bc.png" width="419" />
    <img src="https://user-images.githubusercontent.com/41478239/224809793-9a42c9bf-fdfc-435c-819a-0827b8136ae8.png" width="406" />
<p>

## Special thanks
* [Multako](https://www.roblox.com/users/2485612194/profile) - Designing the Bloxstrap logo.
* [@1011025m](https://github.com/1011025m) - Providing a method for disabling the Roblox desktop app.
* taskmanager ([@Mantaraix](https://github.com/Mantaraix)) - Helping with designing the new menu look and layout.
* [@Extravi](https://github.com/Extravi) - Allowing their presets to be bundled with Bloxstrap, and helping with improving UX.
* [@axstin](https://github.com/axstin) - Making [rbxfpsunlocker](https://github.com/axstin/rbxfpsunlocker), which was used for Bloxstrap's FPS unlocking up until v2.2.0.
