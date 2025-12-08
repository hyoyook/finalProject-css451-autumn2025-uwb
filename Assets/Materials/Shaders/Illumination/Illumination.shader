Shader "Custom/Illumination"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MinTheta("Min Theta", Range(0,1.5)) = 0
        _MaxTheta("Max Theta", Range(0,1.5)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 vertexWC : TEXCOORD3;
            };

            sampler2D _MainTex;

            // --- VARIABLES ---
            // UPDATED: Using float4 everywhere to prevent "1.0" clamping
            float4 LightPosition;
            float4 LightColor;  
            float  LightNear;
            float  LightFar;

            float3 LightDirection;
            float3 SlightPos;
            float _MinTheta;
            float _MaxTheta;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 

                o.vertexWC = mul(UNITY_MATRIX_M, v.vertex);

                // Normal calculation
                float3 p = v.vertex + 10 * v.normal;
                p = mul(UNITY_MATRIX_M, float4(p,1)); 
                o.normal = normalize(p - o.vertexWC);
                
                return o;
            }
            
            float ComputeDiffuse(v2f i) {
                float3 l = SlightPos - i.vertexWC;
                float d = length(l);
                
                if(d < 0.0001) d = 0.0001;

                l = l / d;
                float strength = 0;

                // 1. Calculate Dot Product
                float dotProd = dot(l, LightDirection);
                
                // 2. Safety Clamp
                dotProd = clamp(dotProd, -1.0, 1.0);

                // 3. Convert to Angle
                float alpha = acos(dotProd);
                
                float ndotl = clamp(dot(i.normal, l), 0, 1);
                
                if (alpha < _MaxTheta) {
                    if (alpha > _MinTheta) {
                        float range = _MaxTheta - _MinTheta;
                        float n = _MinTheta - alpha;
                        strength = smoothstep(1, 0, (n*n) / (range*range));
                    }
                    else {
                        strength = 1;
                    }
                } 
                return ndotl * strength;
            }

            // UPDATED: Return 'float4' instead of 'fixed4' to allow brightness > 1
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float diff = ComputeDiffuse(i);
                
                // Multiply texture * diffuse * LightColor
                return col * diff * LightColor;
            }

            ENDCG
        }
    }
}