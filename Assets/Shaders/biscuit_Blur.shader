Shader "biscuit/Blur"
{
  Properties
  {
    [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    _Color ("Tint", Color) = (1,1,1,1)
    _StencilComp ("Stencil Comparison", float) = 8
    _Stencil ("Stencil ID", float) = 0
    _StencilOp ("Stencil Operation", float) = 0
    _StencilWriteMask ("Stencil Write Mask", float) = 255
    _StencilReadMask ("Stencil Read Mask", float) = 255
    _ColorMask ("Color Mask", float) = 15
    [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", float) = 0
    _BlurRange ("BlurRange", float) = 128
    _RangeScale ("RangeScale", float) = 1000
  }
  SubShader
  {
    Tags
    { 
      "CanUseSpriteAtlas" = "true"
      "IGNOREPROJECTOR" = "true"
      "PreviewType" = "Plane"
      "QUEUE" = "Transparent"
      "RenderType" = "Transparent"
    }
    Pass // ind: 1, name: 
    {
      Tags
      { 
        "CanUseSpriteAtlas" = "true"
        "IGNOREPROJECTOR" = "true"
        "PreviewType" = "Plane"
        "QUEUE" = "Transparent"
        "RenderType" = "Transparent"
      }
      ZWrite Off
      Cull Off
      Stencil
      { 
        Ref 0
        ReadMask 0
        WriteMask 0
        Pass Keep
        Fail Keep
        ZFail Keep
        PassFront Keep
        FailFront Keep
        ZFailFront Keep
        PassBack Keep
        FailBack Keep
        ZFailBack Keep
      } 
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask 0
      // m_ProgramMask = 6
      CGPROGRAM
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      
      #define CODE_BLOCK_VERTEX
      //uniform float4x4 unity_ObjectToWorld;
      //uniform float4x4 unity_MatrixVP;
      uniform float4 _Color;
      uniform float _BlurRange;
      uniform float _RangeScale;
      uniform sampler2D _MainTex;
      struct appdata_t
      {
          float4 vertex :POSITION0;
          float4 color :COLOR0;
          float2 texcoord :TEXCOORD0;
      };
      
      struct OUT_Data_Vert
      {
          float4 color :COLOR0;
          float2 texcoord :TEXCOORD0;
          float4 texcoord1 :TEXCOORD1;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float2 texcoord :TEXCOORD0;
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
          u_xlat0 = (in_v.color * _Color);
          out_v.color = u_xlat0;
          out_v.texcoord.xy = in_v.texcoord.xy;
          out_v.texcoord1 = in_v.vertex;
          return out_v;
      }
      
      #define CODE_BLOCK_FRAGMENT
      float4 u_xlat0_d;
      float4 u_xlat16_0;
      float4 u_xlat10_0;
      float4 u_xlat1_d;
      float4 u_xlat16_1;
      float4 u_xlat10_1;
      float4 u_xlat2;
      float4 u_xlat16_2;
      float4 u_xlat10_2;
      float4 u_xlat10_3;
      OUT_Data_Frag frag(v2f in_f)
      {
          OUT_Data_Frag out_f;
          u_xlat0_d.x = (_BlurRange / _RangeScale);
          u_xlat1_d = ((u_xlat0_d.xxxx * float4(-3, 0, (-2), 0)) + in_f.texcoord.xyxy);
          u_xlat10_2 = tex2D(_MainTex, u_xlat1_d.zw);
          u_xlat10_1 = tex2D(_MainTex, u_xlat1_d.xy);
          u_xlat16_2 = (u_xlat10_2 * float4(0.0615000017, 0.0615000017, 0.0615000017, 0.0615000017));
          u_xlat16_1 = ((u_xlat10_1 * float4(0.0215000007, 0.0215000007, 0.0215000007, 0.0215000007)) + u_xlat16_2);
          u_xlat2 = ((u_xlat0_d.xxxx * float4(-1, 0, 1, 0)) + in_f.texcoord.xyxy);
          u_xlat10_3 = tex2D(_MainTex, u_xlat2.xy);
          u_xlat10_2 = tex2D(_MainTex, u_xlat2.zw);
          u_xlat16_1 = ((u_xlat10_3 * float4(0.101499997, 0.101499997, 0.101499997, 0.101499997)) + u_xlat16_1);
          u_xlat16_1 = ((u_xlat10_2 * float4(0.101499997, 0.101499997, 0.101499997, 0.101499997)) + u_xlat16_1);
          u_xlat2 = ((u_xlat0_d.xxxx * float4(2, 0, 3, 0)) + in_f.texcoord.xyxy);
          u_xlat10_3 = tex2D(_MainTex, u_xlat2.xy);
          u_xlat10_2 = tex2D(_MainTex, u_xlat2.zw);
          u_xlat16_1 = ((u_xlat10_3 * float4(0.0615000017, 0.0615000017, 0.0615000017, 0.0615000017)) + u_xlat16_1);
          u_xlat16_1 = ((u_xlat10_2 * float4(0.0215000007, 0.0215000007, 0.0215000007, 0.0215000007)) + u_xlat16_1);
          u_xlat2 = ((u_xlat0_d.xxxx * float4(0, (-3), 0, (-2))) + in_f.texcoord.xyxy);
          u_xlat10_3 = tex2D(_MainTex, u_xlat2.xy);
          u_xlat10_2 = tex2D(_MainTex, u_xlat2.zw);
          u_xlat16_1 = ((u_xlat10_3 * float4(0.0215000007, 0.0215000007, 0.0215000007, 0.0215000007)) + u_xlat16_1);
          u_xlat16_1 = ((u_xlat10_2 * float4(0.0615000017, 0.0615000017, 0.0615000017, 0.0615000017)) + u_xlat16_1);
          u_xlat2 = ((u_xlat0_d.xxxx * float4(0, (-1), 0, 1)) + in_f.texcoord.xyxy);
          u_xlat0_d = ((u_xlat0_d.xxxx * float4(0, 2, 0, 3)) + in_f.texcoord.xyxy);
          u_xlat10_3 = tex2D(_MainTex, u_xlat2.xy);
          u_xlat10_2 = tex2D(_MainTex, u_xlat2.zw);
          u_xlat16_1 = ((u_xlat10_3 * float4(0.101499997, 0.101499997, 0.101499997, 0.101499997)) + u_xlat16_1);
          u_xlat10_3 = tex2D(_MainTex, in_f.texcoord.xy);
          u_xlat16_1 = ((u_xlat10_3 * float4(0.239999995, 0.239999995, 0.239999995, 0.239999995)) + u_xlat16_1);
          u_xlat16_1 = ((u_xlat10_2 * float4(0.101499997, 0.101499997, 0.101499997, 0.101499997)) + u_xlat16_1);
          u_xlat10_2 = tex2D(_MainTex, u_xlat0_d.xy);
          u_xlat10_0 = tex2D(_MainTex, u_xlat0_d.zw);
          u_xlat16_1 = ((u_xlat10_2 * float4(0.0615000017, 0.0615000017, 0.0615000017, 0.0615000017)) + u_xlat16_1);
          u_xlat16_0 = ((u_xlat10_0 * float4(0.0215000007, 0.0215000007, 0.0215000007, 0.0215000007)) + u_xlat16_1);
          out_f.color = u_xlat16_0;
          return out_f;
      }
      
      
      ENDCG
      
    } // end phase
  }
  FallBack Off
}
