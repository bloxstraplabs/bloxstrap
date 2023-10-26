using System.Windows.Media;

namespace Bloxstrap.Models
{
    public class BootstrapperIconEntry
    {
        public BootstrapperIcon IconType { get; set; }
        public string Name => IconType.ToString();
        public ImageSource ImageSource => IconType.GetIcon().GetImageSource();
    }
}
