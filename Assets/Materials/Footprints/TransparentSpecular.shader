Shader "Custom/TransparentSpecular" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_SpecTex("Specular", 2D) = "white" {}
		_NormTex("Normal", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Trans("Transparency", Range(0,1)) = 0.0
	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT SpecularSetup
	ENDCG

	SubShader {
		Tags { "RenderType"="Transparent" "PreviewType"="Plane" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#include "UnityPBSLighting.cginc"
		#pragma surface surf StandardSpecular fullforwardshadows alpha:premul

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _SpecTex;
		sampler2D _NormTex;


		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		float _Trans;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Specular = max(0, tex2D(_SpecTex, IN.uv_MainTex) - (1 - _Trans));
			o.Normal = UnpackNormal(tex2D(_NormTex, IN.uv_MainTex));
			o.Smoothness = _Glossiness * tex2D(_SpecTex, IN.uv_MainTex).a;
			o.Alpha = c.a * _Trans;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
