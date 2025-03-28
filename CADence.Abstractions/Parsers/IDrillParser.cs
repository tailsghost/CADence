﻿using Clipper2Lib;

namespace CADence.App.Abstractions.Parsers;

public interface IDrillParser
{
    /// <summary>
    /// Выполняет парсинг команд для дырок.
    /// </summary>
    IDrillParser Execute(List<string> drills);

    PathsD GetLayer();
}
