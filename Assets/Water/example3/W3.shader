// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "FX/Water3" {
	Properties{
		_RefrColor("Refraction color", COLOR) = (.34, .85, .92, 1)
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "" {}
		[HideInInspector] _RefractionTex("Internal Refraction", 2D) = "" {}
	}
	Subshader{
		Tags { "WaterMode" = "Refractive" "RenderType" = "Opaque" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE

			#if defined (WATER_REFRACTIVE)
			#define HAS_REFRACTION 1
			#endif


			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 ref : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.ref = ComputeNonStereoScreenPos(o.pos);

				return o;
			}

			sampler2D _ReflectionTex;

			#if HAS_REFRACTION
			sampler2D _RefractionTex;
			uniform float4 _RefrColor;
			#endif

			half4 frag(v2f i) : SV_Target
			{
				// perturb reflection/refraction UVs by bumpmap, and lookup colors
				float4 uv1 = i.ref;
				half4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
				#if HAS_REFRACTION
				float4 uv2 = i.ref;
				half4 refr = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(uv2)) * _RefrColor;
				#endif

				// final color is between refracted and reflected based on fresnel
				half4 color;

				#if HAS_REFRACTION
				color = lerp(refr, refl, 0.5);
				#else
				color = refl;
				#endif
				return color;
			}
			ENDCG
		}
	}
}
