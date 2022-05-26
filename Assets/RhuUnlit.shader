Shader "Unlit/RhuUnlit" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color("Tint", Color) = (0.0, 0.0, 1.0, 1.0)
		_Alpha("Alpha ratio", Float) = 1.0
	}

		SubShader{
				Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
				LOD 200
				ZWrite On
				Blend SrcAlpha OneMinusSrcAlpha

				Pass {
					CGPROGRAM
						#pragma vertex vert
						#pragma fragment frag
						#pragma multi_compile_fog
						#include "UnityCG.cginc"

						struct appdata_t {
							float4 vertex : POSITION;
							float2 texcoord : TEXCOORD0;
						};



						struct v2f {
							float4 vertex : SV_POSITION;
							half2 texcoord : TEXCOORD0;
							UNITY_FOG_COORDS(1)
						};

						sampler2D _MainTex;
						float4 _MainTex_ST;
						float4 _Color;
						float _Alpha;

						v2f vert(appdata_t v)
						{
							v2f o;
							o.vertex = UnityObjectToClipPos(v.vertex);
							float2 flip = v.texcoord;
							flip.y = -flip.y;
							o.texcoord = TRANSFORM_TEX(flip, _MainTex);
							UNITY_TRANSFER_FOG(o,o.vertex);
							return o;
						}

						fixed4 frag(v2f i) : SV_Target
						{
							fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
							col.a = col.a * _Alpha;
							UNITY_APPLY_FOG(i.fogCoord, col);
							return col;
						}
					ENDCG
				}
		}

}