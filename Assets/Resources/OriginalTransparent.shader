Shader "Hushwake/OriginalTransparent" {
 Properties { _Color("Tint",Color)=(1,1,1,.42) _MainTex("Albedo",2D)="white"{} _NormalMap("Normal",2D)="bump"{} _RoughnessMap("Roughness",2D)="white"{} _Glossiness("Smoothness",Range(0,1))=.15 _Metallic("Metallic",Range(0,1))=0 _EmissionColor("Emission",Color)=(0,0,0,0) _Cutoff("Cutout",Range(0,1))=0 _HasNormal("Normal present",Float)=0 _HasRoughness("Roughness present",Float)=0 _Cull("Cull",Float)=2 _Wind("Leaf movement",Float)=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull [_Cull] ZWrite Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade fullforwardshadows vertex:vert
 #pragma target 3.0
 struct Input {float2 uv_MainTex;float4 color:COLOR;};sampler2D _MainTex,_NormalMap,_RoughnessMap;fixed4 _Color,_EmissionColor;half _Glossiness,_Metallic,_Cutoff,_HasNormal,_HasRoughness;float _Wind;
 void vert(inout appdata_full v){float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;v.vertex.x+=sin(_Time.y+w.x*.19+w.z*.16)*max(0,v.vertex.y-2)*.006*_Wind;}
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 tex=tex2D(_MainTex,IN.uv_MainTex);clip(tex.a-_Cutoff);o.Albedo=tex.rgb*_Color.rgb*IN.color.rgb;o.Alpha=tex.a*_Color.a;o.Metallic=_Metallic;o.Smoothness=lerp(_Glossiness,1-tex2D(_RoughnessMap,IN.uv_MainTex).g,_HasRoughness);o.Normal=lerp(float3(0,0,1),normalize(tex2D(_NormalMap,IN.uv_MainTex).xyz*2-1),_HasNormal);o.Emission=_EmissionColor.rgb;}
 ENDCG } Fallback Off
}
