using CADence.Layer.Abstractions;
using CADence.Layer.Enums;
using NetTopologySuite.Geometries;
using System.Globalization;
using System.Text;

namespace CADence.Infrastructure.SVG_JSON;

/// <summary>
/// Класс для генерации SVG изображений на основе списка слоев.
/// </summary>
public class SVGWriter
{
    private List<LayerBase> _layers;
    private readonly double _scale;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SVGWriter"/>.
    /// </summary>
    /// <param name="layers">Список слоев, используемых для генерации SVG.</param>
    /// <param name="scale">Масштаб, применяемый к SVG изображению.</param>
    public SVGWriter(List<LayerBase> layers, double scale)
    {
        _layers = layers;
        _scale = scale;
    }

    /// <summary>
    /// Генерирует SVG изображение с учетом порядка слоев.
    /// Если <paramref name="flipped"/> равно <c>true</c>, то используется порядок: TopFinish, TopCopper, TopMask, TopSilk, Substrate.
    /// Если <paramref name="flipped"/> равно <c>false</c>, то используется порядок: BottomFinish, BottomSilk, BottomMask, BottomCopper, Substrate.
    /// </summary>
    /// <param name="flipped">
    /// Флаг, определяющий порядок вывода слоев.
    /// Если <c>true</c> — используется порядок верхних слоев, если <c>false</c> — нижних.
    /// </param>
    /// <param name="path">
    /// Путь для сохранения SVG файла.
    /// Если параметр не указан или является пустой строкой, метод возвращает сгенерированное SVG изображение в виде строки.
    /// </param>
    /// <returns>
    /// Возвращает строку, содержащую SVG изображение, если путь для сохранения не указан; иначе возвращается пустая строка.
    /// </returns>
    public string Execute(bool flipped, string path)
    {
        StringBuilder stream = new();

        var bounds = _layers[0].GetLayer().EnvelopeInternal;

        var width = (bounds.Width) + 20;
        var height = (bounds.Height) + 20;

        stream.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "<svg viewBox=\"0 0 {0} {1}\" width=\"{2}\" height=\"{3}\" xmlns=\"http://www.w3.org/2000/svg\">",
        width, height, width * _scale, height * _scale));

        var tx = 10 - (flipped ? (-bounds.MaxX) : bounds.MinX);
        var ty = 10 + (bounds.MaxY);

        stream.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "<g transform=\"translate({0} {1}) scale({2} -1)\" filter=\"drop-shadow(0 0 1 rgba(0, 0, 0, 0.2))\">",
            tx, ty, flipped ? "-1" : "1"));

        var resultLayers = GetOrderedLayers(flipped);

        for (var i = 0; i < resultLayers.Count; i++)
        {
            var layer = resultLayers[i];
            stream.Append(ParseGeometry(layer));
        }

        stream.AppendLine("</g>");
        stream.AppendLine("</svg>");

        if (!string.IsNullOrWhiteSpace(path))
        {
            File.WriteAllText(path, stream.ToString());
            return string.Empty;
        }

        return stream.ToString();
    }

    /// <summary>
    /// Преобразует геометрию слоя в SVG элементы.
    /// </summary>
    /// <param name="layer">Объект слоя, содержащий геометрические данные.</param>
    /// <returns>Буфер, содержащий SVG разметку для данного слоя.</returns>
    private StringBuilder ParseGeometry(LayerBase layer)
    {
        StringBuilder Data = new();
        var name = layer.Layer.ToString();
        var th = layer.Thickness;
        Data.Append(string.Format(CultureInfo.InvariantCulture, "<g id=\"{0}\">\n", $"{name}-{th}"));

        if (layer.ColorLayer.A == 0.0) return Data;

        Data.Append(string.Format(CultureInfo.InvariantCulture,
            "<path fill=\"rgb({0},{1},{2})\" stroke=\"none\"",
            (int)(layer.ColorLayer.R * 255), (int)(layer.ColorLayer.G * 255), (int)(layer.ColorLayer.B * 255)));

        if (layer.ColorLayer.A < 1.0)
        {
            Data.Append(string.Format(CultureInfo.InvariantCulture, " fill-opacity=\"{0}\"", layer.ColorLayer.A));
        }

        Data.Append(" d=\"");

        if (layer.GetLayer() is Polygon polygon)
        {
            AppendPolygon(polygon, Data);
        }
        else if (layer.GetLayer() is MultiPolygon multiPolygon)
        {
            for (int i = 0; i < multiPolygon.Geometries.Length; i++)
            {
                AppendPolygon((Polygon)multiPolygon.Geometries[i], Data);
            }
        }
        else if (layer.GetLayer() is LineString lineString)
        {
            AppendLineString(lineString, Data);
        }

        Data.Append("\"/>\n");
        Data.AppendLine("</g>");

        return Data;
    }

    /// <summary>
    /// Добавляет координаты полигона в строку SVG разметки.
    /// Обрабатывает как внешнее кольцо, так и внутренние.
    /// </summary>
    /// <param name="polygon">Объект типа <see cref="Polygon"/>, содержащий геометрию полигона.</param>
    /// <param name="data">Буфер для формирования SVG команды.</param>
    private void AppendPolygon(Polygon polygon, StringBuilder data)
    {
        AppendLineString(polygon.ExteriorRing, data);

        for (int i = 0; i < polygon.NumInteriorRings; i++)
        {
            AppendLineString(polygon.GetInteriorRingN(i), data);
        }
    }

    /// <summary>
    /// Добавляет координаты линии в строку SVG разметки.
    /// </summary>
    /// <param name="lineString">Объект типа <see cref="LineString"/>, содержащий координаты линии.</param>
    /// <param name="data">Буфер для формирования SVG команды.</param>
    private void AppendLineString(LineString lineString, StringBuilder data)
    {
        var coordinates = lineString.Coordinates;
        if (coordinates.Length > 0)
        {
            data.Append(string.Format(CultureInfo.InvariantCulture, "M {0} {1} ",
                coordinates[0].X, coordinates[0].Y));

            for (int i = 1; i < coordinates.Length; i++)
            {
                data.Append(string.Format(CultureInfo.InvariantCulture, "L {0} {1} ",
                    coordinates[i].X, coordinates[i].Y));
            }
        }
    }

    /// <summary>
    /// Добавляет координаты полигона в список SVG элементов.
    /// Используется для формирования отдельных элементов разметки.
    /// </summary>
    /// <param name="polygon">Объект типа <see cref="Polygon"/> с данными полигона.</param>
    /// <param name="elements">Список строк, в который добавляются SVG команды.</param>
    private void AppendPolygonToElements(Polygon polygon, List<string> elements)
    {
        AppendLineStringToElements(polygon.ExteriorRing, elements);

        for (int i = 0; i < polygon.NumInteriorRings; i++)
        {
            AppendLineStringToElements(polygon.GetInteriorRingN(i), elements);
        }
    }

    /// <summary>
    /// Добавляет координаты линии в список SVG элементов.
    /// </summary>
    /// <param name="lineString">Объект типа <see cref="LineString"/> с координатами линии.</param>
    /// <param name="elements">Список строк, в который добавляются SVG команды.</param>
    private void AppendLineStringToElements(LineString lineString, List<string> elements)
    {
        var coordinates = lineString.Coordinates;
        if (coordinates.Length > 0)
        {
            elements.Add(string.Format(CultureInfo.InvariantCulture, "M {0} {1}",
                coordinates[0].X, coordinates[0].Y).Trim());

            for (int i = 1; i < coordinates.Length; i++)
            {
                elements.Add(string.Format(CultureInfo.InvariantCulture, "L {0} {1}",
                    coordinates[i].X, coordinates[i].Y).Trim());
            }
        }
    }


    /// <summary>
    /// Возвращает список слоев в требуемом порядке.
    /// Порядок определяется значением параметра <paramref name="flipped"/>.
    /// </summary>
    /// <param name="flipped">
    /// Если <c>true</c>, то возвращается порядок: TopFinish, TopCopper, TopMask, TopSilk, Substrate.
    /// Если <c>false</c>, то возвращается порядок: BottomFinish, BottomSilk, BottomMask, BottomCopper, Substrate.
    /// </param>
    /// <returns>Список объектов <see cref="LayerBase"/>, отсортированных согласно заданному порядку.</returns>
    public List<LayerBase> GetOrderedLayers(bool flipped)
    {
        GerberLayer[] order = flipped
            ? new[] { GerberLayer.Substrate,GerberLayer.BottomSilk,GerberLayer.BottomCopper,GerberLayer.BottomMask,GerberLayer.BottomFinish
                 }
            : new[] {GerberLayer.Substrate,GerberLayer.TopCopper,GerberLayer.TopMask,GerberLayer.TopSilk, GerberLayer.TopFinish,
            };

        var orderedLayers = new List<LayerBase>();

        for (int i = 0; i < order.Length; i ++)
        {
            var gerberLayer = order[i];
            var layer = _layers.FirstOrDefault(l => l.Layer == gerberLayer);
            if (layer != null)
            {
                orderedLayers.Add(layer);
            }
        }

        return orderedLayers;
    }
}
