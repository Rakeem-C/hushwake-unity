Shader "Hushwake/Landscape" {
    Properties {_Grass("Grass albedo 2K",2D)="white"{} _Rock("Rock albedo 2K",2D)="white"{} _GrassNormal("Grass normal",2D)="bump"{} _RockNormal("Rock normal",2D)="bump"{} _GrassRough("Grass roughness",2D)="white"{} _RockRough("Rock roughness",2D)="white"{}}
    SubShader {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        struct Input {float3 worldPos;float3 worldNormal; INTERNAL_DATA};
        sampler2D _Grass,_Rock,_GrassNormal,_RockNormal,_GrassRough,_RockRough,_GroundShade;
        void surf(Input IN,inout SurfaceOutputStandard o){
            float2 uv=IN.worldPos.xz*.35;float slope=1-abs(WorldNormalVector(IN,float3(0,0,1)).y);
            float patch=sin(IN.worldPos.x*.23+sin(IN.worldPos.z*.16))*sin(IN.worldPos.z*.31);
            float rock=saturate(slope*2.1-.20+patch*.16);
            float3 grass=tex2D(_Grass,uv).rgb,stone=tex2D(_Rock,uv*.8).rgb;
            float macro=.88+.12*sin(IN.worldPos.x*.051+sin(IN.worldPos.z*.047));
            float2 ground=tex2D(_GroundShade,(IN.worldPos.xz+125)/250).rg;
            o.Albedo=lerp(grass*float3(.45,1.2,.45),stone,rock)*macro*ground.g;o.Occlusion=ground.r;
            float3 gn=tex2D(_GrassNormal,uv).xyz*2-1,rn=tex2D(_RockNormal,uv*.8).xyz*2-1;
            o.Normal=normalize(lerp(float3(0,0,1),lerp(gn,rn,rock),.65));
            o.Smoothness=1-lerp(tex2D(_GrassRough,uv).r,tex2D(_RockRough,uv*.8).r,rock);o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
