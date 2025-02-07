namespace CADence.Infrastructure.Parser.Abstractions;

/// <summary>
/// Абстрактный базовый класс для парсера Gerber команд.
/// </summary>
public abstract class GerberParserBase
{
   /// <summary>
   /// Выполняет парсинг Gerber команд.
   /// </summary>
   public abstract void Execute();
}