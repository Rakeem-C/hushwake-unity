Shader "Hushwake/Water"{
 Properties{_Color("Water",Color)=(.17,.38,.39,.82)}
 SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"} ZWrite Off Cull Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade fullforwardshadows
 #pragma target 3.0
 struct Input{float3 worldPos;};fixed4 _Color;
 void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float a=sin(p.x*3.4+p.y*2.7+_Time.y*1.2),b=sin(p.y*4.5-p.x*1.9-_Time.y*.9);float glint=pow(saturate(a*b),9);o.Albedo=_Color.rgb*(.85+a*.08)+glint*float3(.16,.22,.20);o.Normal=normalize(float3(a*.07,b*.07,1));o.Smoothness=.87;o.Metallic=.3;o.Alpha=.82;o.Emission=glint*.04;}
 ENDCG
 }Fallback "Transparent/Diffuse"
}
