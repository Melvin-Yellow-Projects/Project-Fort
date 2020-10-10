Shader "Custom/Terrain2"
{
	Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MainTex ("Terrain Texture Array", 2DArray) = "white" {}
        _GridTex ("Grid Texture", 2D) = "white" {}
		_ColorStrength("Color Strength", Range(0, 1)) = 1
		_ShadowStrength("Shadow Strength", Range(0, 1)) = 1

        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader
    {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf LambertClampFunc vertex:vert
		#pragma target 3.5

        #pragma multi_compile _ GRID_ON

        #include "HexCellData.cginc"

		//sampler2D _MainTex; // this was when MainTex was only one texture
        UNITY_DECLARE_TEX2DARRAY(_MainTex); // MainTex is now a texture array
        sampler2D _GridTex;
		float4 _Color;

		half _ColorStrength;
		half _ShadowStrength;

        //half _Glossiness;
		//half _Metallic;

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
			//float2 uv_MainTex;
			float4 color : COLOR; // gets color from the mesh's uv's ?
            float3 worldPos;
            float3 terrain;
		};

        void vert (inout appdata_full v, out Input data) 
        {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			// retrieve the cell data for all three cell indices stored in the vertex data. Then 
			// assign their terrain indices to data.terrain.
			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.terrain.x = cell0.w;
			data.terrain.y = cell1.w;
			data.terrain.z = cell2.w;
		}

        float4 GetTerrainColor (Input IN, int index) 
        {

            // parameters for UNITY_SAMPLE_TEX2DARRAY()...

            // "The first two coordinates are regular UV coordinates. We'll use the world XZ
            // coordinates, scaled by 0.02. That produces a good texture resolution when fully
            // zoomed in, with textures tiling roughly every four cells [...] The third coordinate 
            // is used to index the texture array [...]""
			float3 uvw = float3(IN.worldPos.xz * 0.02, IN.terrain[index]);

            // sample the texture array
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);

            // modulate the sample with the splat map for one index
			return c * IN.color[index];
		}

		void surf (Input IN, inout SurfaceOutput o)
        {
            // "We have to sample the texture array three times per fragment [...] and combine the
            // results."
            fixed4 c = GetTerrainColor(IN, 0) + GetTerrainColor(IN, 1) + GetTerrainColor(IN, 2);

            // this value is calculated off of HexMetrics, thus it should be exposed to script
            fixed4 grid = 1;
			#if defined(GRID_ON)
                float2 gridUV = IN.worldPos.xz;
			    gridUV.x *= 1 / (4 * 8.66025404);
			    gridUV.y *= 1 / (2 * 15.0);

	            grid = tex2D(_GridTex, gridUV);
			#endif

			o.Albedo = c.rgb * grid * _Color;

			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * _ColorStrength;
			//o.Albedo = saturate(c.rgb + IN.color);

            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;

            o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}