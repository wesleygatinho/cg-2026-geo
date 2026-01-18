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

        // Multiple-choice support (optional). If `choices` is set and has length > 0,
        // the UI will present these options and the `correctIndex` indicates the
        // index of the correct choice.
        public string[] choices;
        public int correctIndex = -1;
    }
}

