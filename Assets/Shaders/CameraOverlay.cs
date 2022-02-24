using UnityEngine;

[ExecuteInEditMode]
//[RequireComponent(typeof(Camera))]
public class CameraOverlay : MonoBehaviour
{
    public Shader replacementShader;
    public Color OverDrawColor;

    private void OnValidate()
    {
        Shader.SetGlobalColor("_OverDrawColor", OverDrawColor);
    }

    private void OnEnable()
    {
        if (replacementShader != null)
        {
            GetComponent<Camera>().SetReplacementShader(replacementShader, "");
        }
    }
   /* public void Awake()
    {
        if (replacementShader)
           // transform.camera.SetReplacementShader(shader, null);
        transform.GetComponent<Camera>().SetReplacementShader(replacementShader, "");
    }*/

        /*private void OnRenderImage(RenderTexture source, RenderTexture destination)
         {
             if (replacementShader == null) return;
             Graphics.Blit(source, destination);
         }
     */
        /* void OnDisable()
         {
             if (_material)
             {
                 DestroyImmediate(_material);
             }
         }*/
    }
