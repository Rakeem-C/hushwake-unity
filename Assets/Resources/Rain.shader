Shader "Hushwake/Rain"{SubShader{Tags{"Queue"="Transparent"}Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct a{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}fixed4 frag(v i):SV_Target{return fixed4(.75,.86,.87,sin(i.uv.y*3.14159)*.20);}
 ENDCG}}}
