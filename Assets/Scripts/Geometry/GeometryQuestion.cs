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

        // Fator que relaciona o tamanho visual do objeto (pequeno na cena)
        // ao tamanho real usado nos cálculos. Ex: visualScale=0.1 -> valores
        // reais são 10x maiores que os valores armazenados no JSON.
        public float visualScale = 1f;
    }
}

