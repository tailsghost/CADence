using CADence.Format;
using Moq;

namespace CADence.Models.Tests.FormatTests;

public class ApertureFormatTests
{
    private ApertureFormat _apertureFormat;

    [SetUp]
    public void Setup()
    {
        _apertureFormat = new ApertureFormat();
    }

    [Test]
    public void ParseFixedPositiveMM()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureMM();

        var result = _apertureFormat.ParseFixed("242126");

        Assert.That(result, Is.EqualTo(2421260.0));
    }

    [Test]
    public void ParseFixedPositiveInch()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureInch();

        var result = _apertureFormat.ParseFixed("242126");

        Assert.That(result, Is.EqualTo(61500004.0));
    }

    [Test]
    public void ParseFixedNegativeMM()
    {
        _apertureFormat.ConfigureFormat(4, 3);

        Assert.That(() => _apertureFormat.ParseFixed("242126"), Throws.TypeOf<InvalidOperationException>());
    }
}
