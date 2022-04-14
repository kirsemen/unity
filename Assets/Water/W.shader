Shader "FX/Water" {
	Properties{
		_ReflDistort("Reflection distort", Range(0,1.5)) = 0.44
		_RefrDistort("Refraction distort", Range(0,1.5)) = 0.40
		_RefrColor("Refraction color", COLOR) = (.34, .85, .92, 1)
		[NoScaleOffset] _RefractionFresnel("Refraction Fresnel", 2D) = "gray" {}
		[NoScaleOffset] _ReflectionFresnel("Reflection Fresnel", 2D) = "" {}
		[NoScaleOffset] _BumpTex("Bump Texture", 2D) = "black"{}
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "" {}
		[HideInInspector] _RefractionTex("Internal Refraction", 2D) = "" {}
		_tess("Tesseletion", vector) = (1,1,1,1)
	}
		Subshader{
			Tags { "WaterMode" = "Refractive" "RenderType" = "Opaque" }
			Pass {
				CGPROGRAM

				#pragma vertex vert
				#pragma hull hull_shader
				#pragma domain domain_shader
				#pragma fragment frag

				#pragma multi_compile_fog
				#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE

				#if defined (WATER_REFRACTIVE)
				#define HAS_REFRACTION 1
				#endif

				#include "UnityCG.cginc"

				half3 UnpakeNormalMap(half4 packednormal) {
					half3 normal;
					normal.xy = packednormal.xy * 2 - 1;
					normal.z = sqrt(1 - normal.x * normal.x - normal.y * normal.y);
					return normal;
				}


				struct appdata {
					float4 vertex : POSITION;
					float3 uv : TEXCOORD0;
				};

				struct InternalTessInterp_appdata {
				  float4 vertex : INTERNALTESSPOS;
				  float3 uv : TEXCOORD0;
				};

				InternalTessInterp_appdata vert(appdata v) {
					InternalTessInterp_appdata o;
					o.vertex = v.vertex;
					o.uv = v.uv;
					return o;
				}

				struct TessellationFactors {
				  float edge[3] : SV_TessFactor;
				  float inside : SV_InsideTessFactor;
				};

				float4 _tess;

				TessellationFactors hull_const(InputPatch<InternalTessInterp_appdata, 3> v) {
					TessellationFactors o;
					o.edge[0] = _tess.x;
					o.edge[1] = _tess.y;
					o.edge[2] = _tess.z;
					o.inside = _tess.w;
					return o;
				}

				[UNITY_domain("tri")]                  //triangle
				[UNITY_partitioning("fractional_odd")] //integer,fractional_even,fractional_odd
				[UNITY_outputtopology("triangle_cw")]  //triangle_cw, triangle_ccw
				[UNITY_patchconstantfunc("hull_const")]//patch function
				[UNITY_outputcontrolpoints(3)]         //output control point count
				InternalTessInterp_appdata hull_shader(InputPatch<InternalTessInterp_appdata, 3> v, uint id : SV_OutputControlPointID) {
					return v[id];
				}

				struct v2f {
					float4 pos : SV_POSITION;
					float4 ref : TEXCOORD0;
					float4 uv : TEXCOORD1;
					float3 viewDir : TEXCOORD2;
				};


				sampler2D _BumpTex;
				[UNITY_domain("tri")]
				v2f domain_shader(TessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata, 3> vi, float3 bary : SV_DomainLocation) {
					v2f o;
					o.uv = float4((vi[0].uv * bary.x + vi[1].uv * bary.y + vi[2].uv * bary.z).xyz, 0);
					o.pos = UnityObjectToClipPos(
						(vi[0].vertex * bary.x + vi[1].vertex * bary.y + vi[2].vertex * bary.z).xyzw + float4(0, tex2Dlod(_BumpTex, float4(o.uv.xy, 0, 0)).a, 0, 0));
					o.ref = ComputeNonStereoScreenPos(o.pos);
					o.viewDir = WorldSpaceViewDir((vi[0].vertex * bary.x + vi[1].vertex * bary.y + vi[2].vertex * bary.z).xyzw).xzy;// +float4(0, tex2Dlod(_BumpTex, float4(o.uv.xy, 0, 0)).a, 0, 0));
					return o;
				}


				sampler2D _ReflectionTex;
				uniform float4 _RefrColor;
				uniform float _ReflDistort;

				#if HAS_REFRACTION
				uniform float _RefrDistort;
				sampler2D _RefractionTex;
				sampler2D _RefractionFresnel;
				#else
				sampler2D _ReflectionFresnel;
				#endif

				half4 frag(v2f i) : SV_Target{
					i.viewDir = normalize(i.viewDir);

					half3 bump = UnpakeNormalMap(tex2Dlod(_BumpTex, float4(i.uv.xy, 0, 0)));
					
					half fresnelFac = dot(i.viewDir, bump);

					half4 color;
					float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
					half4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
					#if HAS_REFRACTION
					float4 uv2 = i.ref;uv2.xy -= bump * _RefrDistort;
					half4 refr = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(uv2)) * _RefrColor;
					half fresnel = UNITY_SAMPLE_1CHANNEL(_RefractionFresnel, float2(fresnelFac, fresnelFac));
					color = lerp(refr, refl, fresnel);
					#else
					half4 water = tex2D(_ReflectionFresnel, float2(fresnelFac, fresnelFac));
					color.rgb = lerp(water.rgb, refl.rgb, water.a);
					color.a = refl.a * water.a;
					//color = lerp(_RefrColor, refl, 0.5);
					#endif
					return color;
				}
				ENDCG
			}
		}
}
