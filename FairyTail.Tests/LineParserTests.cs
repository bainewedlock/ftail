using NUnit.Framework;

namespace FairyTail.Tests
{
    [TestFixture]
    public class LineParserTests
    {
        [Test]
        public void Simple()
        {
            var sut = new LineParser();

            sut.Append_Text("Hallo");
            sut.Append_Text("\r\nWelt");

            Assert.That(sut.Get_Collection(), Is.EqualTo(new[] { "Hallo", "Welt" }));
        }

        [Test]
        public void Append_Last_Line()
        {
            var sut = new LineParser();

            sut.Append_Text("Hallo");
            sut.Append_Text(" Welt\r\nOder was");

            Assert.That(sut.Get_Collection(), Is.EqualTo(new[] { "Hallo Welt", "Oder was" }));
        }

        [Test]
        public void File_Starts_With_NewLine()
        {
            var sut = new LineParser();

            sut.Append_Text("\r\nstadden");

            Assert.That(sut.Get_Collection(), Is.EqualTo(new[] { "", "stadden" }));
        }

        [Test]
        public void File_Ends_With_NewLine()
        {
            var sut = new LineParser();

            sut.Append_Text("whoot\r\n");

            Assert.That(sut.Get_Collection(), Is.EqualTo(new[] { "whoot", "" }));
        }


        [Test]
        public void Clean_Up()
        {
            var sut = new LineParser();
            sut.Lines_To_Keep = 2;
            sut.Append_Text("wie\r\nwer\r\nwas\r\nwarum");

            Assert.That(sut.Get_Collection(), Is.EqualTo(new[] { "was", "warum"}));
        }
    }
}
