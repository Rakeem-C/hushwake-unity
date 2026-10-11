Shader "Hushwake/Sky"{
 Properties{_Hush("Hush",Range(0,1))=0}
 SubShader{Tags{"Queue"="Background" "RenderType"="Background"}Cull Off ZWrite Off
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct a{float4 vertex:POSITION;};struct v{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};float _Hush;
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.dir=i.vertex.xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.54);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 fixed4 frag(v i):SV_Target{float3 d=normalize(i.dir);float h=saturate(d.y);float3 sky=lerp(float3(.61,.70,.72),float3(.24,.40,.48),pow(h,.45));float2 uv=d.xz/max(.14,d.y)*.9;float cloud=noise(uv)+noise(uv*2.2)*.5+noise(uv*4.4)*.23;cloud=smoothstep(.77,1.24,cloud)*smoothstep(.02,.2,d.y);sky=lerp(sky,float3(.75,.79,.78),cloud*.6);sky=GammaToLinearSpace(sky);sky=lerp(sky,sky*float3(.24,.28,.54),_Hush);return float4(sky,1);}
 ENDCG}
 }
}
