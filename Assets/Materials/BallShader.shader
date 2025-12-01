Shader "Custom/BallShaderWithSmoothness"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _UseTexture ("Use Texture (0/1)", Float) = 1

        // --- NEW SMOOTHNESS PROPERTIES ---
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(1, 500)) = 50.0 
        // ---------------------------------

        _Ambient ("Ambient Intensity", Range(0, 1)) = 0.15

        // Optional UI - tweakable point - light settings
        _PointLightColor ("Point Light Color", Color) = (1, 1, 1, 1)
        _PointLightIntensity ("Point Light Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        Cull Off // no culling, must handle two - sided in the frag

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // Shader properties
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _UseTexture;
            float _Ambient;

            // Specular properties
            float4 _SpecularColor;
            float _Smoothness;

            // Point light controls
            float4 _PointLightColor;
            float _PointLightIntensity;

            // Global uniforms
            float _EnableDirLight;
            float _EnablePointLight;
            float4 _LightPosition;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3; // We need this to calculate reflections
            };

            v2f vert (appdata v)
            {
                v2f o;
                // Vertex position in object space to clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                // Apply texture tiling and offset
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Convert to world space
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                // Calculate direction from the vertex to the Camera
                o.viewDir = WorldSpaceViewDir(v.vertex);

                return o;
            }

            // VFACE tells us if we're rendering front of back face.
            fixed4 frag (v2f i, float face : VFACE) : SV_Target
            {
                // 1. SETUP VECTORS
                // flip normal for back faces
                float3 n = normalize(i.worldNormal);
                if (face < 0) n = -n; 

                // Normalize view direction (Camera to pixel)
                float3 v = normalize(i.viewDir);

                // -- - AMBIENT LIGHTING -- -
                float3 ambient = _Ambient * _Color.rgb;

                // Variables to accumulate light
                float3 totalDiffuse = float3(0,0,0);
                float3 totalSpecular = float3(0,0,0);

                // -- - UNITY DIRECTIONAL LIGHT (Sun) -- -
                if (_EnableDirLight > 0.5)
                {
                    float3 Ld;
                    if (_WorldSpaceLightPos0.w == 0)
                        Ld = normalize(_WorldSpaceLightPos0.xyz);
                    else
                        Ld = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);

                    // Diffuse (Lambert)
                    float ndotl = saturate(dot(n, Ld));
                    totalDiffuse += _LightColor0.rgb * ndotl;

                    // Specular (Blinn-Phong)
                    // Half vector between Light and View
                    float3 h = normalize(Ld + v); 
                    float NdotH = saturate(dot(n, h));
                    float spec = pow(NdotH, _Smoothness);
                    
                    // Only apply specular if the light is hitting the face (ndotl > 0)
                    if (ndotl > 0)
                        totalSpecular += _SpecularColor.rgb * spec * _LightColor0.rgb;
                }

                // -- - CUSTOM POINT LIGHT -- -
                if (_EnablePointLight > 0.5)
                {
                    float3 toPoint = _LightPosition.xyz - i.worldPos;
                    float dist = length(toPoint);

                    if (dist > 1e-4)
                    {
                        float3 Lp = toPoint / dist; // normalized light dir
                        
                        // Attenuation
                        float atten = 1.0 / (1.0 + 0.1 * dist + 0.02 * dist * dist);

                        // Diffuse
                        float ndot = saturate(dot(n, Lp));
                        float3 pointDiff = _PointLightColor.rgb * ndot * atten * _PointLightIntensity;
                        totalDiffuse += pointDiff;

                        // Specular
                        float3 h = normalize(Lp + v);
                        float NdotH = saturate(dot(n, h));
                        float spec = pow(NdotH, _Smoothness);

                        if (ndot > 0)
                        {
                            totalSpecular += _SpecularColor.rgb * spec * atten * _PointLightIntensity;
                        }
                    }
                }

                // -- - BASE COLOR -- -
                fixed4 texColor;
                if (_UseTexture < 0.5)
                    texColor = _Color;
                else
                    texColor = tex2D(_MainTex, i.uv) * _Color;

                // -- - FINAL COMBINE -- -
                // Standard formula: Texture * (Ambient + Diffuse) + Specular
                // Specular is usually added on TOP of the texture (makes it look shiny/plastic/metallic)
                
                float3 finalColor = texColor.rgb * (ambient + totalDiffuse) + totalSpecular;

                return fixed4(finalColor, texColor.a);
            }
            ENDCG
        }
    }
    Fallback Off
}