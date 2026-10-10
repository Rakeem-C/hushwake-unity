Shader "Hushwake/Landscape" {
    Properties { _Tint ("Tint", Color) = (1,1,1,1) }
    SubShader {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 3.0
        struct Input { float4 color : COLOR; };
        fixed4 _Tint;
        void surf(Input IN, inout SurfaceOutput o) { o.Albedo = IN.color.rgb * _Tint.rgb; o.Alpha = 1; }
        ENDCG
    }
    Fallback "Diffuse"
}
