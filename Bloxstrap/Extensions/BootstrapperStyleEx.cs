using Bloxstrap.Enums;
using Bloxstrap.UI;

namespace Bloxstrap.Extensions
{
    static class BootstrapperStyleEx
    {
        public static IBootstrapperDialog GetNew(this BootstrapperStyle bootstrapperStyle) => Controls.GetBootstrapperDialog(bootstrapperStyle);
    }
}
