using System;

namespace ARGeometryGame.Geometry
{
    public static class GeometryFormulaHelper
    {
        public static string GetFormula(GeometryShapeKind shape, GeometryMetric metric)
        {
            return shape switch
            {
                GeometryShapeKind.Rectangle => metric switch
                {
                    GeometryMetric.Perimeter => "P = 2 * (a + b)",
                    GeometryMetric.Area => "A = a * b",
                    _ => ""
                },
                GeometryShapeKind.Triangle => metric switch
                {
                    GeometryMetric.Perimeter => "P = a + b + c",
                    GeometryMetric.Area => "A = sqrt(s * (s-a) * (s-b) * (s-c)), onde s = P/2",
                    _ => ""
                },
                GeometryShapeKind.Circle => metric switch
                {
                    GeometryMetric.Perimeter => "C = 2 * pi * r",
                    GeometryMetric.Area => "A = pi * r^2",
                    _ => ""
                },
                GeometryShapeKind.Cube => metric switch
                {
                    GeometryMetric.Volume => "V = a^3",
                    GeometryMetric.SurfaceArea => "A = 6 * a^2",
                    _ => ""
                },
                GeometryShapeKind.Cuboid => metric switch
                {
                    GeometryMetric.Volume => "V = a * b * c",
                    GeometryMetric.SurfaceArea => "A = 2 * (ab + ac + bc)",
                    _ => ""
                },
                GeometryShapeKind.Cylinder => metric switch
                {
                    GeometryMetric.Volume => "V = pi * r^2 * h",
                    GeometryMetric.SurfaceArea => "A = 2 * pi * r * (r + h)",
                    _ => ""
                },
                GeometryShapeKind.Sphere => metric switch
                {
                    GeometryMetric.Volume => "V = (4/3) * pi * r^3",
                    GeometryMetric.SurfaceArea => "A = 4 * pi * r^2",
                    _ => ""
                },
                _ => ""
            };
        }
    }
}
