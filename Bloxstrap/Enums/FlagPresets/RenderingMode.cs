namespace Bloxstrap.Enums.FlagPresets
{
    public enum RenderingMode
    {
        [EnumName(FromTranslation = "Common.Automatic")]
        Default,
        D3D10,
        D3D11,
        OpenGL,
        Vulkan,
        Metal
    }
}
