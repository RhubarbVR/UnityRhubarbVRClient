Shader "Rhu/RhuUnlitBlendTwoSided" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
	}

        SubShader{
            Cull Off
            Tags {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
            }
            Lighting Off ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform fixed4 _Color;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color * _Color;
                    o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                    return col;
                }
                ENDCG
            }
        }
}