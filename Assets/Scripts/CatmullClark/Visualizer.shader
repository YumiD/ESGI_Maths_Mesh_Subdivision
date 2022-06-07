Shader "Custom/Visualizer"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            uint hash(uint s)
            {
                s ^= 2747636419u;
                s *= 2654435769u;
                s ^= s >> 16;
                s *= 2654435769u;
                s ^= s >> 16;
                s *= 2654435769u;
                return s;
            }

            float random(uint seed)
            {
                return float(hash(seed)) / 4294967295.0; // 2^32-1
            }

            fixed4 frag (uint triangleID: SV_PrimitiveID) : SV_Target
            {
                return fixed4(random(triangleID), random(triangleID * triangleID), random(triangleID * triangleID * triangleID), 1);
            }
            ENDCG
        }
    }
}
