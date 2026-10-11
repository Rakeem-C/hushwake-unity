Shader "Hushwake/Water"{
 Properties{_Color("Water",Color)=(.17,.38,.39,.66) _Coverage("Shore coverage",2D)="white"{}}
 SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"} ZWrite Off Cull Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade fullforwardshadows
 #pragma target 3.0
 struct Input{float3 worldPos;};fixed4 _Color;sampler2D _Coverage;
 void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float shore=saturate((tex2D(_Coverage,(p+125)/250).r-.15)/.7);clip(shore-.02);
 float a=sin(p.x*1.7+p.y*2.3+_Time.y*.8),b=sin(p.y*3.2-p.x*2.1-_Time.y*.65),ripple=sin(p.x*9.1+p.y*6.7+_Time.y*1.8)*sin(p.y*7.7-p.x*4.5-_Time.y);
 float foam=(1-shore)*.18;o.Albedo=_Color.rgb*(.88+a*.018+b*.012)+foam;
 o.Normal=normalize(float3(a*.045+ripple*.018,b*.045-ripple*.018,1));o.Smoothness=.92;o.Metallic=0;o.Alpha=_Color.a*shore;o.Emission=foam*.025;}
 ENDCG
 }Fallback "Transparent/Diffuse"
}
