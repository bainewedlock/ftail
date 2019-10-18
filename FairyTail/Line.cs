using System.Windows.Media;

namespace FTail
{
    public class Line
    {
        public string Text { get; set; }
        public Brush Foreground { get; set; } = Brushes.Black;
        public Brush Background { get; set; } = Brushes.White;
    }
}
