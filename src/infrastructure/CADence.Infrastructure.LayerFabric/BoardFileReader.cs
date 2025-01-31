using SharpCompress.Archives.Rar;
using System.IO.Compression;

namespace CADence.LayerFabric
{
    public class BoardFileReader
    {
        public BoardFileType FindFileType(MemoryStream ms)
        {
            try
            {
                ms.Seek(0, SeekOrigin.Begin);

                using var reader = new StreamReader(ms);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("%FS")) return BoardFileType.Gerber;
                    if (line.Contains("M48")) return BoardFileType.Drill;
                }
            }
            catch
            {
                return BoardFileType.Unsupported;
            }

            return BoardFileType.Unsupported;
        }
    }
}
