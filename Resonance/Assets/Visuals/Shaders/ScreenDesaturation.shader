Shader "Custom/ScreenDesaturation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float3 ray : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _MainTex_ST;
            float4x4 _FrustumCornersRay;
            
            // Variables globales para las esferas de color
            uniform float4 _ColorSpheres[10]; // xyz = posición, w = radio
            uniform float _ColorSphereFades[10]; // Fade width individual por esfera
            uniform int _ColorSphereCount;
            uniform float _GlobalDesaturation; // 0 = color completo, 1 = B&N completo

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                // Calcular el rayo desde la cámara
                int index = 0;
                if (v.uv.x < 0.5 && v.uv.y < 0.5) index = 0; // Bottom Left
                else if (v.uv.x > 0.5 && v.uv.y < 0.5) index = 1; // Bottom Right
                else if (v.uv.x > 0.5 && v.uv.y > 0.5) index = 2; // Top Right
                else index = 3; // Top Left
                
                o.ray = _FrustumCornersRay[index].xyz;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Obtener el color original de la pantalla
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Dirección del rayo normalizada
                float3 rayDir = normalize(i.ray);
                
                // Empezar con desaturación según el valor global
                float colorFactor = 1.0 - _GlobalDesaturation; // 0 = B&N, 1 = color
                
                // Revisar cada esfera de color
                for (int s = 0; s < _ColorSphereCount; s++)
                {
                    float3 sphereCenter = _ColorSpheres[s].xyz;
                    float sphereRadius = _ColorSpheres[s].w;
                    float fadeWidth = _ColorSphereFades[s];
                    
                    // Calcular intersección rayo-esfera
                    float3 oc = _WorldSpaceCameraPos - sphereCenter;
                    float a = dot(rayDir, rayDir);
                    float b = dot(oc, rayDir);
                    float c = dot(oc, oc) - sphereRadius * sphereRadius;
                    float discriminant = b * b - a * c;
                    
                    // Si el rayo atraviesa la esfera, dar color al píxel
                    if (discriminant >= 0.0)
                    {
                        // Calcular la distancia mínima del rayo al centro de la esfera
                        float t = -b / a;
                        float3 closestPoint = _WorldSpaceCameraPos + t * rayDir;
                        float distToCenter = distance(closestPoint, sphereCenter);
                        
                        // Aplicar color con transición suave
                        float factor = 1.0 - smoothstep(
                            sphereRadius - fadeWidth,
                            sphereRadius,
                            distToCenter
                        );
                        
                        colorFactor = max(colorFactor, factor);
                    }
                }
                
                // Convertir a blanco y negro usando pesos perceptuales
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                
                // Interpolar: colorFactor = 0 es B&N, colorFactor = 1 es color
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, colorFactor);
                
                return col;
            }
            ENDCG
        }
    }
}
