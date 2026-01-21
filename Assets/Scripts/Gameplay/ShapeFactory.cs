using ARGeometryGame.Geometry;
using UnityEngine;

namespace ARGeometryGame.Gameplay
{
    public static class ShapeFactory
    {
        public static GameObject CreateShapeVisual(GeometryQuestion q)
        {
            var root = new GameObject("Shape");

            var go = q.shape switch
            {
                GeometryShapeKind.Rectangle => CreateRectangle(q.a, q.b),
                GeometryShapeKind.Triangle => CreateTriangle(q.a, q.b, q.c),
                GeometryShapeKind.Circle => CreateDisk(q.r),
                GeometryShapeKind.Cube => CreateCube(q.a),
                GeometryShapeKind.Cuboid => CreateCuboid(q.a, q.b, q.c),
                GeometryShapeKind.Cylinder => CreateCylinder(q.r, q.h),
                GeometryShapeKind.Sphere => CreateSphere(q.r),
                _ => CreateCube(0.2f)
            };

            go.transform.SetParent(root.transform, false);
            ApplyMaterial(go, q.shape);
            return root;
        }

        private static void ApplyMaterial(GameObject go, GeometryShapeKind shape)
        {
            var shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null)
            {
                shader = Shader.Find("Mobile/Diffuse");
            }
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                return;
            }

            var color = shape switch
            {
                GeometryShapeKind.Rectangle => new Color(0.15f, 0.75f, 1.00f),
                GeometryShapeKind.Triangle => new Color(1.00f, 0.55f, 0.15f),
                GeometryShapeKind.Circle => new Color(0.60f, 0.25f, 1.00f),
                GeometryShapeKind.Cube => new Color(0.20f, 1.00f, 0.45f),
                GeometryShapeKind.Cuboid => new Color(1.00f, 0.85f, 0.20f),
                GeometryShapeKind.Cylinder => new Color(0.95f, 0.25f, 0.40f),
                GeometryShapeKind.Sphere => new Color(0.25f, 0.65f, 1.00f),
                _ => Color.white
            };

            var mr = go.GetComponentInChildren<MeshRenderer>();
            if (mr == null)
            {
                return;
            }

            var mat = new Material(shader);
            mat.color = color;
            mat.SetFloat("_Glossiness", 0.65f);
            
            // Generate Grid Texture
            mat.mainTexture = GenerateGridTexture(color);
            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mr.receiveShadows = true;
        }

        private static Texture2D GenerateGridTexture(Color baseColor)
        {
            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var colors = new Color[size * size];
            var gridColor = new Color(baseColor.r * 0.7f, baseColor.g * 0.7f, baseColor.b * 0.7f);
            var mainColor = baseColor;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool edge = (x % 32 == 0) || (y % 32 == 0) || x == 0 || y == 0 || x == size - 1 || y == size - 1;
                    // Usar cor do grid (mais escura) ao invés de branco para evitar linhas vermelhas
                    colors[y * size + x] = edge ? gridColor : mainColor;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static GameObject CreateCube(float a)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Cube";
            go.transform.localScale = new Vector3(a, a, a);
            return go;
        }

        private static GameObject CreateCuboid(float a, float b, float c)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Cuboid";
            go.transform.localScale = new Vector3(a, c, b);
            return go;
        }

        private static GameObject CreateCylinder(float r, float h)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Cylinder";
            go.transform.localScale = new Vector3(r * 2f, h * 0.5f, r * 2f);
            return go;
        }

        private static GameObject CreateSphere(float r)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Sphere";
            go.transform.localScale = new Vector3(r * 2f, r * 2f, r * 2f);
            return go;
        }

        private static GameObject CreateRectangle(float a, float b)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Rectangle";
            go.transform.localRotation = Quaternion.identity;

            // Increased thickness from 0.02f to 0.05f for better 3D visibility
            go.transform.localScale = new Vector3(a, 0.05f, b);
            return go;
        }

        private static GameObject CreateDisk(float r)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Circle";

            // Increased thickness from 0.005f to 0.02f
            go.transform.localScale = new Vector3(r * 2f, 0.02f, r * 2f);
            return go;
        }

        private static GameObject CreateTriangle(float a, float b, float c)
        {
            var go = new GameObject("Triangle");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();

            var mesh = new Mesh();

            var ax = 0f;
            var az = 0f;
            var bx = a;
            var bz = 0f;

            var cx = (c * c + a * a - b * b) / (2f * a);
            var under = c * c - cx * cx;
            var cz = under <= 0 ? 0 : Mathf.Sqrt(under);

            // Increased thickness from 0.02f to 0.05f
            var thickness = 0.05f;
            var v0 = new Vector3(ax, 0, az);
            var v1 = new Vector3(bx, 0, bz);
            var v2 = new Vector3(cx, 0, cz);
            var v3 = v0 + Vector3.up * thickness;
            var v4 = v1 + Vector3.up * thickness;
            var v5 = v2 + Vector3.up * thickness;

            mesh.vertices = new[] { v0, v1, v2, v3, v4, v5 };
            mesh.triangles = new[]
            {
                0, 2, 1,
                3, 4, 5,
                0, 1, 4, 0, 4, 3,
                1, 2, 5, 1, 5, 4,
                2, 0, 3, 2, 3, 5
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            go.transform.localRotation = Quaternion.identity;
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            // convex helps physics interactions on simple shapes
            mc.convex = true;
            return go;
        }
    }
}
