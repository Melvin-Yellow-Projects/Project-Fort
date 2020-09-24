Shader "Custom/LambertClamp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _ColorStrength ("Color Strength", Range(0, 1)) = 1
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 1
    }

	SubShader
    {

		Tags
        {
			"Queue" = "Geometry"
		}

		CGPROGRAM
		#pragma surface surf CustomLightingFunc

        sampler2D _MainTex;
		float4 _Color;
        half _ColorStrength;
        half _ShadowStrength;

        half4 LightingCustomLightingFunc (SurfaceOutput s, half3 lightDir, half atten)
        {
            // initialization
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;

            // add light color
            c.rgb *= _LightColor0.rgb;

            // shadow calculation 
            c.rgb *= clamp(dot(s.Normal, lightDir), (1 - _ShadowStrength), 1);
            c.rgb *= clamp(atten, (1 - _ShadowStrength), 1);

            //c.rgb = s.Albedo;

            return c;
        }

		struct Input
        {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = (tex2D(_MainTex, IN.uv_MainTex) * _Color * _ColorStrength).rgb;
		}

		ENDCG 

	}

	FallBack "Diffuse"

}
