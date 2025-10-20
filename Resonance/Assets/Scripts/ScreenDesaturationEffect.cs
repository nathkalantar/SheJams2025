using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ScreenDesaturationEffect : MonoBehaviour
{
    [Header("Shader")]
    public Shader desaturationShader;
    private Material desaturationMaterial;
    
    [Header("Global Settings")]
    [Range(0f, 1f)]
    [Tooltip("0 = Todo en color, 1 = Todo en B&N (esferas devuelven el color)")]
    public float globalDesaturation = 1f;
    
    [Header("Info")]
    [Tooltip("Las esferas se registran automáticamente desde ColorSphere scripts")]
    public int registeredSpheresCount = 0;
    
    private List<ColorSphere> registeredSpheres = new List<ColorSphere>();
    private Vector4[] sphereData = new Vector4[10];
    private float[] sphereFades = new float[10];
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
        
        // Crear material si no existe
        if (desaturationShader == null)
        {
            desaturationShader = Shader.Find("Custom/ScreenDesaturation");
        }
        
        if (desaturationShader != null)
        {
            desaturationMaterial = new Material(desaturationShader);
        }
    }

    private void Update()
    {
        registeredSpheresCount = registeredSpheres.Count;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (desaturationMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // Calcular rayos del frustum para reconstruir posición del mundo
        SetFrustumRays();
        
        // Actualizar datos de las esferas registradas
        int count = Mathf.Min(registeredSpheres.Count, 10);
        
        for (int i = 0; i < count; i++)
        {
            if (registeredSpheres[i] != null && registeredSpheres[i].transform != null)
            {
                Vector3 pos = registeredSpheres[i].transform.position;
                
                sphereData[i] = new Vector4(
                    pos.x, 
                    pos.y, 
                    pos.z, 
                    registeredSpheres[i].GetCurrentRadius()
                );
                
                sphereFades[i] = registeredSpheres[i].fadeWidth;
            }
        }
        
        // Pasar datos al shader
        desaturationMaterial.SetVectorArray("_ColorSpheres", sphereData);
        desaturationMaterial.SetFloatArray("_ColorSphereFades", sphereFades);
        desaturationMaterial.SetInt("_ColorSphereCount", count);
        desaturationMaterial.SetFloat("_GlobalDesaturation", globalDesaturation);
        
        // Aplicar el efecto
        Graphics.Blit(source, destination, desaturationMaterial);
    }
    
    private void SetFrustumRays()
    {
        float fovWHalf = cam.fieldOfView * 0.5f;
        
        Vector3 toRight = cam.transform.right * cam.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * cam.aspect;
        Vector3 toTop = cam.transform.up * cam.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
        
        Vector3 topLeft = (cam.transform.forward * cam.nearClipPlane - toRight + toTop);
        float camScale = topLeft.magnitude * cam.farClipPlane / cam.nearClipPlane;
        
        topLeft.Normalize();
        topLeft *= camScale;
        
        Vector3 topRight = (cam.transform.forward * cam.nearClipPlane + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;
        
        Vector3 bottomRight = (cam.transform.forward * cam.nearClipPlane + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;
        
        Vector3 bottomLeft = (cam.transform.forward * cam.nearClipPlane - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;
        
        Matrix4x4 frustumCorners = Matrix4x4.identity;
        frustumCorners.SetRow(0, bottomLeft);
        frustumCorners.SetRow(1, bottomRight);
        frustumCorners.SetRow(2, topRight);
        frustumCorners.SetRow(3, topLeft);
        
        desaturationMaterial.SetMatrix("_FrustumCornersRay", frustumCorners);
    }
    
    private void OnDestroy()
    {
        if (desaturationMaterial != null)
        {
            DestroyImmediate(desaturationMaterial);
        }
    }

    // Métodos públicos para control desde otros scripts
    public void SetGlobalDesaturation(float value)
    {
        globalDesaturation = Mathf.Clamp01(value);
    }

    public void RegisterColorSphere(ColorSphere sphere)
    {
        if (!registeredSpheres.Contains(sphere))
        {
            registeredSpheres.Add(sphere);
        }
    }

    public void UnregisterColorSphere(ColorSphere sphere)
    {
        registeredSpheres.Remove(sphere);
    }

    public void ClearColorSpheres()
    {
        registeredSpheres.Clear();
    }
}
