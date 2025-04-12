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
using CADence.Abstractions.Accuracy;
using CADence.Core.Accuracy;
using GCommand = CADence.Core.Commands.Drill.GCommand;

namespace CADence.Core.Dependency;

/// <summary>
/// Contains extension methods for setting up dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static IServiceProvider _provider;

    /// <summary>
    /// Initializes the service provider by building the dependency injection container.
    /// </summary>
    public static void Initial()
    {
        if (_provider == null)
        {
            var services = new ServiceCollection();

            _provider = services.InitialServiceCollection().BuildServiceProvider();

        }
    }

    /// <summary>
    /// Retrieves a required service of type T.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <returns>An instance of type T.</returns>
    public static T GetService<T>()
    {
        try
        {
            if (_provider == null)
            {
                Console.WriteLine("Services must be initialized first!");
            }

            return _provider.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return default;
        }
    }


    /// <summary>
    /// Configures the service collection with necessary dependencies.
    /// </summary>
    /// <param name="collection">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
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


        collection.AddTransient<Substrate>();

        collection.AddTransient<Func<Substrate, string, BottomCopper>>(sp =>

            (substrate, file) =>
            {
                var parser = sp.GetRequiredService<IGerberParser>();
                var accuracy = sp.GetRequiredService<ICalculateAccuracy>();

                return new BottomCopper(parser, accuracy, substrate, file);
            }
        );

        collection.AddTransient<Func<BottomMask, BottomCopper, BottomFinish>>(sp
            => (mask, copper) => new BottomFinish(mask, copper));

        collection.AddTransient<Func<Substrate, string, BottomMask>>(sp =>
        
            (substrate, file) =>
            {
                var parser = _provider.GetRequiredService<IGerberParser>();
                return new BottomMask(parser, substrate, file);
            }
        );

        collection.AddTransient<Func<BottomMask, string, BottomSilk>>(sp =>
            (mask, file) =>
            {
                var parser = _provider.GetRequiredService<IGerberParser>();
                return new BottomSilk(parser, mask, file);
            }
        );

        collection.AddTransient<Func<Substrate, string, TopCopper>>(sp =>

            (substrate, file) =>
            {
                var parser = sp.GetRequiredService<IGerberParser>();
                var accuracy = sp.GetRequiredService<ICalculateAccuracy>();

                return new TopCopper(parser, accuracy, substrate, file);
            }
        );

        collection.AddTransient<Func<TopMask, TopCopper, TopFinish>>(sp
            => (mask, copper) => new TopFinish(mask, copper));

        collection.AddTransient<Func<Substrate, string, TopMask>>(sp =>

            (substrate, file) =>
            {
                var parser = _provider.GetRequiredService<IGerberParser>();
                return new TopMask(parser, substrate, file);
            }
        );

        collection.AddTransient<Func<TopMask, string, TopSilk>>(sp =>
            (mask, file) =>
            {
                var parser = _provider.GetRequiredService<IGerberParser>();
                return new TopSilk(parser, mask, file);
            }
        );


        return collection;

    }
}
