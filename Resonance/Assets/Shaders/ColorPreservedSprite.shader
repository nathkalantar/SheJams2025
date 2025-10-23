Shader "Custom/ColorPreservedSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] [NonModifiableTextureData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
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

        // Pass normal para que funcione con sorting
        Pass
        {
            Name "SpriteBase"
            
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            ENDCG
        }
        
        // Pass adicional para color preservation - se ejecuta después
        Pass
        {
            Name "ColorOverlay"
            Tags { "LightMode" = "Always" }
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFragPreserved
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 SpriteFragPreserved(v2f IN) : SV_Target
            {
                // Obtener el color del sprite con preservación
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                c.rgb *= _Color.rgb;
                c.rgb *= c.a;
                
                return c;
            }
            ENDCG
        }
    }
    
    // Fallback al shader sprite por defecto
    Fallback "Sprites/Default"
}