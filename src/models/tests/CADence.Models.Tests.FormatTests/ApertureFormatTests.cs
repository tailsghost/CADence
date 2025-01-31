using CADence.Format;
using NUnit.Framework;
using System;

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

    [Test]
    public void ParseFixed_WhenPositiveValueInMillimeters_ReturnsExpectedResult()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureMillimeters();

        var result = _apertureFormat.ParseFixed("242126");

        Assert.That(result, Is.EqualTo(2421260.0));
    }

    [Test]
    public void ParseFixed_WhenPositiveValueInInches_ReturnsExpectedResult()
    {
        _apertureFormat.ConfigureFormat(4, 3);
        _apertureFormat.ConfigureInches();

        var result = _apertureFormat.ParseFixed("242126");

        Assert.That(result, Is.EqualTo(61500004.0));
    }

    [Test]
    public void ParseFixed_WhenUnitNotConfigured_ThrowsInvalidOperationException()
    {
        _apertureFormat.ConfigureFormat(4, 3);

        Assert.That(() => _apertureFormat.ParseFixed("242126"), Throws.InvalidOperationException);
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
}