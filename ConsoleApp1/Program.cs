using CADence.Infrastructure.LayerFabric.Fabrics.Gerber274x;
using CADence.Infrastructure.LayerFabric.Readers;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            byte[] file = File.ReadAllBytes("C:/Users/Андрей/Desktop/M3CITY2REV0Gerber.zip");
            var stream = new MemoryStream(file);
            BoardFileReader reader = new();
            var data = reader.ParseArchive(stream, "M3CITY2REV0Gerber.zip");
            LayerFabricGerber274x fabric = new();
            fabric.GetLayers(data);
        }
    }
}
