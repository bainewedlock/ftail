using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FairyTail
{
    public class LineCollector
    {
        public static string SNIP = "----- snip -----";
        public const int Growth_Threshold = 10 * 1024; // 10 KB
        readonly string NewLine = "\r\n";
        ObservableCollection<Line> lines = new ObservableCollection<Line>();
        int lines_to_keep;
        long previous_file_size;
        Config.Filter[] filters;

        public LineCollector(int lines_to_keep = 5)
        {
            Lines_To_Keep = lines_to_keep;
            previous_file_size = 0;
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


        public ReadOnlyObservableCollection<Line> Get_Collection()
            => new ReadOnlyObservableCollection<Line>(lines);

        void Append_Text(string text)
        {
            var new_lines = text.Split(new[] { NewLine }, StringSplitOptions.None);

            if (lines.Count > 0 && !new_lines[0].StartsWith(NewLine))
            {
                var last_line = lines.Last();
                lines.RemoveAt(lines.Count - 1);
                Append_Lines(new[] { last_line.Text + new_lines[0] });
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

                var line = Reapply_Filters(new Line { Text = item });

                lines.Add(line);
            }
        }

        Line Reapply_Filters(Line line)
        {
            if (Find_Filter(line.Text, out var filter))
            {
                return new Line
                {
                    Text = line.Text,
                    Foreground = filter.Foreground,
                    Background = filter.Background,
                };
            }
            else
            {
                return new Line
                {
                    Text = line.Text
                };
            }
        }

        bool Find_Filter(string line, out Config.Filter filter)
        {
            foreach (var f in filters)
            {
                if (f.Pattern.IsMatch(line))
                {
                    filter = f;
                    return true;
                }
            }

            filter = null;
            return false;
        }

        public void File_Size_Changed(long total, Func<long, string> seek_and_read)
        {
            var growth = total - previous_file_size;

            if (growth < 0)
            {
                throw new InvalidOperationException("File shrunk?");
            }

            if (growth == 0)
            {
                return;
            }

            if (growth < Growth_Threshold)
            {
                Append_Text(seek_and_read(previous_file_size));
            }
            else
            {
                if (lines.Count == 0 || lines.Last().Text != "") lines.Add(new Line { Text = "" });
                Append_Text($"{SNIP}\r\n");
                Append_Text(seek_and_read(total - Growth_Threshold));
            }

            previous_file_size = total;
        }

        public void Set_Filters(Config.Filter[] filters)
        {
            this.filters = filters ?? new Config.Filter[0];

            var old_lines = lines.ToArray();
            lines.Clear();
            
            foreach (var l in old_lines)
            {
                lines.Add(Reapply_Filters(l));
            }
        }
    }
}
