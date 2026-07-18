using System;
using Windows.UI.Xaml.Data;

namespace DTFApp.Helpers
{
    public class BoolToGlyphConverter : IValueConverter
    {
        private const string ExpandedGlyph = "\xE70D";
        private const string CollapsedGlyph = "\xE76C";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool isExpanded) && isExpanded ? ExpandedGlyph : CollapsedGlyph;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
