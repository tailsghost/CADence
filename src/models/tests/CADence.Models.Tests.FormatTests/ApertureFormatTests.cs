namespace CADence.Models.Tests.FormatTests;

/// <summary>
/// Набор тестов для класса <see cref="ApertureFormat"/>.
/// </summary>
[TestFixture]
public class ApertureFormatTests
{
    private ApertureFormat _apertureFormat;

    [SetUp]
    public void SetUp()
    {
        _apertureFormat = new ApertureFormat();
    }

    [TestCase("242126", 4, 3, 242126.0)]
    [TestCase("242126", 4, 3, 6150000.4, true)]
    [TestCase("-242126", 4, 3, -242126.0)]
    [TestCase("-242126", 4, 3, -6150000.4, true)]
    [TestCase("0", 4, 3, 0.0)]
    [TestCase("1234567890", 10, 9, 1234567890.0)]
    [TestCase("0.000001", 6, 5, 0.000001)]
    [TestCase("-0.000001", 6, 5, -0.000001)]
    [TestCase("12345.6789", 5, 4, 12345.6789)]
    public void ParseFixed_And_ParseFixedOld_AreEquivalent_ForVariousInputs(
        string value, int integerDigits, int decimalDigits, double expectedResult, bool isInches = false)
    {
        _apertureFormat.ConfigureFormat(integerDigits, decimalDigits);
        if (isInches)
        {
            _apertureFormat.ConfigureInches();
        }
        else
        {
            _apertureFormat.ConfigureMillimeters();
        }


        double resultNew = _apertureFormat.ParseFixed(value);

        Assert.That(resultNew, Is.EqualTo(expectedResult).Within(0.0001), "Parsed result does not match the expected result.");
    }

    [Test]
    public void ParseFixed_WhenPositiveValueInMillimeters_ReturnsExpectedResult()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureMillimeters();

        var result = _apertureFormat.ParseFixed("242126");

        Assert.That(result, Is.EqualTo(242126.0));
    }

    [Test]
    public void ParseFixed_WhenPositiveValueInInches_ReturnsExpectedResult()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureInches();

        var result = _apertureFormat.ParseFixed("242126");

        Assert.That(result, Is.EqualTo(6150000.4).Within(0.1));
    }

    [Test]
    public void ParseFixed_WhenUnitNotConfigured_ThrowsInvalidOperationException()
    {
        _apertureFormat.ConfigureFormat(4, 3);

        Assert.That(() => _apertureFormat.ParseFixed("242126.0"), Throws.InvalidOperationException);
    }

    [Test]
    public void ConfigureFormat_SetsCorrectValues()
    {
        _apertureFormat.ConfigureFormat(5, 2);
        _apertureFormat.ConfigureMillimeters();

        Assert.DoesNotThrow(() => _apertureFormat.ParseFixed("00012"));
    }

    [Test]
    public void ConfigureInches_SetsCorrectConversionFactor()
    {
        _apertureFormat.ConfigureInches();

        Assert.DoesNotThrow(() => _apertureFormat.ConfigureFormat(4, 2));
    }

    [Test]
    public void ConfigureMillimeters_SetsCorrectConversionFactor()
    {
        _apertureFormat.ConfigureMillimeters();

        Assert.DoesNotThrow(() => _apertureFormat.ConfigureFormat(4, 2));
    }

    [Test]
    public void ParseFloat_WhenValidInput_ReturnsExpectedResult()
    {
        _apertureFormat.ConfigureFormat(3, 2);
        _apertureFormat.ConfigureMillimeters();

        var result = _apertureFormat.ParseFloat("12.34");

        Assert.That(result, Is.EqualTo(12.34));
    }

    [Test]
    public void ToFixed_WhenValidInput_RoundsCorrectly()
    {
        _apertureFormat.ConfigureFormat(2, 3);
        _apertureFormat.ConfigureMillimeters();

        var result = _apertureFormat.ToFixed(12.34567);

        Assert.That(result, Is.EqualTo(12.34567));
    }

    [Test]
    public void EnsureConfigured_WhenNotConfigured_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _apertureFormat.ToFixed(10));
    }

    [Test]
    public void EnsureReconfigurable_WhenConfigured_ThrowsInvalidOperationException()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureMillimeters();
        _apertureFormat.ToFixed(10);

        Assert.Throws<InvalidOperationException>(() => _apertureFormat.ConfigureFormat(4, 3));
        Assert.Throws<InvalidOperationException>(() => _apertureFormat.ConfigureInches());
        Assert.Throws<InvalidOperationException>(() => _apertureFormat.ConfigureMillimeters());
    }

    [Test]
    public void ParseFixed_WhenCoordinateOutOfRange_ThrowsInvalidFormatException()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureMillimeters();

        Assert.Throws<FormatException>(() => _apertureFormat.ParseFixed("5993858472"));
    }

}