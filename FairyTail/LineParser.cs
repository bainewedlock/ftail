using System;
using System.Collections.Generic;
using System.Linq;

namespace FairyTail
{
    public class LineParser
    {
        List<string> buffered_lines = new List<string>();
        readonly string NewLine = "\r\n";

        public void Append_Text(string text)
        {
            var lines = text.Split(new[] { NewLine }, StringSplitOptions.None);

            if (buffered_lines.Count > 0 && !lines[0].StartsWith(NewLine))
            {
                buffered_lines[buffered_lines.Count - 1] += lines[0];
                buffered_lines.AddRange(lines.Skip(1));
            }
            else
            {
                buffered_lines.AddRange(lines);
            }
        }

        public IEnumerable<string> Get_Lines(int max_lines)
        {
            var skip = Math.Max(0, buffered_lines.Count - max_lines);
            return buffered_lines.Skip(skip);
        }

        public void Remove_Except_Last(int lines_to_keep)
        {
            var remove_count = Math.Max(0, buffered_lines.Count - lines_to_keep);
            buffered_lines.RemoveRange(0, remove_count);
        }
    }
}
