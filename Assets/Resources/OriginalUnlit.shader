Shader "Hushwake/OriginalUnlit" {
 Properties { _Color("Tint",Color)=(1,1,1,1) _MainTex("Albedo",2D)="white"{} _NormalMap("Normal",2D)="bump"{} _RoughnessMap("Roughness",2D)="white"{} _Glossiness("Smoothness",Range(0,1))=.15 _Metallic("Metallic",Range(0,1))=0 _EmissionColor("Emission",Color)=(0,0,0,0) _Cutoff("Cutout",Range(0,1))=0 _HasNormal("Normal present",Float)=0 _HasRoughness("Roughness present",Float)=0 _Cull("Cull",Float)=2 _Wind("Leaf movement",Float)=0 }
 SubShader { Tags {"RenderType"="Opaque"} Cull [_Cull]
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct varying {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_FOG_COORDS(1)};
 sampler2D _MainTex;float4 _MainTex_ST,_Color,_EmissionColor;float _Cutoff,_Wind;
 varying vert(app i){varying o;float3 w=mul(unity_ObjectToWorld,i.vertex).xyz;i.vertex.x+=sin(_Time.y+w.x*.19+w.z*.16)*max(0,i.vertex.y-2)*.006*_Wind;o.position=UnityObjectToClipPos(i.vertex);o.uv=TRANSFORM_TEX(i.uv,_MainTex);o.color=i.color;UNITY_TRANSFER_FOG(o,o.position);return o;}
 fixed4 frag(varying i):SV_Target{fixed4 tex=tex2D(_MainTex,i.uv);clip(tex.a-_Cutoff);fixed4 color=float4(tex.rgb*_Color.rgb*i.color.rgb+_EmissionColor.rgb,1);UNITY_APPLY_FOG(i.fogCoord,color);return color;}
 ENDCG } } Fallback Off
}
