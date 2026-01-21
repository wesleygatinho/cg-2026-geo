using System;
using System.Globalization;

namespace ARGeometryGame.Geometry
{
    public static class GeometryAnswerValidator
    {
        public static bool TryParseAnswer(string raw, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            var normalized = raw.Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static bool IsCorrect(GeometryQuestion q, double userAnswer)
        {
            var expected = GeometryCalculator.Compute(q);
            var tol = q == null ? 0 : Math.Max(0d, q.tolerance);
            // Aceita respostas dentro da margem de erro (tolerance)
            // Para respostas inteiras, a tolerância é aumentada automaticamente
            return Math.Abs(expected - userAnswer) <= tol;
        }

        public static double ExpectedAnswer(GeometryQuestion q) => GeometryCalculator.Compute(q);
    }
}

