using System;
using System.IO;
using UnityEngine;

namespace ARGeometryGame.Geometry
{
    public static class QuestionBankLoader
    {
        public static GeometryQuestion[] LoadQuestionsFromStreamingAssets(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Nome do arquivo inválido.", nameof(fileName));
            }

            var path = Path.Combine(Application.streamingAssetsPath, fileName);
            string json = null;

            if (File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
            else
            {
                var key = Path.GetFileNameWithoutExtension(fileName);
                var asset = Resources.Load<TextAsset>(key);
                if (asset != null)
                {
                    json = asset.text;
                }
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<GeometryQuestion>();
            }

            var parsed = JsonUtility.FromJson<GeometryQuestionList>(json);
            if (parsed?.questions == null)
            {
                return Array.Empty<GeometryQuestion>();
            }

            return parsed.questions;
        }
    }
}
