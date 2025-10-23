Shader "Custom/AnimatedSky"
{
    Properties
    {
        [Header(Sky Colors)]
        _SkyColorTop ("Sky Color Top", Color) = (0.5, 0.8, 1.0, 1.0)
        _SkyColorBottom ("Sky Color Bottom", Color) = (0.8, 0.9, 1.0, 1.0)
        _HorizonLine ("Horizon Line", Range(0, 1)) = 0.3
        _HorizonSoftness ("Horizon Softness", Range(0.01, 1)) = 0.1
        
        [Header(Cloud Settings)]
        _CloudColor ("Cloud Color", Color) = (1.0, 1.0, 1.0, 0.8)
        _CloudShadowColor ("Cloud Shadow Color", Color) = (0.6, 0.6, 0.7, 1.0)
        _CloudCoverage ("Cloud Coverage", Range(0, 1)) = 0.5
        _CloudDensity ("Cloud Density", Range(0.1, 2)) = 1.0
        _CloudSharpness ("Cloud Sharpness", Range(0.1, 5)) = 2.0
        
        [Header(Animation)]
        _CloudSpeedX ("Cloud Speed X", Range(-2, 2)) = 0.5
        _CloudSpeedY ("Cloud Speed Y", Range(-2, 2)) = 0.1
        _WindTurbulence ("Wind Turbulence", Range(0, 2)) = 0.3
        
        [Header(Noise Settings)]
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _NoiseOctaves ("Noise Octaves", Range(1, 4)) = 3
        _NoisePersistence ("Noise Persistence", Range(0.1, 1)) = 0.5
        
        [Header(Advanced)]
        _SunDirection ("Sun Direction", Vector) = (0.3, 0.7, 0.6, 0)
        _SunInfluence ("Sun Influence on Clouds", Range(0, 2)) = 1.0
        _AtmosphericPerspective ("Atmospheric Perspective", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Background"
            "PreviewType"="Plane"
        }
        
        LOD 200
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"

            // Properties
            fixed4 _SkyColorTop;
            fixed4 _SkyColorBottom;
            float _HorizonLine;
            float _HorizonSoftness;
            
            fixed4 _CloudColor;
            fixed4 _CloudShadowColor;
            float _CloudCoverage;
            float _CloudDensity;
            float _CloudSharpness;
            
            float _CloudSpeedX;
            float _CloudSpeedY;
            float _WindTurbulence;
            
            float _NoiseScale;
            int _NoiseOctaves;
            float _NoisePersistence;
            
            float4 _SunDirection;
            float _SunInfluence;
            float _AtmosphericPerspective;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            // Función de ruido simple (Pseudo-random)
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Ruido suave interpolado
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                // Suavizado hermite
                f = f * f * (3.0 - 2.0 * f);
                
                // Interpolación bilinear de los 4 puntos
                return lerp(
                    lerp(hash(i + float2(0, 0)), hash(i + float2(1, 0)), f.x),
                    lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x),
                    f.y
                );
            }

            // Ruido fractal (FBM - Fractional Brownian Motion)
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;
                
                for (int i = 0; i < _NoiseOctaves; i++)
                {
                    value += amplitude * noise(p * frequency);
                    amplitude *= _NoisePersistence;
                    frequency *= 2.0;
                }
                
                return value;
            }

            // Función para generar nubes
            float generateClouds(float2 uv, float time)
            {
                // Movimiento de las nubes
                float2 cloudUV = uv * _NoiseScale;
                cloudUV.x += time * _CloudSpeedX;
                cloudUV.y += time * _CloudSpeedY;
                
                // Turbulencia adicional
                float2 turbulence = float2(
                    fbm(cloudUV * 0.5 + time * 0.1) * _WindTurbulence,
                    fbm(cloudUV * 0.7 + time * 0.15) * _WindTurbulence
                );
                
                cloudUV += turbulence;
                
                // Generar patrón de nubes principal
                float cloudPattern = fbm(cloudUV);
                
                // Agregar detalle adicional
                float detailNoise = fbm(cloudUV * 3.0) * 0.3;
                cloudPattern += detailNoise;
                
                // Aplicar cobertura y densidad
                cloudPattern = saturate((cloudPattern - (1.0 - _CloudCoverage)) * _CloudDensity);
                
                // Suavizar bordes
                cloudPattern = pow(cloudPattern, _CloudSharpness);
                
                return cloudPattern;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y;
                
                // === GRADIENTE DEL CIELO ===
                float skyGradient = smoothstep(_HorizonLine - _HorizonSoftness, _HorizonLine + _HorizonSoftness, uv.y);
                fixed4 skyColor = lerp(_SkyColorBottom, _SkyColorTop, skyGradient);
                
                // === GENERAR NUBES ===
                float cloudMask = generateClouds(uv, time);
                
                // === ILUMINACIÓN DE NUBES ===
                // Simular dirección del sol
                float3 sunDir = normalize(_SunDirection.xyz);
                
                // Calcular "altura" de la nube basada en la posición Y
                float cloudHeight = uv.y;
                
                // Iluminación direccional simple
                float sunDot = dot(float3(uv.x - 0.5, cloudHeight, 0.5), sunDir);
                float lighting = saturate(sunDot * _SunInfluence + 0.5);
                
                // Color de nube con iluminación
                fixed4 litCloudColor = lerp(_CloudShadowColor, _CloudColor, lighting);
                
                // === PERSPECTIVA ATMOSFÉRICA ===
                // Las nubes más alejadas se ven más azuladas
                float distance = length(i.worldPos);
                float atmosphericFade = saturate(distance * _AtmosphericPerspective * 0.01);
                litCloudColor = lerp(litCloudColor, skyColor, atmosphericFade * 0.3);
                
                // === COMPOSICIÓN FINAL ===
                // Mezclar cielo y nubes
                fixed4 finalColor = lerp(skyColor, litCloudColor, cloudMask * litCloudColor.a);
                
                // Asegurar alpha
                finalColor.a = 1.0;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Unlit/Color"
}