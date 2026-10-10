Shader "Hushwake/ContactShadow" { SubShader { Tags {"Queue"="Transparent-5" "RenderType"="Transparent"} ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct varying {float4 position:SV_POSITION;float2 uv:TEXCOORD0;};
 varying vert(app i){varying o;o.position=UnityObjectToClipPos(i.vertex);o.uv=i.uv;return o;}
 fixed4 frag(varying i):SV_Target{float radius=length(i.uv*2-1);float opacity=pow(saturate(1-radius),1.4)*.28;return float4(.025,.035,.029,opacity);}
 ENDCG } } }
