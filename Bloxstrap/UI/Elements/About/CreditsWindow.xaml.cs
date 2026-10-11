using System.Windows;

using Bloxstrap.UI.Elements.Base;

namespace Bloxstrap.UI.Elements.About
{
    public partial class CreditsWindow : WpfUiWindow
    {
        public class CreditEntry
        {
            public string Name { get; init; } = "";

            public string Description { get; init; } = "";

            public string License { get; init; } = "";

            public string Link { get; init; } = "";

            public Visibility LicenseVisibility => String.IsNullOrEmpty(License) ? Visibility.Collapsed : Visibility.Visible;
        }

        public class CreditGroup
        {
            public string Title { get; init; } = "";

            public string Description { get; init; } = "";

            public List<CreditEntry> Entries { get; init; } = new();
        }

        public List<CreditGroup> Groups { get; } = new()
        {
            new CreditGroup
            {
                Title = Strings.Credits_BuiltOn_Title,
                Description = Strings.Credits_BuiltOn_Description,
                Entries =
                {
                    new() { Name = "Bloxstrap", Description = Strings.Credits_Bloxstrap, License = "MIT", Link = "https://github.com/bloxstraplabs/bloxstrap" }
                }
            },
            new CreditGroup
            {
                Title = Strings.Credits_Bootstrappers_Title,
                Description = Strings.Credits_Bootstrappers_Description,
                Entries =
                {
                    new() { Name = "Fishstrap", Description = Strings.Credits_Fishstrap, License = "MIT", Link = "https://github.com/fishstrap/fishstrap" },
                    new() { Name = "Froststrap", Description = Strings.Credits_Froststrap, License = "MPL-2.0", Link = "https://github.com/Froststrap/Froststrap" },
                    new() { Name = "Funkstrap", Description = Strings.Credits_Funkstrap, License = "MIT", Link = "https://github.com/DawndreamerStudios/funkstrap" },
                    new() { Name = "Bubblestrap", Description = Strings.Credits_Bubblestrap, License = "MIT", Link = "https://github.com/ItzBloxxy/Bubblestrap" },
                    new() { Name = "Voidstrap", Description = Strings.Credits_Voidstrap, License = "MIT", Link = "https://github.com/Voidstrap-rbx/Voidstrap" }
                }
            },
            new CreditGroup
            {
                Title = Strings.Credits_Services_Title,
                Description = Strings.Credits_Services_Description,
                Entries =
                {
                    new() { Name = "RoValra", Description = Strings.Credits_RoValra, Link = "https://www.rovalra.com" },
                    new() { Name = "ipinfo.io", Description = Strings.Credits_IpInfo, Link = "https://ipinfo.io" },
                    new() { Name = "Roblox", Description = Strings.Credits_Roblox, Link = "https://www.roblox.com" },
                    new() { Name = "Cloudflare Workers", Description = Strings.Credits_Cloudflare, Link = "https://workers.cloudflare.com" }
                }
            },
            new CreditGroup
            {
                Title = Strings.Credits_Libraries_Title,
                Description = Strings.Credits_Libraries_Description,
                Entries =
                {
                    new() { Name = "WPF UI", Description = Strings.Credits_WpfUi, License = "MIT", Link = "https://github.com/lepoco/wpfui" },
                    new() { Name = "DiscordRichPresence", Description = Strings.Credits_DiscordRpc, License = "MIT", Link = "https://github.com/Lachee/discord-rpc-csharp" },
                    new() { Name = "CommunityToolkit.Mvvm", Description = Strings.Credits_Mvvm, License = "MIT", Link = "https://github.com/CommunityToolkit/dotnet" },
                    new() { Name = "CsWin32", Description = Strings.Credits_CsWin32, License = "MIT", Link = "https://github.com/microsoft/CsWin32" },
                    new() { Name = "SharpZipLib", Description = Strings.Credits_SharpZipLib, License = "MIT", Link = "https://github.com/icsharpcode/SharpZipLib" },
                    new() { Name = "AvalonEdit", Description = Strings.Credits_AvalonEdit, License = "MIT", Link = "https://github.com/icsharpcode/AvalonEdit" },
                    new() { Name = "Markdig", Description = Strings.Credits_Markdig, License = "BSD-2-Clause", Link = "https://github.com/xoofx/markdig" },
                    new() { Name = "XamlAnimatedGif", Description = Strings.Credits_XamlAnimatedGif, License = "Apache-2.0", Link = "https://github.com/XamlAnimatedGif/XamlAnimatedGif" },
                    new() { Name = "ShellLink", Description = Strings.Credits_ShellLink, License = "MIT", Link = "https://github.com/securifybv/ShellLink" },
                    new() { Name = "System.Diagnostics.PerformanceCounter", Description = Strings.Credits_PerformanceCounter, License = "MIT", Link = "https://github.com/dotnet/runtime" },
                    new() { Name = "Roblox Studio Mod Manager", Description = Strings.Credits_ModManager, License = "MIT", Link = "https://github.com/MaximumADHD/Roblox-Studio-Mod-Manager" }
                }
            },
            new CreditGroup
            {
                Title = Strings.Credits_Assets_Title,
                Description = Strings.Credits_Assets_Description,
                Entries =
                {
                    new() { Name = "Fluent UI System Icons", Description = Strings.Credits_FluentIcons, License = "MIT", Link = "https://github.com/microsoft/fluentui-system-icons" },
                    new() { Name = "Rubik", Description = Strings.Credits_Rubik, License = "OFL-1.1", Link = "https://fonts.google.com/specimen/Rubik" }
                }
            }
        };

        public CreditsWindow()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
