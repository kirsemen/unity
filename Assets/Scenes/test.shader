Shader "Custom/test"
{
	Properties{
		_MaxDist("Max Distance", float) = 10
		_MinDist("Min Distance", float) = 1
		_MaxTess("Max Tessellation", int) = 4
		_MinTess("Min Tessellation", int) = 1
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DispTex("Disp Texture", 2D) = "black" {}
		_NormalMap("Normalmap", 2D) = "bump" {}
		_Displacement("Displacement",float) = 0.3
		_Color("Color", color) = (1,1,1,0)
		_SpecColor("Spec color", color) = (0.5,0.5,0.5,0.5)
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 300

			CGPROGRAM
			#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap
			#pragma target 4.6
			#include "Tessellation.cginc"


			float _MaxDist;
			float _MinDist;
			float _MaxTess;
			float _MinTess;


			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			float4 tess(appdata v0, appdata v1, appdata v2) {
				float3 wpos = mul(unity_ObjectToWorld, v0.vertex).xyz;
				float dist0 = distance(wpos, _WorldSpaceCameraPos);
				wpos = mul(unity_ObjectToWorld, v1.vertex).xyz;
				float dist1 = distance(wpos, _WorldSpaceCameraPos);
				wpos = mul(unity_ObjectToWorld, v2.vertex).xyz;
				float dist2 = distance(wpos, _WorldSpaceCameraPos);
				float dist = (dist0 + dist1 + dist2) / 3;

				return (clamp(dist, _MinDist, _MaxDist) - _MinDist) / (_MaxDist - _MinDist) * (_MinTess - _MaxTess) + _MaxTess;
			}


			sampler2D _DispTex;
			float _Displacement;

			void disp(inout appdata v)
			{
				float d = tex2Dlod(_DispTex, float4(v.texcoord.xy,0,0)).r * _Displacement;
				v.vertex.xyz += v.normal * d;
			}

			struct Input {
				float2 uv_MainTex;
			};

			sampler2D _MainTex;
			sampler2D _NormalMap;
			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutput o) {
				half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Specular = 0.2;
				o.Gloss = 1.0;
				o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
			}
			ENDCG
		}
			FallBack "Diffuse"
}
