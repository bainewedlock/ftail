using NUnit.Framework;
using System.Text;

namespace FairyTail.Tests
{
    [TestFixture]
    public class EncodingTests
    {
        [Test]
        public void Detect_Ansi_File()
        {
            var ansi_file = new byte[] { 122, 101, 105, 108, 101, 32, 49, 13, 10, 104,
                101, 108, 108, 246, 32, 119, 246, 114, 108, 100, 32, 128, 13, 10, 122,
                101, 105, 108, 101, 32, 51, 13, 10 };

            if (EncodingDetection.TryDetect(ansi_file, out var encoding))
            {
                Assert.That(encoding, Is.EqualTo(Encoding.GetEncoding(1252)));
            }
            else Assert.Fail();
        }

        [Test]
        public void Detect_UTF8_File()
        {
            var utf8_file = new byte[] { 122, 101, 105, 108, 101, 32, 49, 13, 10, 104,
                101, 108, 108, 195, 182, 32, 119, 195, 182, 114, 108, 100, 32, 226, 130,
                172, 13, 10, 122, 101, 105, 108, 101, 32, 51, 13, 10 };
            if (EncodingDetection.TryDetect(utf8_file, out var encoding))
            {
                Assert.That(encoding, Is.EqualTo(Encoding.UTF8));
            }
            else Assert.Fail();
        }

        [Test]
        public void Detect_Ansi_Char()
        {
            var ansi_ö = new byte[] { 246 };
            if (EncodingDetection.TryDetect(ansi_ö, out var encoding))
            {
                Assert.That(encoding, Is.EqualTo(Encoding.GetEncoding(1252)));
            }
            else Assert.Fail();
        }

        [Test]
        public void Detect_UTF8()
        {
            var utf8_ö = new byte[] { 195, 182 };
            if (EncodingDetection.TryDetect(utf8_ö, out var encoding))
            {
                Assert.That(encoding, Is.EqualTo(Encoding.UTF8));
            }
            else Assert.Fail();
        }


        [Test]
        public void Dont_Detect_ASCII()
        {
            var ascii = new byte[] { 65 };

            if (EncodingDetection.TryDetect(ascii, out var encoding))
                Assert.Fail(encoding.ToString());
        }
    }
}
