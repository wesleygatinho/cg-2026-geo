using System;

namespace ARGeometryGame.Geometry
{
    [Serializable]
    public sealed class GeometryQuestion
    {
        public string id;
        public string prompt;
        public GeometryShapeKind shape;
        public GeometryMetric metric;

        public float a;
        public float b;
        public float c;
        public float r;
        public float h;

        public float tolerance;
        public string unit;

        // Fator opcional para reduzir o tamanho visual sem alterar os valores reais usados nos cálculos.
        public float visualScale = 1f;
    }
}

