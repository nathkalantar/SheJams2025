using UnityEngine;

[RequireComponent(typeof(ColorSphere))]
public class ColorSphereVisual : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Mostrar una esfera visual transparente")]
    public bool showVisual = true;
    
    [Tooltip("Color de la esfera visual")]
    public Color sphereColor = new Color(1f, 0.5f, 0f, 0.3f);
    
    [Range(0.5f, 8f)]
    public float rimPower = 3f;
    
    [Range(0f, 2f)]
    public float rimIntensity = 1f;
    
    [Header("Auto Setup")]
    public bool autoCreateMesh = true;
    
    private GameObject visualMesh;
    private Material visualMaterial;
    private ColorSphere colorSphere;

    private void Start()
    {
        colorSphere = GetComponent<ColorSphere>();
        
        if (autoCreateMesh && showVisual)
        {
            CreateVisualMesh();
        }
    }

    private void Update()
    {
        if (visualMesh != null && colorSphere != null)
        {
            // Actualizar el tamaño de la esfera visual según el radio actual
            float currentRadius = colorSphere.GetCurrentRadius();
            visualMesh.transform.localScale = Vector3.one * currentRadius * 2f;
        }
        
        if (visualMaterial != null)
        {
            visualMaterial.SetColor("_Color", sphereColor);
            visualMaterial.SetFloat("_RimPower", rimPower);
            visualMaterial.SetFloat("_RimIntensity", rimIntensity);
        }
    }

    private void CreateVisualMesh()
    {
        // Crear una esfera como hijo
        visualMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualMesh.name = "ColorSphereVisual";
        visualMesh.transform.SetParent(transform);
        visualMesh.transform.localPosition = Vector3.zero;
        visualMesh.transform.localRotation = Quaternion.identity;
        
        // Remover el collider (no lo necesitamos)
        Collider col = visualMesh.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
        
        // Crear material con el shader
        Shader shader = Shader.Find("Custom/ColorSphereVisual");
        if (shader != null)
        {
            visualMaterial = new Material(shader);
            visualMesh.GetComponent<Renderer>().material = visualMaterial;
        }
        
        // Configurar tamaño inicial
        if (colorSphere != null)
        {
            visualMesh.transform.localScale = Vector3.one * colorSphere.radius * 2f;
        }
    }

    private void OnDestroy()
    {
        if (visualMesh != null)
        {
            Destroy(visualMesh);
        }
        
        if (visualMaterial != null)
        {
            Destroy(visualMaterial);
        }
    }

    public void SetVisualActive(bool active)
    {
        showVisual = active;
        if (visualMesh != null)
        {
            visualMesh.SetActive(active);
        }
        else if (active && autoCreateMesh)
        {
            CreateVisualMesh();
        }
    }

    public void SetColor(Color color)
    {
        sphereColor = color;
    }
}
