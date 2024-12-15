namespace Bloxstrap.Enums.FlagPresets
{
    public enum RenderingMode
    {
        [EnumName(FromTranslation = "Common.Automatic")]
        Default,
        Vulkan,
        OpenGL,
        D3D11,
        D3D10,
    }
}
