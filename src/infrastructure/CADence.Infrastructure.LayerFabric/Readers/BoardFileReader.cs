using SharpCompress.Archives.Rar;
using System.IO.Compression;

namespace CADence.Infrastructure.LayerFabric.Readers
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

        private bool isValidFile(string filename)
        {
            if (filename != string.Empty)
            {
                var ext = new FileInfo(filename).Extension.ToLower().TrimStart('.');

                return supported.Contains(ext);
            }
            else return false;
        }

        private readonly IEnumerable<string> supported = new List<string>() { "zip", "rar" };
    }
}
