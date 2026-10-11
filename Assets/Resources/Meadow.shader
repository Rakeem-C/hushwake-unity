Shader "Hushwake/Meadow" {
 Properties {_MainTex("Original meadow leaf",2D)="white"{}}
 SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard vertex:vert addshadow fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex,_GroundShade;struct Input {float2 uv_MainTex;float4 color:COLOR;float3 worldPos;};
 void vert(inout appdata_full v){float phase=v.vertex.x*.8+v.vertex.z*.65;v.vertex.x+=sin(_Time.y*1.3+phase)*v.texcoord.y*v.texcoord.y*.06;}
 void surf(Input IN,inout SurfaceOutputStandard o){float shade=tex2D(_GroundShade,(IN.worldPos.xz+125)/250).g;float vein=1-.14*abs(IN.uv_MainTex.x*2-1);o.Albedo=IN.color.rgb*lerp(.55,1.15,IN.uv_MainTex.y)*vein*shade;o.Smoothness=.07;o.Normal=float3(0,0,1);o.Alpha=1;}
 ENDCG
 }Fallback "Diffuse"
}
