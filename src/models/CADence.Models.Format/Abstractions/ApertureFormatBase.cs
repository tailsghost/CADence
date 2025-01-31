namespace CADence.Format.Abstractions;

/// <summary>
/// Абстрактный базовый класс для работы с форматом апертур.
/// </summary>
public abstract class ApertureFormatBase
{
    protected bool _isFormatConfigured = false;
    protected int _integerDigits;
    protected int _decimalDigits;
    protected bool _isUnitConfigured = false;
    protected bool _addTrailingZeros = false;
    protected double _conversionFactor;
    protected bool _isUsed = false;
    protected int QuadrantSegments { get; }
    protected double MitreLimit { get; }

    protected ApertureFormatBase(double mitreLimit = 1, int quadrantSegments = 4)
    {
        MitreLimit = mitreLimit;
        QuadrantSegments = quadrantSegments;
    }

    /// <summary>
    /// Конфигурирует формат чисел.
    /// </summary>
    public abstract void ConfigureFormat(int integerDigits, int decimalDigits);

    /// <summary>
    /// Настраивает добавление нулей в конце числа.
    /// </summary>
    public abstract void ConfigureTrailingZeros(bool addTrailingZeros);

    /// <summary>
    /// Устанавливает единицы измерения в дюймы.
    /// </summary>
    public abstract void ConfigureInches();

    /// <summary>
    /// Устанавливает единицы измерения в миллиметры.
    /// </summary>
    public abstract void ConfigureMillimeters();

    /// <summary>
    /// Разбирает строку фиксированного формата в число.
    /// </summary>
    public abstract double ParseFixed(string value);

    /// <summary>
    /// Разбирает строку с плавающей точкой в число.
    /// </summary>
    public abstract double ParseFloat(string value);

    /// <summary>
    /// Конвертирует число в фиксированный формат.
    /// </summary>
    public abstract double ToFixed(double value);

    /// <summary>
    /// Конвертирует миллиметры в заданный формат.
    /// </summary>
    public abstract double FromMillimeters(double value);

    /// <summary>
    /// Конвертирует число в миллиметры.
    /// </summary>
    public abstract double ToMillimeters(double value, int digits = 2);

    /// <summary>
    /// Проверяет возможность повторной конфигурации.
    /// </summary>
    protected abstract void EnsureReconfigurable();

    /// <summary>
    /// Проверяет, что конфигурация завершена перед использованием.
    /// </summary>
    protected abstract void EnsureConfigured();
}