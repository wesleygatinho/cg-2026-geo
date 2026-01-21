using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARGeometryGame.AR
{
    /// <summary>
    /// Aplica material transparente com malha aos planos AR detectados
    /// </summary>
    public sealed class ARPlaneMaterialController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Material transparentGridMaterial;

        private void Awake()
        {
            if (planeManager == null)
            {
                planeManager = FindAnyObjectByType<ARPlaneManager>();
            }

            if (planeManager != null)
            {
                planeManager.planesChanged += OnPlanesChanged;
                ApplyMaterialToExistingPlanes();
            }

            if (transparentGridMaterial == null)
            {
                CreateTransparentGridMaterial();
            }
        }

        private void OnDestroy()
        {
            if (planeManager != null)
            {
                planeManager.planesChanged -= OnPlanesChanged;
            }
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            foreach (var plane in args.added)
            {
                ApplyMaterialToPlane(plane);
            }
        }

        private void ApplyMaterialToExistingPlanes()
        {
            if (planeManager == null) return;

            foreach (var plane in planeManager.trackables)
            {
                ApplyMaterialToPlane(plane);
            }
        }

        private void ApplyMaterialToPlane(ARPlane plane)
        {
            if (plane == null || transparentGridMaterial == null) return;

            var meshRenderer = plane.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = transparentGridMaterial;
            }
        }

        private void CreateTransparentGridMaterial()
        {
            // Criar material transparente com textura de malha
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            }

            if (shader == null)
            {
                Debug.LogWarning("Não foi possível encontrar shader apropriado para material transparente");
                return;
            }

            transparentGridMaterial = new Material(shader);
            
            // Configurar para modo transparente
            transparentGridMaterial.SetFloat("_Mode", 3f); // Transparent mode
            transparentGridMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentGridMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentGridMaterial.SetInt("_ZWrite", 0);
            transparentGridMaterial.DisableKeyword("_ALPHATEST_ON");
            transparentGridMaterial.EnableKeyword("_ALPHABLEND_ON");
            transparentGridMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentGridMaterial.renderQueue = 3000;

            // Cor cinza transparente com malha
            var gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Cinza transparente
            transparentGridMaterial.color = gridColor;
            
            // Criar textura de malha
            var gridTexture = CreateGridTexture();
            transparentGridMaterial.mainTexture = gridTexture;
        }

        private Texture2D CreateGridTexture()
        {
            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var colors = new Color[size * size];
            
            var gridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // Linhas da malha
            var transparent = new Color(1f, 1f, 1f, 0f); // Fundo transparente

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Criar padrão de malha
                    bool isGridLine = (x % 16 == 0) || (y % 16 == 0);
                    colors[y * size + x] = isGridLine ? gridColor : transparent;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }
    }
}
