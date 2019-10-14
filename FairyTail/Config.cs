using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;

namespace FairyTail
{
    public static class Config
    {
        public static Filter[] Filters { get; private set; }
        public static string FontFamily { get; private set; }
        public static int FontSize { get; private set; }

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

            static Brush Parse_Brush(string value)
            {
                var brush = new BrushConverter().ConvertFromString(value);
                return brush as Brush;
            }
        }
    }
}
