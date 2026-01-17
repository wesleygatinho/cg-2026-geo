using System;
using System.Collections.Generic;

namespace ARGeometryGame.Geometry
{
    public static class QuestionSelector
    {
        public static List<GeometryQuestion> SelectRandom(GeometryQuestion[] source, int count, int seed)
        {
            var result = new List<GeometryQuestion>();
            if (source == null || source.Length == 0 || count <= 0)
            {
                return result;
            }

            var rng = new Random(seed);
            var indices = new List<int>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                indices.Add(i);
            }

            for (var i = indices.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            var take = Math.Min(count, indices.Count);
            for (var i = 0; i < take; i++)
            {
                result.Add(source[indices[i]]);
            }

            return result;
        }
    }
}

