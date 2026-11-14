namespace Bloxstrap.Enums.FlagPresets
{
    public enum RenderingMode
    {
        [EnumName(FromTranslation = "Common.Automatic")]
        Default,

        [EnumName(StaticName = "Direct3D 11")]
        D3D11,

        [EnumName(StaticName = "Vulkan")]
        Vulkan
    }
}
