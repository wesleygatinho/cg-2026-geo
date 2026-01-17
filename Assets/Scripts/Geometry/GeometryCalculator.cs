using System;

namespace ARGeometryGame.Geometry
{
    public static class GeometryCalculator
    {
        public static double Compute(GeometryQuestion q)
        {
            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            return q.shape switch
            {
                GeometryShapeKind.Rectangle => ComputeRectangle(q.metric, q.a, q.b),
                GeometryShapeKind.Triangle => ComputeTriangle(q.metric, q.a, q.b, q.c),
                GeometryShapeKind.Circle => ComputeCircle(q.metric, q.r),
                GeometryShapeKind.Cube => ComputeCube(q.metric, q.a),
                GeometryShapeKind.Cuboid => ComputeCuboid(q.metric, q.a, q.b, q.c),
                GeometryShapeKind.Cylinder => ComputeCylinder(q.metric, q.r, q.h),
                GeometryShapeKind.Sphere => ComputeSphere(q.metric, q.r),
                _ => throw new NotSupportedException($"Forma não suportada: {q.shape}")
            };
        }

        private static double ComputeRectangle(GeometryMetric metric, double a, double b)
        {
            return metric switch
            {
                GeometryMetric.Perimeter => 2d * (a + b),
                GeometryMetric.Area => a * b,
                _ => throw new NotSupportedException($"Métrica não suportada para retângulo: {metric}")
            };
        }

        private static double ComputeTriangle(GeometryMetric metric, double a, double b, double c)
        {
            return metric switch
            {
                GeometryMetric.Perimeter => a + b + c,
                GeometryMetric.Area => ComputeTriangleAreaHeron(a, b, c),
                _ => throw new NotSupportedException($"Métrica não suportada para triângulo: {metric}")
            };
        }

        private static double ComputeTriangleAreaHeron(double a, double b, double c)
        {
            var s = (a + b + c) / 2d;
            var under = s * (s - a) * (s - b) * (s - c);
            return under <= 0 ? 0 : Math.Sqrt(under);
        }

        private static double ComputeCircle(GeometryMetric metric, double r)
        {
            return metric switch
            {
                GeometryMetric.Perimeter => 2d * Math.PI * r,
                GeometryMetric.Area => Math.PI * r * r,
                _ => throw new NotSupportedException($"Métrica não suportada para círculo: {metric}")
            };
        }

        private static double ComputeCube(GeometryMetric metric, double a)
        {
            return metric switch
            {
                GeometryMetric.Volume => a * a * a,
                GeometryMetric.SurfaceArea => 6d * a * a,
                _ => throw new NotSupportedException($"Métrica não suportada para cubo: {metric}")
            };
        }

        private static double ComputeCuboid(GeometryMetric metric, double a, double b, double c)
        {
            return metric switch
            {
                GeometryMetric.Volume => a * b * c,
                GeometryMetric.SurfaceArea => 2d * (a * b + a * c + b * c),
                _ => throw new NotSupportedException($"Métrica não suportada para paralelepípedo: {metric}")
            };
        }

        private static double ComputeCylinder(GeometryMetric metric, double r, double h)
        {
            return metric switch
            {
                GeometryMetric.Volume => Math.PI * r * r * h,
                GeometryMetric.SurfaceArea => 2d * Math.PI * r * (r + h),
                _ => throw new NotSupportedException($"Métrica não suportada para cilindro: {metric}")
            };
        }

        private static double ComputeSphere(GeometryMetric metric, double r)
        {
            return metric switch
            {
                GeometryMetric.Volume => 4d / 3d * Math.PI * r * r * r,
                GeometryMetric.SurfaceArea => 4d * Math.PI * r * r,
                _ => throw new NotSupportedException($"Métrica não suportada para esfera: {metric}")
            };
        }
    }
}

