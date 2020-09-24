Shader "Custom/VertexColors2"
{
	Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorStrength("Color Strength", Range(0, 1)) = 1
		_ShadowStrength("Shadow Strength", Range(0, 1)) = 1
	}

	SubShader
    {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf LambertClampFunc
		#pragma target 3.0

		sampler2D _MainTex;
		float4 _Color;
		half _ColorStrength;
		half _ShadowStrength;

		half4 LightingLambertClampFunc(SurfaceOutput s, half3 lightDir, half atten)
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

			return c;
		}

		struct Input
        {
			float2 uv_MainTex;
			float4 color : COLOR; // gets color from the uv's ?
		};

		void surf (Input IN, inout SurfaceOutput o)
        {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * _ColorStrength;
			o.Albedo = saturate(c.rgb + IN.color);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}