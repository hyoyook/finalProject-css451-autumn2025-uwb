﻿// Built with the help of AI
Shader "Custom/451NoCullShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _UseTexture ("Use Texture (0/1)", Float) = 1

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

            // Shader properties passed from material inspector
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _UseTexture;
            float _Ambient;

            // Point light controls
            float4 _PointLightColor;
            float _PointLightIntensity;

            // Global uniforms you control from LightControl.cs
            float _EnableDirLight; // 1 = use Unity main light, 0 = ignore
            float _EnablePointLight; // 1 = use LightPosition, 0 = ignore
            float4 _LightPosition; // world - space position of ALightPosition

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // clip space position for rasterization
                float2 uv : TEXCOORD0; // texture coordinates
                float3 worldPos : TEXCOORD1; // world position for lighting calculations
                float3 worldNormal : TEXCOORD2; // world - space normal for lighting
            };

            v2f vert (appdata v)
            {
                v2f o;
                // Vertex position in object space to clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                // Apply texture tiling and offset
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Convert to world space for lighting calculations in fragment shader
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                return o;
            }

            // VFACE tells us if we're rendering front of back face.
            fixed4 frag (v2f i, float face : VFACE) : SV_Target
            {
                // flip normal for back faces; so we can light either side
                float3 n = normalize(i.worldNormal);
                if (face < 0) n = - n; // face < 0 means back face

                // -- - AMBIENT LIGHTING -- -
                float3 ambient = _Ambient * _Color.rgb; // Ambient light just means light in all directions; didn't know that

                // -- - UNITY DIRECTIONAL LIGHT (the sun) -- -
                float3 dirDiffuse = 0;

                if (_EnableDirLight > 0.5) // Is changed in Source / Lights / LightControl.cs
                {
                    // Get light direction from Unity's built - in directional light
                    float3 Ld;
                    if (_WorldSpaceLightPos0.w == 0) // w = 0 means directional light
                    Ld = normalize(_WorldSpaceLightPos0.xyz);
                    else // point / spot light fallback
                    Ld = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);

                    // Lambert diffuse lighting : dot product of normal and light direction
                    float ndotl = saturate(dot(n, Ld));
                    dirDiffuse = _LightColor0.rgb * ndotl;
                }

                // -- - CUSTOM POINT LIGHT -- -
                // Our own point light that can be moved around via script
                float3 pointDiffuse = 0;

                if (_EnablePointLight > 0.5) // Is changed in Source / Lights / LightControl.cs
                {
                    // Calculate vector from surface to light
                    float3 toPoint = _LightPosition.xyz - i.worldPos;
                    float dist = length(toPoint);

                    if (dist > 1e-4) // Avoid division by zero; IF YOU USE AUTO FORMATTING (1e-4) CAN SPLIT AND NOT COUNT AS EXPONENT
                    {
                        float3 Lp = toPoint / dist; // normalized light direction
                        float ndot = saturate(dot(n, Lp)); // Dot normal of surface and light direction; saturate clamps 0 - 1

                        // light falls off with distance squared
                        float atten = 1.0 / (1.0 + 0.1 * dist + 0.02 * dist * dist);

                        pointDiffuse =
                        _PointLightColor.rgb * ndot * atten * _PointLightIntensity;
                    }
                }

                // Combine all lighting contributions
                float3 lighting = ambient + dirDiffuse + pointDiffuse;

                // -- - BASE COLOR : TEXTURE OR SOLID COLOR -- -
                fixed4 baseCol;
                if (_UseTexture < 0.5)
                baseCol = _Color; // Use solid color
                else
                baseCol = tex2D(_MainTex, i.uv) * _Color; // Sample texture and tint

                // Apply lighting to final color
                baseCol.rgb *= lighting;
                return baseCol;
            }
            ENDCG
        }
    }

    Fallback Off
}
