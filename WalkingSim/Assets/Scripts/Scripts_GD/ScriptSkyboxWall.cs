using UnityEngine;

public class ScriptSkyboxWall : MonoBehaviour
{
   public Camera skyboxCamera;
   public RenderTexture skyboxRender;
   public Material wallMaterial;
    void Start()
    {
        if (skyboxCamera != null && skyboxRender != null && wallMaterial != null)
        {
            // Assigne le RenderTexture à la caméra
            skyboxCamera.targetTexture = skyboxRender;

            // Assigne le RenderTexture au material du mur
            wallMaterial.mainTexture = skyboxRender;
        }
    }
}
