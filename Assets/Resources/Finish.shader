Shader "Hushwake/Finish"{Properties{_MainTex("Scene",2D)="white"{}}SubShader{Cull Off ZWrite Off ZTest Always Pass{CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize;
 float luma(float3 c){return dot(c,float3(.299,.587,.114));}
 fixed4 frag(v2f_img i):SV_Target{
 float2 px=_MainTex_TexelSize.xy;float3 c=tex2D(_MainTex,i.uv).rgb;
 float3 nw=tex2D(_MainTex,i.uv+px*float2(-1,-1)).rgb,ne=tex2D(_MainTex,i.uv+px*float2(1,-1)).rgb,sw=tex2D(_MainTex,i.uv+px*float2(-1,1)).rgb,se=tex2D(_MainTex,i.uv+px*float2(1,1)).rgb;
 float a=luma(nw),b=luma(ne),d=luma(sw),e=luma(se),m=luma(c);float lo=min(m,min(min(a,b),min(d,e))),hi=max(m,max(max(a,b),max(d,e)));
 float2 dir=float2(-((a+b)-(d+e)),(a+d)-(b+e));float reduce=max((a+b+d+e)*.03125,.0078125);dir=clamp(dir/(min(abs(dir.x),abs(dir.y))+reduce),-6,6)*px;
 float3 filtered=(tex2D(_MainTex,i.uv+dir*(1.0/3-.5)).rgb+tex2D(_MainTex,i.uv+dir*(2.0/3-.5)).rgb)*.5;
 if(hi-lo>max(.03,hi*.12))c=filtered;
 float3 glow=max(0,(nw+ne+sw+se)*.25-.82);c+=glow*.07;
 // Preserve shadow detail rather than crushing it with a strong contrast/vignette.
 c=(c-.5)*1.025+.5;float2 p=i.uv*2-1;c*=1-dot(p,p)*.045;return float4(c,1);}
 ENDCG}}}
