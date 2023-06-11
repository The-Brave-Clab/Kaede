Shader "Custom/ColorPalette"
{
  Properties
  {
    _MainTex ("MainTex", 2D) = "white" {}
    _Ramp ("Ramp", 2D) = "white" {}
    _OpacityAmount ("OpacityAmount", float) = 1
    [HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
  }
  SubShader
  {
    Tags
    { 
      "IGNOREPROJECTOR" = "true"
      "QUEUE" = "Transparent"
      "RenderType" = "Transparent"
    }
    Pass // ind: 1, name: FORWARD
    {
      Name "FORWARD"
      Tags
      { 
        "IGNOREPROJECTOR" = "true"
        "LIGHTMODE" = "FORWARDBASE"
        "QUEUE" = "Transparent"
        "RenderType" = "Transparent"
        "SHADOWSUPPORT" = "true"
      }
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      // m_ProgramMask = 6
      CGPROGRAM
      #pragma multi_compile DIRECTIONAL
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      
      #define CODE_BLOCK_VERTEX
      //uniform float4x4 unity_ObjectToWorld;
      //uniform float4x4 unity_MatrixVP;
      uniform float4 _MainTex_ST;
      uniform float4 _Ramp_ST;
      uniform float _OpacityAmount;
      uniform sampler2D _MainTex;
      uniform sampler2D _Ramp;
      struct appdata_t
      {
          float4 vertex :POSITION0;
          float2 texcoord :TEXCOORD0;
          float4 color :COLOR0;
      };
      
      struct OUT_Data_Vert
      {
          float2 texcoord :TEXCOORD0;
          float4 color :COLOR0;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float2 texcoord :TEXCOORD0;
          float4 color :COLOR0;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target0;
      };
      
      float4 u_xlat0;
      float4 u_xlat1;
      OUT_Data_Vert vert(appdata_t in_v)
      {
          OUT_Data_Vert out_v;
          out_v.vertex = UnityObjectToClipPos(in_v.vertex);
          out_v.texcoord.xy = in_v.texcoord.xy;
          out_v.color = in_v.color;
          return out_v;
      }
      
      #define CODE_BLOCK_FRAGMENT
      float2 u_xlat0_d;
      float u_xlat16_0;
      float3 u_xlat10_0;
      float2 u_xlat1_d;
      OUT_Data_Frag frag(v2f in_f)
      {
          OUT_Data_Frag out_f;
          u_xlat0_d.xy = TRANSFORM_TEX(in_f.texcoord.xy, _MainTex);
          u_xlat10_0.x = tex2D(_MainTex, u_xlat0_d.xy).x;
          u_xlat16_0 = (u_xlat10_0.x + 0.5);
          u_xlat16_0 = ((u_xlat16_0 * 2) + (-1));
          u_xlat16_0 = clamp(u_xlat16_0, 0, 1);
          u_xlat1_d.x = (u_xlat16_0 * _Ramp_ST.x);
          u_xlat0_d.x = (u_xlat16_0 * _OpacityAmount);
          out_f.color.w = (u_xlat0_d.x * in_f.color.w);
          u_xlat1_d.y = 0;
          u_xlat0_d.xy = (u_xlat1_d.xy + _Ramp_ST.zw);
          u_xlat10_0.xyz = tex2D(_Ramp, u_xlat0_d.xy).xyz;
          out_f.color.xyz = (u_xlat10_0.xyz * in_f.color.xyz);
          return out_f;
      }
      
      
      ENDCG
      
    } // end phase
  }
  FallBack "Diffuse"
}
