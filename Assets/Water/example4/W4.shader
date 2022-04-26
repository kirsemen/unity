Shader "FX/Water4" {
	Properties{
		_RefrColor("Refraction color", COLOR) = (.34, .85, .92, 1)
		_BumpTex("Bump Texture", 2D) = "black"{}
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
				#pragma geometry geo
				#pragma fragment frag

				#pragma multi_compile_fog
				#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE

				#if defined (WATER_REFRACTIVE)
				#define HAS_REFRACTION 1
				#endif

				#include "UnityCG.cginc"


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
				};


				[UNITY_domain("tri")]
				v2f domain_shader(TessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata, 3> vi, float3 bary : SV_DomainLocation) {
					v2f o;
					o.pos = (vi[0].vertex * bary.x + vi[1].vertex * bary.y + vi[2].vertex * bary.z).xyzw;
					o.ref = float4(0, 0, 0, 0);
					o.uv = float4((vi[0].uv * bary.x + vi[1].uv * bary.y + vi[2].uv * bary.z).xyz, 0);
					return o;
				}
				sampler2D _BumpTex;

				[maxvertexcount(3)]
				void geo(triangle v2f IN[3] : SV_POSITION, inout TriangleStream<v2f> triStream)
				{
					v2f o;
					for (int i = 0; i < 3; i++)
					{
						o.pos = UnityObjectToClipPos(IN[i].pos + float4(0, tex2Dlod(_BumpTex, float4(IN[i].uv.xy, 0, 0)).r,0,0));
						o.ref = ComputeNonStereoScreenPos(o.pos);
						o.uv = IN[i].uv;
						triStream.Append(o);
					}
				}

				sampler2D _ReflectionTex;
				uniform float4 _RefrColor;

				#if HAS_REFRACTION
				sampler2D _RefractionTex;
				#endif

				fixed4 frag(v2f i) : SV_Target{

					half4 color;
					#if HAS_REFRACTION
					float4 uv1 = i.ref;
					half4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1))+ _RefrColor * (1-tex2Dlod(_BumpTex, float4(i.uv.xy, 0, 0)).r)*4;
					float4 uv2 = i.ref;
					half4 refr = tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(uv2)) * _RefrColor*(1+tex2Dlod(_BumpTex, float4(i.uv.xy, 0, 0)).r);
					color = lerp(refr, refl, 0.1+ tex2Dlod(_BumpTex, float4(i.uv.xy, 0, 0)).r/1.1);
					#else
					float4 uv1 = i.ref;
					half4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1));
					color = lerp(_RefrColor, refl, 0.1 + tex2Dlod(_BumpTex, float4(i.uv.xy, 0, 0)).r / 1.1);
					#endif
					return color;
				}
				ENDCG
			}
		}
}
