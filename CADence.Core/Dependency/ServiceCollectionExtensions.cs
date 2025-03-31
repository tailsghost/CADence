using CADence.Abstractions.Apertures;
using CADence.Abstractions.Commands;
using CADence.Abstractions.Layers;
using CADence.Abstractions.Readers;
using CADence.Abstractions.Svg_Json;
using CADence.App.Abstractions.Formats;
using CADence.App.Abstractions.Layers.Gerber_274x;
using CADence.App.Abstractions.Parsers;
using CADence.Core.Apertures.Gerber_274;
using CADence.Core.Commands.Drill;
using CADence.Core.Commands.Gerber;
using CADence.Core.Fabrics;
using CADence.Core.Formats;
using CADence.Core.Parsers;
using CADence.Core.Readers;
using CADence.Core.Settings;
using CADence.Core.SVG_JSON;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;
using CADence.Abstractions.Accuracy;
using CADence.Core.Accuracy;
using GCommand = CADence.Core.Commands.Drill.GCommand;

namespace CADence.Core.Dependency;

public static class ServiceCollectionExtensions
{
    private static IServiceProvider _provider;

    public static void Initial()
    {
        if (_provider == null)
        {
            var services = new ServiceCollection();

            _provider = services.InitialServiceCollection().BuildServiceProvider();

        }
    }

    public static T GetService<T>()
    {
        try
        {
            if (_provider == null)
            {
                Console.WriteLine("Сначала необходимо инициализировать сервисы!");
            }

            return _provider.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return default;
        }
    }

    private static IServiceCollection InitialServiceCollection(this IServiceCollection collection)
    {
        // Aperture
        collection.AddTransient<Circle>();
        collection.AddTransient<Drill>();
        collection.AddTransient<Obround>();
        collection.AddTransient<Polygon>();
        collection.AddTransient<Rectangle>();
        collection.AddTransient<Unknown>();

        collection.AddSingleton<ICalculateAccuracy, CalculateAccuracy>();

        // ApertureMacro
        collection.AddTransient<IApertureMacro, ApertureMacro>();

        // Parser
        collection.AddTransient<IDrillParser, DrillParser274X>();
        collection.AddTransient<IGerberParser, GerberParser274X>();

        // Settings
        collection.AddTransient<IDrillSettings, DrillParser274xSettings>();
        collection.AddTransient<IGerberSettings, GerberParser274xSettings>();

        collection.AddTransient<Tool>();

        // Fabrics Command
        collection.AddSingleton<CoordinateCommand>();
        collection.AddSingleton<EndHeaderCommand>();
        collection.AddSingleton<GCommand>();
        collection.AddSingleton<HeaderCommentCommand>();
        collection.AddSingleton<InchCommand>();
        collection.AddSingleton<M48Command>();
        collection.AddSingleton<MCommand>();
        collection.AddSingleton<MetricCommand>();
        collection.AddSingleton<NoOpCommand>();
        collection.AddSingleton<ToolChangeCommand>();

        collection.AddSingleton<ABCommand>();
        collection.AddSingleton<ADCommand>();
        collection.AddSingleton<AMCommand>();
        collection.AddSingleton<FSCommand>();
        collection.AddSingleton<Commands.Gerber.GCommand>();
        collection.AddSingleton<InstallCommand>();
        collection.AddSingleton<LCommand>();
        collection.AddSingleton<M0Command>();
        collection.AddSingleton<MOCommand>();


        // Layer Fabric
        collection.AddTransient<IInputData, InputData>();
        collection.AddSingleton<IReader, BoardFileReader>();
        collection.AddTransient<ILayerFabric, LayerFabricGerber274x>();
        collection.AddSingleton<IFabricCommand<IDrillSettings>, DrillFabricCommand>();
        collection.AddSingleton<IFabricCommand<IGerberSettings>, GerberFabricCommand>();
        // Svg
        collection.AddSingleton<IWriter, SVGWriter>();

        // Format
        collection.AddTransient<ILayerFormat, LayerFormat>();

        //Layers
        collection.AddTransient<BottomCopper>();
        collection.AddTransient<BottomFinish>();
        collection.AddTransient<BottomMask>();
        collection.AddTransient<BottomSilk>();
        collection.AddTransient<Substrate>();
        collection.AddTransient<TopCopper>();
        collection.AddTransient<TopFinish>();
        collection.AddTransient<TopMask>();
        collection.AddTransient<TopSilk>();


        return collection;

    }
}
