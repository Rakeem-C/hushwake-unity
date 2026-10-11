Shader "Hushwake/Meadow" {
 Properties {_MainTex("Original meadow leaf",2D)="white"{}}
 SubShader {Tags{"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
 CGPROGRAM
 #pragma surface surf Lambert vertex:vert addshadow fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex;struct Input {float2 uv_MainTex;float4 color:COLOR;};
 void vert(inout appdata_full v){float phase=v.vertex.x*.8+v.vertex.z*.65;v.vertex.x+=sin(_Time.y*1.3+phase)*v.texcoord.y*v.texcoord.y*.06;}
 void surf(Input IN,inout SurfaceOutput o){float4 tex=tex2D(_MainTex,IN.uv_MainTex);clip(tex.a-.36);o.Albedo=GammaToLinearSpace(IN.color.rgb)*(.77+tex.r*.35);o.Normal=float3(0,0,1);o.Alpha=1;}
 ENDCG
 }Fallback "Diffuse"
}
