Shader "Hushwake/Hair" {
 Properties { _Color("Hair tint",Color)=(.15,.12,.1,1) _MainTex("Hair surface",2D)="white"{} _Glossiness("Soft sheen",Range(0,1))=.32 }
 SubShader { Tags {"RenderType"="Opaque"} Cull Back
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 struct Input {float2 uv_MainTex;float4 color:COLOR;float3 viewDir;};
 sampler2D _MainTex;fixed4 _Color;half _Glossiness;
 void surf(Input i,inout SurfaceOutputStandard o){
  float strand=sin(i.uv_MainTex.x*370+sin(i.uv_MainTex.y*18)*2)*.025;
  o.Albedo=tex2D(_MainTex,i.uv_MainTex).rgb*_Color.rgb*i.color.rgb*(.97+strand);
  o.Smoothness=_Glossiness;o.Metallic=0;o.Alpha=1;
  float rim=pow(1-saturate(dot(normalize(i.viewDir),float3(0,0,1))),3);
  o.Emission=_Color.rgb*rim*.065;
 }
 ENDCG
 } Fallback "Standard"
}
