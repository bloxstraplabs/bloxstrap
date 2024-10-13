> [!IMPORTANT]
> [6th October 2024 ‐ Upcoming changes to Bloxstrap](https://github.com/pizzaboxer/bloxstrap/wiki/6th-October-2024-%E2%80%90-Upcoming-changes-to-Bloxstrap)

> [!CAUTION]
> The only official places to download Bloxstrap are this GitHub repository and [bloxstraplabs.com](https://bloxstraplabs.com). Any other websites offering downloads or claiming to be us are not controlled by us.

<p align="center">
    <img src="https://github.com/pizzaboxer/bloxstrap/raw/main/Images/Bloxstrap-full-dark.png#gh-dark-mode-only" width="420">
    <img src="https://github.com/pizzaboxer/bloxstrap/raw/main/Images/Bloxstrap-full-light.png#gh-light-mode-only" width="420">
</p>

<div align="center">

[![License][shield-repo-license]][repo-license]
[![GitHub Workflow Status][shield-repo-workflow]][repo-actions]
[![Crowdin][shield-crowdin-status]][crowdin-project]
[![Downloads][shield-repo-releases]][repo-releases]
[![Version][shield-repo-latest]][repo-latest]
[![Discord][shield-discord-server]][discord-invite]
[![lol][shield-tenor-meme]][tenor-gif]

</div>

----

Bloxstrap is a third-party replacement for the standard Roblox bootstrapper, providing additional useful features and improvements.

Running into a problem or need help with something? [Check out the Wiki](https://github.com/pizzaboxer/bloxstrap/wiki). If you can't find anything, or would like to suggest something, please [submit an issue](https://github.com/pizzaboxer/bloxstrap/issues).

Bloxstrap is only supported for PCs running Windows.

## Frequently Asked Questions

**Q: Is this malware?**

**A:** No. The source code here is viewable to all, and it'd be impossible for us to slip anything malicious into the downloads without anyone noticing. Just be sure you're downloading it from an official source. The only two official sources are this GitHub repository and [bloxstraplabs.com](https://bloxstraplabs.com).

**Q: Can using this get me banned?**

**A:** No, it shouldn't. Bloxstrap doesn't interact with the Roblox client in the same way that exploits do. [Read more about that here.](https://github.com/pizzaboxer/bloxstrap/wiki/Why-it's-not-reasonably-possible-for-you-to-be-banned-by-Bloxstrap)

**Q: Why was multi-instance launching removed?**

**A:** It was removed starting with v2.6.0 for the [reasons stated here](https://github.com/pizzaboxer/bloxstrap/wiki/Plans-to-remove-multi%E2%80%90instance-launching-from-Bloxstrap). It may be added back in the future when there are less issues with doing so.

## Features

- Hassle-free Discord Rich Presence to let your friends know what you're playing at a glance
- Simple support for modding of content files for customizability (death sound, mouse cursor, etc)
- See where your server is geographically located (courtesy of [ipinfo.io](https://ipinfo.io))
- Ability to configure graphics fidelity and UI experience

## Installing
Download the [latest release of Bloxstrap](https://github.com/pizzaboxer/bloxstrap/releases/latest), and run it. Configure your preferences if needed, and install. That's about it!

Alternatively, you can install Bloxstrap via [Winget](https://winstall.app/apps/pizzaboxer.Bloxstrap) by running this in a Command Prompt window:
```
> winget install bloxstrap
```

You will also need the [.NET 6 Desktop Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win11-x64&apphost_version=6.0.16&gui=true). If you don't already have it installed, you'll be prompted to install it anyway. Be sure to install Bloxstrap after you've installed this.

It's not unlikely that Windows Smartscreen will show a popup when you run Bloxstrap for the first time. This happens because it's an unknown program, not because it's actually detected as being malicious. To dismiss it, just click on "More info" and then "Run anyway".

Once installed, Bloxstrap is added to your Start Menu, where you can access the menu and reconfigure your preferences if needed.

## Code

Bloxstrap uses the [WPF UI](https://github.com/lepoco/wpfui) library for the user interface design. We currently use and maintain our own fork of WPF UI at [bloxstraplabs/wpfui](https://github.com/bloxstraplabs/wpfui).


[shield-repo-license]:  https://img.shields.io/github/license/pizzaboxer/bloxstrap
[shield-repo-workflow]: https://img.shields.io/github/actions/workflow/status/pizzaboxer/bloxstrap/ci-release.yml?branch=main&label=builds
[shield-repo-releases]: https://img.shields.io/github/downloads/pizzaboxer/bloxstrap/latest/total?color=981bfe
[shield-repo-latest]:   https://img.shields.io/github/v/release/pizzaboxer/bloxstrap?color=7a39fb

[shield-crowdin-status]: https://badges.crowdin.net/bloxstrap/localized.svg
[shield-discord-server]: https://img.shields.io/discord/1099468797410283540?logo=discord&logoColor=white&label=discord&color=4d3dff
[shield-tenor-meme]:     https://img.shields.io/badge/mom_made-pizza_rolls-orange

[repo-license]:  https://github.com/pizzaboxer/bloxstrap/blob/main/LICENSE
[repo-actions]:  https://github.com/pizzaboxer/bloxstrap/actions
[repo-releases]: https://github.com/pizzaboxer/bloxstrap/releases
[repo-latest]:   https://github.com/pizzaboxer/bloxstrap/releases/latest

[crowdin-project]: https://crowdin.com/project/bloxstrap
[discord-invite]:  https://discord.gg/nKjV3mGq6R
[tenor-gif]:       https://media.tenor.com/FIkSGbGycmAAAAAd/manly-roblox.gif
