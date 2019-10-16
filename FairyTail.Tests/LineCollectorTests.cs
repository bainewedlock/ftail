using NUnit.Framework;
using System.Linq;

namespace FairyTail.Tests
{
    [TestFixture]
    public class LineCollectorTests
    {
        [TestCase(0, 1)]
        [TestCase(1, 2)]
        public void File_Grew_A_Bit(int initial_file_size, int grow_size)
        {
            var sut = new LineCollector();
            long actual_seek = -1;
            sut.File_Size_Changed(initial_file_size, seek => "");

            // ACT
            sut.File_Size_Changed(initial_file_size + grow_size, seek =>
            {
                actual_seek = seek;
                return "!";
            });

            Assert.That(actual_seek, Is.EqualTo(initial_file_size));
            Assert.That(Lines_Of(sut), Is.EqualTo(new[] { "!" }));
        }

        [TestCase(10, 100000, "BEFORE-SNIP")]
        [TestCase(10, 100000, "BEFORE-SNIP\r\n")]
        public void File_Grew_A_Lot(int initial_file_size, int grow_size, string before_snip)
        {
            var sut = new LineCollector();
            long actual_seek = -1;
            sut.File_Size_Changed(initial_file_size, seek => before_snip);

            // ACT
            sut.File_Size_Changed(initial_file_size + grow_size, seek =>
            {
                actual_seek = seek;
                return "AFTER-SNIP";
            });

            Assert.That(actual_seek, Is.GreaterThan(initial_file_size));
            Assert.That(Lines_Of(sut), Is.EqualTo(new[] {
                "BEFORE-SNIP",
                LineCollector.SNIP,
                "AFTER-SNIP" }));
        }

        [TestCase(0)]
        public void File_Size_Didnt_Change(int initial_file_size)
        {
            var sut = new LineCollector();
            sut.File_Size_Changed(initial_file_size, seek => "" );

            // ACT
            sut.File_Size_Changed(initial_file_size, seek =>
            {
                Assert.Fail("Shouldn't have read the file!");
                return null;
            });

            Assert.That(Lines_Of(sut), Is.Empty);
        }

        static string[] Lines_Of(LineCollector sut)
        {
            return sut.Get_Collection().Select(x => x.Text).ToArray();
        }
    }
}
