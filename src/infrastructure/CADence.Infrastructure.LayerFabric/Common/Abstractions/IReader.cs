namespace CADence.Infrastructure.LayerFabric.Common.Abstractions;

public interface IReader
{
    IInputData ParseArchive(Stream stream, string fileName);
}