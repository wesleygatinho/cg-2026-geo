using UnityEngine;

namespace ARGeometryGame.AR
{
    /// <summary>
    /// Desabilita as linhas visuais do XR Interaction Toolkit (linha vermelha na tela)
    /// </summary>
    public sealed class XRLineDisabler : MonoBehaviour
    {
        private void Start()
        {
            DisableAllXRInteractionLines();
        }

        private void DisableAllXRInteractionLines()
        {
            // Encontrar todos os LineRenderers que são parte do XR Interaction Toolkit
            var lineRenderers = FindObjectsByType<LineRenderer>(FindObjectsSortMode.None);
            
            foreach (var lineRenderer in lineRenderers)
            {
                // Verificar se está em um GameObject que parece ser do XR Interaction Toolkit
                // Geralmente esses objetos têm componentes XR relacionados ou estão em hierarquia específica
                var parentName = lineRenderer.transform.parent != null 
                    ? lineRenderer.transform.parent.name 
                    : lineRenderer.transform.name;
                
                // Desabilitar linhas que parecem ser do XR Interaction Toolkit
                // (geralmente são as linhas de interação que aparecem vermelhas)
                if (parentName.Contains("XR") || parentName.Contains("Interactor") || 
                    parentName.Contains("Ray") || parentName.Contains("Line"))
                {
                    lineRenderer.enabled = false;
                }
            }

            // Também tentar desabilitar via componentes MonoBehaviour que controlam linhas
            // Isso é mais robusto pois não depende de nomes
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var behaviour in behaviours)
            {
                var typeName = behaviour.GetType().Name;
                // XRInteractorLineVisual é o componente do XR Interaction Toolkit que controla linhas
                if (typeName.Contains("InteractorLineVisual") || typeName.Contains("LineVisual"))
                {
                    behaviour.enabled = false;
                    
                    // Também desabilitar o LineRenderer filho se existir
                    var lineRenderer = behaviour.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        lineRenderer.enabled = false;
                    }
                }
            }
        }
    }
}
