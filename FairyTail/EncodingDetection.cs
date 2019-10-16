using System.Text;

namespace FairyTail
{
    public static class EncodingDetection
    {
        public static bool TryDetect(byte[] bytes, out Encoding encoding)
        {
            var utf8 = Encoding.UTF8;
            var ansi = Encoding.GetEncoding(1252);

            var utf8_error = utf8.GetString(bytes).Contains("�");
            var ansi_error = ansi.GetString(bytes).Contains("Ã");

            if (utf8_error && !ansi_error)
            {
                encoding = ansi;
                return true;
            }

            if (ansi_error && !utf8_error)
            {
                encoding = utf8;
                return true;
            }

            encoding = null;
            return false;
        }
    }
}
