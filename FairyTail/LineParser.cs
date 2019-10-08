using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace FairyTail
{
    public class LineParser
    {
        readonly string NewLine = "\r\n";
        ObservableCollection<string> lines = new ObservableCollection<string>();
        int lines_to_keep;
        WaitHandle drehkreuz = new AutoResetEvent(false);

        public LineParser(int lines_to_keep = 5)
        {
            Lines_To_Keep = lines_to_keep;
        }

        public int Lines_To_Keep
        {
            get { return lines_to_keep; }
            set
            {
                if (value < 1) throw new ArgumentException();
                lines_to_keep = value;
            }
        }

        public ReadOnlyObservableCollection<string> Get_Collection() => new ReadOnlyObservableCollection<string>(lines);

        public void Append_Text(string text)
        {
            var new_lines = text.Split(new[] { NewLine }, StringSplitOptions.None);

            if (lines.Count > 0 && !new_lines[0].StartsWith(NewLine))
            {
                var last_line = lines.Last();
                lines.RemoveAt(lines.Count - 1);
                Append_Lines(new[] { last_line + new_lines[0] });
                Append_Lines(new_lines.Skip(1));
            }
            else
            {
                Append_Lines(new_lines);
            }
        }

        void Append_Lines(IEnumerable<string> new_lines)
        {
            foreach (var item in new_lines)
            {
                if (lines.Count == Lines_To_Keep)
                    lines.RemoveAt(0);
                lines.Add(item);
            }
        }

        public void F11()
        {
            if (lines.Any())
                lines.RemoveAt(0);
        }
    }
}
