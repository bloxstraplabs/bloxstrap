# <img src="https://github.com/pizzaboxer/bloxstrap/raw/main/Images/Bloxstrap.png" width="48"/> Bloxstrap
[![License](https://img.shields.io/github/license/pizzaboxer/bloxstrap)](https://github.com/pizzaboxer/bloxstrap/blob/main/LICENSE)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/pizzaboxer/bloxstrap/ci.yml?branch=main&label=builds)](https://github.com/pizzaboxer/bloxstrap/actions)
[![Downloads](https://img.shields.io/github/downloads/pizzaboxer/bloxstrap/latest/total?color=981bfe)](https://github.com/pizzaboxer/bloxstrap/releases)
[![Version](https://img.shields.io/github/v/release/pizzaboxer/bloxstrap?color=7a39fb)](https://github.com/pizzaboxer/bloxstrap/releases/latest)
[![Discord](https://img.shields.io/discord/1099468797410283540?logo=discord&logoColor=white&label=discord&color=4d3dff)](https://discord.gg/nKjV3mGq6R)
[![lol](https://img.shields.io/badge/mom%20made-pizza%20rolls-orange)](https://media.tenor.com/FIkSGbGycmAAAAAd/manly-roblox.gif)

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

You will also need the [.NET 6 Desktop Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win11-x64&apphost_version=6.0.16&gui=true). If you don't already have it installed, you'll be prompted to install it anyway. Be sure to install Bloxstrap after you've installed this.

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
    <img src="https://github.com/pizzaboxer/bloxstrap/assets/41478239/cd723d23-9bff-401e-aadf-deea265a3b1c" width="829" />
    <img src="https://github.com/pizzaboxer/bloxstrap/assets/41478239/dcfd0cdf-1aae-45bb-849a-f7710ec63b28" width="435" />
    <img src="https://github.com/pizzaboxer/bloxstrap/assets/41478239/e08cdf28-4f99-46b5-99f2-5c338aac86db" width="390" />
    <img src="https://github.com/pizzaboxer/bloxstrap/assets/41478239/a45755cb-39da-49df-b0ad-456a139e2efc" Width="593" />
    <img src="https://github.com/pizzaboxer/bloxstrap/assets/41478239/7ba35223-9115-401f-bbc1-d15e9c5fd79e" width="232" />
<p>

## Special thanks
* [@MaximumADHD](https://github.com/MaximumADHD) - Initially inspiring the idea for Bloxstrap with [Roblox Studio Mod Manager](https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager).
* [Multako](https://www.roblox.com/users/2485612194/profile) - Designing the Bloxstrap logo.
* [@1011025m](https://github.com/1011025m) - Providing a method for disabling the Roblox desktop app.
* taskmanager ([@Mantaraix](https://github.com/Mantaraix)) - Helping with designing the new menu look and layout.
* [@Extravi](https://github.com/Extravi) - Allowing their presets to be bundled with Bloxstrap, and helping with improving UX.
* [@axstin](https://github.com/axstin) - Making [rbxfpsunlocker](https://github.com/axstin/rbxfpsunlocker), which was used for Bloxstrap's FPS unlocking up until v2.2.0.
