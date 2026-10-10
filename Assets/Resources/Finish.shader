Shader "Hushwake/Finish"{Properties{_MainTex("Scene",2D)="white"{}}SubShader{Cull Off ZWrite Off ZTest Always Pass{CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize;
 fixed4 frag(v2f_img i):SV_Target{float3 c=tex2D(_MainTex,i.uv).rgb;float3 glow=0;for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++){float3 tap=tex2D(_MainTex,i.uv+float2(x,y)*_MainTex_TexelSize.xy*5).rgb;glow+=max(0,tap-.78);}c+=glow*.027;c=(c-.5)*1.07+.5;float l=dot(c,float3(.2126,.7152,.0722));c=lerp(l.xxx,c,1.04);float2 p=i.uv*2-1;c*=1-dot(p,p)*.08;return float4(c,1);}
 ENDCG}}}
