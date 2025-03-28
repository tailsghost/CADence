﻿using Clipper2Lib;

namespace CADence.App.Abstractions.Parsers;

public interface IGerberParser
{
    IGerberParser Execute(string file);
    PathsD GetResult(bool BoardOutLine = false);
}
