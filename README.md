# Library Documentation

## Overview
This library is a comprehensive solution for working with Gerber files and layers in PCB design. Currently, it supports 274-x. It provides a set of classes to manage various types of PCB layers, including copper layers, solder masks, silkscreen, and substrate. These layers are processed with the help of parsers and factories, which simplifies data manipulation, performing geometric calculations, and creating the final design.

## Installation
To use this library, add the project to your C# solution. .NET Core 6 or later is required.

## Getting Started

### 1. Initializing the Parser
To use the library, you need to initialize the DI containers, obtain the service for parsing the archive, get the factory service, and initialize the layers. For example:

```csharp
ServiceCollectionExtensions.Initial();
```

Then:

```csharp
List<ILayer> layers;
// path - path to the PCB data archive
using (var stream = new MemoryStream(File.ReadAllBytes(path)))
{
    var reader = ServiceCollectionExtensions.GetService<IReader>();
    var data = reader.ParseArchive(stream, Path.GetFileName(path));
    var fabric = ServiceCollectionExtensions.GetService<ILayerFabric>();
    layers = await fabric.GetLayers(data);
}
```

### 2. Creating SVG
After obtaining the layers, you can generate SVG files (back layer, front layer), for example:

```csharp
var svgWriter = ServiceCollectionExtensions.GetService<IWriter>();
// layers - list of layers
// 2 - scale
// true - back layer, false - front layer
// outputPathBack - path to save the file 
svgWriter.Execute(layers, 2, true, outputPathBack);
svgWriter.Execute(layers, 2, false, outputPathFront);
```

This will generate SVG files and save them to the specified paths.

### 3. Calculating the Distance Between PCB Objects
You can calculate the distance between a hole and a pad, as well as the distance between copper traces.
To do this, set the flag to true:

```csharp
ExecuteAccuracy.SetExecute(true);
```

After that, obtain the layers with the required interface, then execute the distance calculation method:

```csharp
var coppers = layers.OfType<ICopper>().ToList();
var box = CalculateAccuracyHelper.Execute(await coppers[0].GetAccuracy(),
                                          await coppers[1].GetAccuracy());
```

#### ⚠️ Attention!
- Due to the nature of the Clipper2 library and the limitations of floating-point precision (double), calculated distances may have a margin of error ranging from 0.5% to 10%, particularly in complex scenarios. Please take this into account when working with strict design constraints. 
- For simple boards (e.g., Board 1), the accuracy class is calculated quite reliably, with minor deviations typically between 0.3% and 1%.
- For more complex boards (Board 2 to Board 5), the error margin can increase up to 10%. This is primarily due to how Clipper2 handles intricate geometry: such boards often consist of multiple sub-boards embedded within a single layout, which can lead to precision loss during boolean operations and distance calculations.

### PCB Layer Previews


#### Board 1
<img src="svgs/outputBack1.svg" width="650"/>
<img src="svgs/outputFront1.svg" width="650"/>

#### Board 2
<img src="svgs/outputBack2.svg" width="650"/>
<img src="svgs/outputFront2.svg" width="650"/>

#### Board 3
<img src="svgs/outputBack3.svg" width="650"/>
<img src="svgs/outputFront3.svg" width="650"/>

#### Board 4
<img src="svgs/outputBack4.svg" width="650"/>
<img src="svgs/outputFront4.svg" width="650"/>

#### Board 5
<img src="svgs/outputBack5.svg" width="650"/>
<img src="svgs/outputFront5.svg" width="650"/>

### Nuget
#### 📦 NuGet: [CADence.Core](https://www.nuget.org/packages/CADence.GerberParser.Core/)
#### 📦 NuGet: [CADence.Abstractions](https://www.nuget.org/packages/CADence.GerberParser.Abstractions/)


## Conclusion
This library provides a powerful and flexible way to work with Gerber files and their layers. It combines parsing, rendering, and precision calculation into a simple and convenient API. Following the provided examples, you can quickly integrate it into your project and start working efficiently with PCB layers.
