// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "FX/Water2" {
	Properties{
		_WaveScale("Wave scale", Range(0.02,0.15)) = 0.063
		_ReflDistort("Reflection distort", Range(0,1.5)) = 0.44
		_RefrDistort("Refraction distort", Range(0,1.5)) = 0.40
		_RefrColor("Refraction color", COLOR) = (.34, .85, .92, 1)
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "" {}
		[HideInInspector] _RefractionTex("Internal Refraction", 2D) = "" {}
	}


		// -----------------------------------------------------------
		// Fragment program cards


		Subshader{
			Tags { "WaterMode" = "Refractive" "RenderType" = "Opaque" }
			Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fog
		#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE

		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
		#define HAS_REFLECTION 1
		#endif
		#if defined (WATER_REFRACTIVE)
		#define HAS_REFRACTION 1
		#endif


		#include "UnityCG.cginc"

		uniform float4 _WaveScale4;
		uniform float4 _WaveOffset;

		#if HAS_REFLECTION
		uniform float _ReflDistort;
		#endif
		#if HAS_REFRACTION
		uniform float _RefrDistort;
		#endif

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				float4 ref : TEXCOORD0;
				float2 bumpuv0 : TEXCOORD1;
				float2 bumpuv1 : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
			#else
				float2 bumpuv0 : TEXCOORD0;
				float2 bumpuv1 : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			#endif
			UNITY_FOG_COORDS(4)
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);


			// scroll bump waves
			float4 temp;
			float4 wpos = mul(unity_ObjectToWorld, v.vertex);
			temp.xyzw = wpos.xzxz * _WaveScale4 + _WaveOffset;
			o.bumpuv0 = temp.xy;
			o.bumpuv1 = temp.wz;

			// object space view direction (will normalize per pixel)
			o.viewDir.xzy = WorldSpaceViewDir(v.vertex);

			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
			o.ref = ComputeNonStereoScreenPos(o.pos);
			#endif

			UNITY_TRANSFER_FOG(o,o.pos);
			return o;
		}

		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
		sampler2D _ReflectionTex;
		#endif
		#if defined (WATER_REFRACTIVE)
		sampler2D _RefractionTex;
		uniform float4 _RefrColor;
		#endif

		half4 frag(v2f i) : SV_Target
		{
			i.viewDir = normalize(i.viewDir);


		// perturb reflection/refraction UVs by bumpmap, and lookup colors

		#if HAS_REFLECTION
		float4 uv1 = i.ref; uv1.xy += 0 * _ReflDistort;
		half4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
		#endif
		#if HAS_REFRACTION
		float4 uv2 = i.ref; uv2.xy -= 0 * _RefrDistort;
		half4 refr = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(uv2)) * _RefrColor;
		#endif

		// final color is between refracted and reflected based on fresnel
		half4 color;

		#if defined(WATER_REFRACTIVE)
		color = lerp(refr, refl, 0.5);
		#endif

		#if defined(WATER_REFLECTIVE)
		color = refl;
		#endif


		UNITY_APPLY_FOG(i.fogCoord, color);
		return color;
	}
	ENDCG

		}
	}

}
