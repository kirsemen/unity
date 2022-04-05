Shader "FX/Water" {
	Properties{
		_RefrColor("Refraction color", COLOR) = (.34, .85, .92, 1)
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


				struct appdata {
					float4 vertex : POSITION;
				};

				struct InternalTessInterp_appdata {
				  float4 vertex : INTERNALTESSPOS;
				};

				InternalTessInterp_appdata vert(appdata v) {
					InternalTessInterp_appdata o;
					o.vertex = v.vertex;
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
				};


				[UNITY_domain("tri")]
				v2f domain_shader(TessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata, 3> vi, float3 bary : SV_DomainLocation) {
					v2f o;
					o.pos = UnityObjectToClipPos(vi[0].vertex * bary.x + vi[1].vertex * bary.y + vi[2].vertex * bary.z);
					o.ref = ComputeNonStereoScreenPos(o.pos);
					return o;
				}

				sampler2D _ReflectionTex;

				#if HAS_REFRACTION
				sampler2D _RefractionTex;
				uniform float4 _RefrColor;
				#endif

				fixed4 frag(v2f i) : SV_Target{
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
