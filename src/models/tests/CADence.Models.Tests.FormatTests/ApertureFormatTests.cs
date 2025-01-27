using CADence.Format;

namespace CADence.Models.Tests.FormatTests
{
    public class Tests
    {
        private ApertureFormat _apertureFormat;

        [SetUp]
        public void Setup()
        {
            _apertureFormat = new ApertureFormat();
        }

        [Test]
        public void Test1()
        {
            _apertureFormat.

            _apertureFormat.ParseFixed("242126");

            Assert.Pass();
        }
    }
}
