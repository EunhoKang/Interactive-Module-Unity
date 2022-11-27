// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NuitrackSDK/NuitrackDemos/CustomMesh" 
{
	Properties 
	{
		_DepthTex ("Depth texture", 2D) = "white" {}
		_SegmentationTex("Segmentation texture", 2D) = "white" {}
		_RGBTex("RGB texture", 2D) = "white" {}
		_CutoffDiff("Depth diff cutoff threshold", Float) = 0.2
		_ShowBorders ("Show borders", int) = 1
		_SegmZeroColor("Segmentation zero color", Color) = (1,1,1,1)
	}
	SubShader 
	{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
		Cull Off
		ZWrite On
	    LOD 200
		Blend SrcAlpha OneMinusSrcAlpha

	    Pass
	    {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//Not working on IOS
			#if !SHADER_API_METAL  
			uniform float fX;
			uniform float fY;

		    struct appdata
            {
                float4 vertex 	: POSITION;
                float2 depthPos : TEXCOORD0;
            };

        	struct v2f
            {
            	float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

			float _CutoffDiff;
			sampler2D _DepthTex;
			float4 _DepthTex_TexelSize;
			sampler2D _SegmentationTex;
			sampler2D _RGBTex;
			int _ShowBorders;
			float4 _SegmZeroColor;
			float _maxSensorDepth;

            v2f vert (appdata v)
            {
            	v2f o;

            	float4 depthCol = tex2Dlod(_DepthTex, float4(v.depthPos, 0, 0));
				float midDepth = (depthCol.r + depthCol.g + depthCol.b) / 3;

				float z = midDepth * _maxSensorDepth;

				float x =  z * (v.depthPos.x - 0.5) / fX;
				float y = -z * ((1 - v.depthPos.y) - 0.5) / fY;

            	float4 newVertexPos = float4(x, y, z, 1);
            	float4 rgbCol = tex2Dlod(_RGBTex, float4(v.depthPos, 0, 0));
            	float4 segmCol = tex2Dlod(_SegmentationTex, float4(v.depthPos, 0, 0));
            	float zeroLength = length(segmCol - _SegmZeroColor);

            	o.color = rgbCol;

            	float segmColUp = length(segmCol - 		tex2Dlod(_SegmentationTex, float4(v.depthPos + float2(0, 0.01667), 0, 0)));
				float segmColDown = length(segmCol - 	tex2Dlod(_SegmentationTex, float4(v.depthPos - float2(0, 0.01667), 0, 0)));
				float segmColLeft = length(segmCol - 	tex2Dlod(_SegmentationTex, float4(v.depthPos - float2(0.01250, 0), 0, 0)));
				float segmColRight = length(segmCol - 	tex2Dlod(_SegmentationTex, float4(v.depthPos + float2(0.01250, 0), 0, 0)));

				if ((_ShowBorders != 0) && (zeroLength > 0.01) && ((segmColUp > 0.01) || (segmColDown > 0.01) || 
				(segmColLeft > 0.01) || (segmColRight > 0.01))) o.color = lerp(o.color, segmCol, 0.5);

            	float4 zRightCol = tex2Dlod(_DepthTex, float4(v.depthPos + float2(_DepthTex_TexelSize.x, 0), 0, 0));
            	float4 zUpCol = tex2Dlod(_DepthTex, float4(v.depthPos + float2(0, _DepthTex_TexelSize.y), 0, 0));
            	float4 zRightUpCol = tex2Dlod(_DepthTex, float4(v.depthPos + float2(_DepthTex_TexelSize.x, _DepthTex_TexelSize.y), 0, 0));

            	float zRight = 	zRightCol.r * _maxSensorDepth;
            	float zUp = 	zUpCol.r * _maxSensorDepth;
            	float zRightUp = zRightUpCol.r * _maxSensorDepth;

            	if ((abs(z) < 1) || (abs(z - zRight) > max(z, zRight) * _CutoffDiff) || 
            		(abs(z - zUp) > max(z, zUp) * _CutoffDiff) || 
            		(abs(z - zRightUp) > max(z, zRightUp) * _CutoffDiff)) o.color.a = 0;
        		
                o.vertex = UnityObjectToClipPos(newVertexPos);

            	return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
            	return i.color;
            }
			#endif
		    ENDCG
	    }
	}
	FallBack "Diffuse"
}
