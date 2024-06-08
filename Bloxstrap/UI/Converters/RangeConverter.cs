using System.Windows.Data;

namespace Bloxstrap.UI.Converters
{
    internal class RangeConverter : IValueConverter
    {
        public int? From { get; set; }

        public int? To { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int numvalue = (int)value;

            if (From is null)
                return numvalue < To;

            if (To is null)
                return numvalue > From;

            return numvalue > From && numvalue < To;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
