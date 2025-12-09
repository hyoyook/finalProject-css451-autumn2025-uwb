Shader "Unlit/451Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        MyColor ("My Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off  // Render both sides

        Pass
        {
            CGPROGRAM
            #pragma vertex MyVert
            #pragma fragment MyFrag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;    // REQUIRED: We need normals for lighting
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1; // Pass normal to fragment
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD3; // We need world position for spotlight math
            };

            // --- CUSTOM VARIABLES ---
            float4x4 MyXformMat;  // Your custom transform matrix
            fixed4   MyColor;

            // --- LIGHTING VARIABLES (From LightControl.cs) ---
            float4 LightColor;
            float3 SlightPos;      // Light Position
            float3 LightDirection; // Direction
            float _MinTheta;
            float _MaxTheta;

            // Texture support
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f MyVert (appdata v)
            {
                v2f o;

                // 1. Apply your Custom Matrix to the Vertex
                float4 worldV = mul(MyXformMat, v.vertex); 
                o.worldPos = worldV.xyz; // Save the World Position for lighting math

                // 2. Apply Custom Matrix to the Normal (Rotate the normal)
                // We cast to float3x3 to rotate/scale but ignore translation
                o.normal = mul((float3x3)MyXformMat, v.normal);

                // 3. Transform to Camera/Clip space
                o.vertex = mul(UNITY_MATRIX_VP, worldV); 
                
                // 4. Texture
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // --- LIGHTING MATH (Same as Illumination.shader) ---
            float ComputeDiffuse(v2f i) {
                float3 l = SlightPos - i.worldPos; // Vector from Surface to Light
                float d = length(l);
                
                if(d < 0.0001) d = 0.0001;
                l = l / d; // Normalize

                float strength = 0;

                // A. Dot Product (Angle of light)
                float dotProd = dot(l, LightDirection);
                dotProd = clamp(dotProd, -1.0, 1.0);

                // B. Spotlight Cone Math
                float alpha = acos(dotProd);
                float ndotl = clamp(dot(normalize(i.normal), l), 0, 1);
                
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
            
            fixed4 MyFrag (v2f i) : SV_Target
            {
                // 1. Sample Texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 2. Calculate Light Strength
                float diff = ComputeDiffuse(i);

                // 3. Combine: Texture * Tint * Lighting Strength * Light Color
                return col * MyColor * diff * LightColor;
            }
            ENDCG
        }
    }
}