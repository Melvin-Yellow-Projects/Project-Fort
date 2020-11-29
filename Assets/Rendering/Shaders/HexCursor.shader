Shader "Custom/HexCursor"
{
	Properties
	{
		_MainTex("Example Texture", 2D) = "white" {}
		_Color("Example Color", Color) = (1, 1, 1, 1)
		_Strength("Example Range", Range(0, 1)) = 1
	}

	SubShader
	{
		Tags { "Queue" = "Transparent+10" }

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;
		half _Strength;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf(Input IN, inout SurfaceOutput o) 
		{
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			//o.Albedo = (c * _Color * _Strength).rgb;
			o.Albedo = _Color.rgb;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
