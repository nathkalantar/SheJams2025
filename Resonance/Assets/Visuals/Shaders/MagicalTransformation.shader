Shader "Custom/MagicalTransformation"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TransformationProgress ("Transformation Progress", Range(0,1)) = 0
        
        [Header(Magical Effects)]
        _MagicColor ("Magic Color", Color) = (1, 0.5, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
        _SparkleFrequency ("Sparkle Frequency", Range(0, 20)) = 10
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(0, 10)) = 5
        
        [Header(Transformation)]
        _TransformSprite ("Transform To Sprite", 2D) = "white" {}
        _TransformColor ("Transform To Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 3)) = 1
        
        [Header(Sprite Settings)]
        [Toggle(PIXELSNAP_ON)] _PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _TransformSprite;
            sampler2D _AlphaTex;
            fixed4 _Color;
            fixed4 _RendererColor;
            float _TransformationProgress;
            fixed4 _MagicColor;
            float _GlowIntensity;
            float _SparkleFrequency;
            float _WaveAmplitude;
            float _WaveFrequency;
            fixed4 _TransformColor;
            float _EmissionIntensity;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                // Efecto de ondas durante la transformación (más sutil para sprites)
                float waveEffect = sin(_Time.y * _WaveFrequency + IN.vertex.y * 50) * _WaveAmplitude * _TransformationProgress;
                IN.vertex.x += waveEffect;
                
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 SampleSpriteTexture(float2 uv)
            {
                // Obtener colores de ambos sprites
                fixed4 originalColor = tex2D(_MainTex, uv);
                fixed4 transformColor = tex2D(_TransformSprite, uv);
                
                // Si el sprite original no tiene alpha en esta posición, no renderizar nada
                if (originalColor.a < 0.01)
                    return fixed4(0, 0, 0, 0);
                
                // Si no hay sprite de transformación, usar solo el original
                if (transformColor.a < 0.01)
                    transformColor = originalColor;
                
                // Interpolar entre sprites manteniendo la forma del original
                fixed4 color = lerp(originalColor, transformColor, _TransformationProgress);
                
                // Usar el alpha del sprite original para mantener la forma
                color.a = originalColor.a;

                #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, uv);
                color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                #endif

                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Obtener color del sprite
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                
                // Solo aplicar efectos donde hay píxeles del sprite (alpha > 0)
                if (c.a > 0.01)
                {
                    // Efecto de chispas/sparkles
                    float2 sparkleUV = IN.texcoord * _SparkleFrequency;
                    float sparkle1 = sin(sparkleUV.x * 50 + _Time.y * 8) * cos(sparkleUV.y * 30 + _Time.y * 6);
                    float sparkle2 = sin(sparkleUV.x * 30 + _Time.y * 10) * cos(sparkleUV.y * 40 + _Time.y * 4);
                    float sparklePattern = (sparkle1 + sparkle2) * 0.5;
                    sparklePattern = smoothstep(0.8, 1.0, sparklePattern);
                    
                    // Brillo mágico basado en la posición
                    float2 center = float2(0.5, 0.5);
                    float distFromCenter = distance(IN.texcoord, center);
                    float edgeGlow = 1.0 - smoothstep(0.3, 0.5, distFromCenter);
                    float magicGlow = edgeGlow * _GlowIntensity * _TransformationProgress;
                    magicGlow += sparklePattern * _TransformationProgress * 0.3;
                    
                    // Ondas de energía que suben
                    float energyWave = sin(IN.texcoord.y * 15 - _Time.y * 20) * 0.5 + 0.5;
                    energyWave = smoothstep(0.4, 0.8, energyWave) * _TransformationProgress;
                    
                    // Color final interpolado entre original y transformado
                    fixed4 finalColor = lerp(c, _TransformColor * c.a, _TransformationProgress * 0.6);
                    
                    // Agregar efectos mágicos
                    fixed4 magicEffect = _MagicColor * (magicGlow + energyWave);
                    finalColor.rgb += magicEffect.rgb * _TransformationProgress * c.a;
                    
                    // Emission final
                    finalColor.rgb += finalColor.rgb * _EmissionIntensity * _TransformationProgress;
                    
                    // Pulso de luz al final de la transformación
                    if (_TransformationProgress > 0.9)
                    {
                        float finalBurst = sin(_Time.y * 25) * 0.3 + 0.3;
                        finalColor.rgb += _MagicColor.rgb * finalBurst * (_TransformationProgress - 0.9) * 20;
                    }
                    
                    // Mantener el alpha original del sprite
                    finalColor.a = c.a;
                    
                    return finalColor;
                }
                
                return c;
            }
            ENDCG
        }
    }
}