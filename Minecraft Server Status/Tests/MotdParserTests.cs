using NUnit.Framework;

namespace Minecraft_Server_Status.Tests {
    [TestFixture]
    internal class MotdParserTests {

        [Test]
        public void EmptyMotdTest() {
            const string expected = "<html><body style=\"background-color:#2E2E2E;" +
                                    "font-family:'monospace';font-size:14px;\"></body></html>";
            Assert.AreEqual(MotdParser.ParseMotd(""), expected);
        }

    }
}