Shader "Custom/CBufferTest" {
Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
SubShader {
    Tags { 
    "Queue"="Transparent" 
    "IgnoreProjector"="True" 
    "RenderType"="Transparent" 
    }
    LOD 200
    Cull Off
    blend srcAlpha one
    Pass {
      CGPROGRAM
            #pragma target 5.0
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            //The buffer containing the points we want to draw.
            StructuredBuffer<float3> buf_Points;
            StructuredBuffer<float3> buf_Colors;
            //A simple input struct for our pixel shader step containing a position.
            struct ps_input {
                float4 pos : SV_POSITION;
                half3 color:COLOR;
            };
 
            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            ps_input vert (uint id : SV_VertexID)
            {
                ps_input o;
                float3 worldPos = buf_Points[id];
                o.color = buf_Colors[id];
                o.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));

                return o;
            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (ps_input i) : COLOR
            {
                return float4(i.color,0.5);
            }
 
            ENDCG
    }
        
} 
    FallBack "Diffuse"
}