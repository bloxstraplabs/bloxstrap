using Bloxstrap.Enums;

namespace Bloxstrap.Helpers
{
    internal class IconManager
    {
        public static Icon GetIconResource()
        {
            return GetIconResource(Program.Settings.BootstrapperIcon);
        }

        public static Icon GetIconResource(BootstrapperIcon icon)
        {
            switch (icon)
            {
                case BootstrapperIcon.Icon2009: return Properties.Resources.Icon2009_ico;
                case BootstrapperIcon.Icon2011: return Properties.Resources.Icon2011_ico;
                case BootstrapperIcon.IconEarly2015: return Properties.Resources.IconEarly2015_ico;
                case BootstrapperIcon.IconLate2015: return Properties.Resources.IconLate2015_ico;
                case BootstrapperIcon.Icon2017: return Properties.Resources.Icon2017_ico;
                case BootstrapperIcon.Icon2019: return Properties.Resources.Icon2019_ico;
                case BootstrapperIcon.Icon2022: return Properties.Resources.Icon2022_ico;
                case BootstrapperIcon.IconBloxstrap: default: return Properties.Resources.IconBloxstrap_ico;
            }
        }

        public static Bitmap GetBitmapResource()
        {
            return GetBitmapResource(Program.Settings.BootstrapperIcon);
        }

        public static Bitmap GetBitmapResource(BootstrapperIcon icon)
        {
            switch (icon)
            {
                case BootstrapperIcon.Icon2009: return Properties.Resources.Icon2009_png;
                case BootstrapperIcon.Icon2011: return Properties.Resources.Icon2011_png;
                case BootstrapperIcon.IconEarly2015: return Properties.Resources.IconEarly2015_png;
                case BootstrapperIcon.IconLate2015: return Properties.Resources.IconLate2015_png;
                case BootstrapperIcon.Icon2017: return Properties.Resources.Icon2017_png;
                case BootstrapperIcon.Icon2019: return Properties.Resources.Icon2019_png;
                case BootstrapperIcon.Icon2022: return Properties.Resources.Icon2022_png;
                case BootstrapperIcon.IconBloxstrap: default: return Properties.Resources.IconBloxstrap_png;
            }
        }
    }
}
