namespace Bloxstrap.Enums;
using DiscordRPC;
/// <summary>
/// Represents the display options for Discord RPC status.
/// </summary>
/// <remarks>
/// This is basically just a copy of the enum from the DiscordRPC library but with attributes added for the UI
/// </remarks>
public enum DiscordRPCStatusDisplay {
	[EnumName(FromTranslation = "Enums.DiscordRPCStatusDisplay.Name")]
	Name = StatusDisplayType.Name,
	[EnumName(FromTranslation = "Enums.DiscordRPCStatusDisplay.Details")]
	Details = StatusDisplayType.Details,
	// State also exists, but I don't think it makes sense to use it here
}