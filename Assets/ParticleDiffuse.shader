Shader "Custom/ParticleDiffuse" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.5
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
    }
    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert

        sampler2D _MainTex;
        fixed4 _Color;
        half _Metallic;
        half _Smoothness;

        struct Input {
            float2 uv_MainTex;
            fixed4 vertexColor;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertexColor = v.color;
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.vertexColor * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}