using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Windows.Media;

namespace FTail
{
    public static class Config
    {
        public static Filter[] Filters { get; private set; }
        public static string FontFamily { get; private set; }
        public static int FontSize { get; private set; }
        public static Brush InteractiveHighlightForeground { get; private set; }
        public static Brush InteractiveHighlightBackground { get; private set; }

        public static void Use_Defaults()
        {
            Filters = new Filter[0];
            FontFamily = "Consolas";
            FontSize = 12;
        }

        public static void Load(string file)
        {
            var xml = XDocument.Load(file).Element("FTailConfig");

            Filters = xml.Elements("Filter").Select(Filter.Parse).ToArray();
            FontFamily = xml.Element("Font").Element("Family").Value;
            FontSize = int.Parse(xml.Element("Font").Element("Size").Value);

            InteractiveHighlightForeground = Filter.Parse_Brush(
                xml.Element("InteractiveHighlightForeground").Value);
            InteractiveHighlightBackground = Filter.Parse_Brush(
                xml.Element("InteractiveHighlightBackground").Value);
        }

        public class Filter
        {
            public Regex Pattern { get; set; }
            public Brush Foreground { get; set; }
            public Brush Background { get; set; }

            public static Filter Parse(XElement e)
            {
                return new Filter
                {
                    Pattern = new Regex(e.Element("Pattern").Value, RegexOptions.IgnoreCase),
                    Foreground = Parse_Brush(e.Element("Foreground").Value),
                    Background = Parse_Brush(e.Element("Background").Value),
                };
            }

            public static Brush Parse_Brush(string value)
            {
                var brush = new BrushConverter().ConvertFromString(value);
                return brush as Brush;
            }
        }
    }
}
