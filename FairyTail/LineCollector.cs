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
        ObservableCollection<string> lines = new ObservableCollection<string>();
        int lines_to_keep;
        long previous_file_size;

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


        public ReadOnlyObservableCollection<string> Get_Collection()
            => new ReadOnlyObservableCollection<string>(lines);

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
                if (lines.Count == 0 || lines.Last() != "") lines.Add("");
                Append_Text($"{SNIP}\r\n");
                Append_Text(seek_and_read(total - Growth_Threshold));
            }

            previous_file_size = total;
        }
    }
}
