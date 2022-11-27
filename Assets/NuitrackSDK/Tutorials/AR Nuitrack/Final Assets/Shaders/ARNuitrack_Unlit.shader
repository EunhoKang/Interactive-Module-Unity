Shader "NuitrackSDK/Tutorials/ARNuitrack/ARNuitrack_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			float4 _CameraPosition;
            float4 _MainTex_ST;

			uniform StructuredBuffer<uint> _DepthFrame : register(t1);
			int _textureWidth;
			int _textureHeight;
			float _maxDepthSensor;

            v2f vert (appdata v)
            {
                v2f o;

				uint rawIndex = _textureWidth * (v.uv.y * _textureHeight) + (v.uv.x * _textureWidth);
				
				// (rawIndex >> 1) == (rawIndex / 2). Because one buffer value contains depth values for two pixels
				uint depthPairVal = _DepthFrame[rawIndex >> 1];

				// Shift trick, because in the Shader we read two values (Int16) as one (Int32)
				uint depthVal = rawIndex % 2 != 0 ? depthPairVal >> 16 : (depthPairVal << 16) >> 16;

				// *1000 because the depth is in millimeters
				float depth = 1 - (float(depthVal) / (_maxDepthSensor * 1000));

				if (depth == 1)
					depth = 0;

				float4 deltaCam = _CameraPosition - v.vertex;
				float4 shiftToCam = normalize(deltaCam) * depth;
				float4 newVertex = v.vertex + shiftToCam * length(deltaCam);

                o.vertex = UnityObjectToClipPos(newVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv).bgra;
            }
            ENDCG
        }
    }
}
