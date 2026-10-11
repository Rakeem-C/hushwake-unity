Shader "Hushwake/Landscape" {
    Properties { _Tint ("Tint", Color) = (1,1,1,1) _Substrate("Original substrate",2D)="white"{} }
    SubShader {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        struct Input { float4 color : COLOR; float3 worldPos; };
        fixed4 _Tint;
        sampler2D _Substrate;
        // Authored meadow swatches are display colors; vertex inputs do not receive Unity's material color conversion.
        void surf(Input IN, inout SurfaceOutputStandard o) { float2 uv=IN.worldPos.xz*.8;float4 soil=tex2D(_Substrate,uv);float grains=tex2D(_Substrate,uv*4).r;float mottling=.92+.08*sin(IN.worldPos.x*.73+sin(IN.worldPos.z*.41));o.Albedo=GammaToLinearSpace(IN.color.rgb)*_Tint.rgb*mottling*(.79+soil.r*.37+grains*.10);o.Smoothness=.08;o.Normal=normalize(float3((soil.g-.5)*.14,(soil.b-.5)*.14,1));o.Alpha = 1; }
        ENDCG
    }
    Fallback "Diffuse"
}
