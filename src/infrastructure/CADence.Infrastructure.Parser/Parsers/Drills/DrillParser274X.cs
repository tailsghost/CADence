using CADence.Infrastructure.Parser.Abstractions;
using CADence.Infrastructure.Parser.Commands.Gerber274x.Fabric;
using CADence.Infrastructure.Parser.Enums;
using CADence.Infrastructure.Parser.Settings;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using System;
using System.Collections.Generic;
using System.IO;

namespace CADence.Infrastructure.Parser.Parsers.Drills;

/// <summary>
/// Парсер Drill файлов формата 274X.
/// </summary>
public class DrillParser274X : DrillParserBase
{
    /// <summary>
    /// Список файлов дырок.
    /// </summary>
    private List<string> DRILLS = new();

    /// <summary>
    /// Коллекция для хранения всех дырок
    /// </summary>
    private List<Geometry> polygons = new();

    /// <summary>
    /// Настройки для парсера.
    /// </summary>
    private DrillParser274xSettings _settings = new DrillParser274xSettings();

    /// <summary>
    /// Фабрика комманд
    /// </summary>
    private Drill274xFabric _fabric = new();

    /// <summary>
    /// Минимальный диаметр отверстия
    /// </summary>
    public double MinHoleDiameter { get; private set; } = double.MaxValue;

    /// <summary>
    /// Инициализирует новый экземпляр парсера Drill 274X.
    /// </summary>
    /// <param name="drills">Строковое представление файлы.</param>
    public DrillParser274X(List<string> drills)
    {
        DRILLS = drills;
        Execute();
    }


    /// <summary>
    /// Выполняет парсинг Drill файлов.
    /// </summary>
    /// <exception cref="InvalidOperationException">Выбрасывается, если хотя бы одна команда не была выполнена.</exception>
    public override void Execute()
    {
        foreach (string drill in DRILLS)
        {
            using var stream = new StringReader(string.Join("\n", drill));
            string? line;

            _settings = new DrillParser274xSettings();
            SetupSettings();

            while ((line = stream.ReadLine()) != null)
            {
                _settings.cmd = line;
                _settings = ExecuteCommand();
                if (!_settings.IsDone) break;
            }

            var geom = GetGeometryDrill();

            if (geom != null)
            {
                polygons.Add(geom);
            }

            MinHoleDiameter = Math.Min(_settings.MinHole, MinHoleDiameter);
        }

        _drillGeometry = UnaryUnionOp.Union(polygons);
    }

    /// <summary>
    /// Получает результат парсинга.
    /// </summary>
    /// <param name="plated">Учитывать платированные отверстия.</param>
    /// <param name="unplated">Учитывать неплатированные отверстия.</param>
    /// <returns>Геометрия результата парсинга.</returns>
    private Geometry? GetGeometryDrill(bool plated = true, bool unplated = true)
    {
        Geometry? result = null;

        if (plated)
        {
            if (unplated)
            {
                Geometry? platedGeom = _settings.Pth.GetAdditive();
                Geometry? unplatedGeom = _settings.Npth.GetAdditive();

                if (platedGeom != null && unplatedGeom != null)
                {
                    result = platedGeom.Union(unplatedGeom);
                }
                else if (platedGeom != null)
                {
                    result = platedGeom;
                }
                else
                {
                    result = unplatedGeom;
                }
            }
            else
            {
                result = _settings.Pth.GetAdditive();
            }
        }

        return result?.Reverse();
    }

    /// <summary>
    /// Выполняет команды в зависимости от первого символа.
    /// </summary>
    /// <returns>Обновленные настройки парсера.</returns>
    /// <exception cref="InvalidOperationException">Выбрасывается, если была обнаружена неизвестная команда.</exception>
    private DrillParser274xSettings ExecuteCommand()
    {
        if (_settings.ParseState == ParseState.HEADER)
        {
            switch (_settings.cmd[0])
            {
                case ';':
                    _settings.cmd = _settings.cmd.Substring(1);
                    return _fabric.ExecuteCommand(_settings);
                case 'T':
                    _settings.fcmd = _settings.cmd;
                    _settings.cmd = "T";
                    return _fabric.ExecuteCommand(_settings);
                default:
                    return _fabric.ExecuteCommand(_settings);
            }
        }
        else if (_settings.ParseState == ParseState.BODY)
        {
            if (_settings.cmd[0] != ';')
            {
                switch (_settings.cmd[0])
                {
                    case 'X':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "X";
                        return _fabric.ExecuteCommand(_settings);
                    case 'Y':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "Y";
                        return _fabric.ExecuteCommand(_settings);
                    case 'T':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "T";
                        return _fabric.ExecuteCommand(_settings);
                    case 'G':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "G";
                        return _fabric.ExecuteCommand(_settings);
                    case 'M':
                        _settings.fcmd = _settings.cmd;
                        _settings.cmd = "M";
                        return _fabric.ExecuteCommand(_settings);
                    default:
                        throw new InvalidOperationException("unknown/unexpected command: " + _settings.cmd);
                }
            }
            _settings.IsDone = true;
            return _settings;
        }

        return _fabric.ExecuteCommand(_settings);
    }


    /// <summary>
    /// Начальная настройка параметров
    /// </summary>
    private void SetupSettings()
    {
        _settings.ParseState = ParseState.PRE_HEADER;
        _settings.format.ConfigureFormat(4, 3);
        _settings.format.ConfigureMillimeters();
        _settings.Point = new Point(0, 0);
        _settings.RoutMode = RoutMode.DRILL;
    }
}
