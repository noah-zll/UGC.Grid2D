Shader "UGC/Grid2D/Grid2D"
{
    Properties
    {
        [Header(Grid Settings)]
        _GridSize ("Grid Size", Vector) = (1, 1, 0, 0)
        _GridColor ("Grid Color", Color) = (1, 1, 1, 1)
        _LineWidth ("Line Width", Float) = 0.02
        _Alpha ("Alpha", Range(0, 1)) = 1
        
        [Header(Fade Settings)]
        _EnableEdgeFade ("Enable Edge Fade", Float) = 1
        _FadeDistance ("Fade Distance", Float) = 10
        _DisplayArea ("Display Area", Vector) = (100, 100, 0, 0)
        _FadeTexture ("Fade Texture", 2D) = "white" {}
        
        [Header(Rendering)]
        _ZWrite ("Z Write", Float) = 0
        _ZTest ("Z Test", Float) = 4
        _Cull ("Cull", Float) = 0
        
        [Header(Anti Aliasing)]
        _AntiAliasing ("Anti Aliasing", Float) = 1
        _SmoothLines ("Smooth Lines", Float) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        
        LOD 100
        
        Pass
        {
            Name "Grid2D"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma multi_compile _ _EDGE_FADE_ON
            #pragma multi_compile _ _ANTI_ALIASING_ON
            #pragma multi_compile _ _SMOOTH_LINES_ON
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float4 color : COLOR;
                UNITY_FOG_COORDS(3)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // Properties
            float4 _GridSize;
            float4 _GridColor;
            float _LineWidth;
            float _Alpha;
            float _EnableEdgeFade;
            float _FadeDistance;
            float4 _DisplayArea;
            sampler2D _FadeTexture;
            float4 _FadeTexture_ST;
            float _AntiAliasing;
            float _SmoothLines;
            
            // Utility functions
            float2 GetGridUV(float2 worldPos)
            {
                return worldPos / _GridSize.xy;
            }
            
            float GetGridLine(float2 gridUV, float lineWidth)
            {
                // 使用frac获取网格内的相对位置
                float2 grid = frac(gridUV);
                
                // 计算到网格线的距离
                float2 gridDist = min(grid, 1.0 - grid);
                
                // 调整线宽计算，使其更合理
                float lineThickness = lineWidth * 0.5; // 使用更合理的倍数
                
                // 分别检测水平和垂直线
                float lineX = step(gridDist.x, lineThickness);
                float lineY = step(gridDist.y, lineThickness);
                
                // 返回任一方向有线就显示
                return max(lineX, lineY);
            }
            
            float GetEdgeFade(float2 worldPos)
            {
                if (_EnableEdgeFade < 0.5) return 1.0;
                
                // 使用物体的世界坐标原点作为渐变中心
                float2 center = float2(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3]);
                float2 halfArea = _DisplayArea.xy * 0.5;
                float2 distanceFromEdge = halfArea - abs(worldPos - center);
                
                float minDistance = min(distanceFromEdge.x, distanceFromEdge.y);
                float fadeAmount = saturate(minDistance / _FadeDistance);
                
                // 简化版本：直接使用线性渐变，不依赖纹理
                return fadeAmount;
            }
            
            float GetAntiAliasing(float2 screenPos)
            {
                #ifdef _ANTI_ALIASING_ON
                    if (_AntiAliasing < 0.5) return 1.0;
                    
                    float2 pixelSize = fwidth(screenPos);
                    float aa = length(pixelSize) * 0.5;
                    return smoothstep(0.0, aa, 1.0);
                #else
                    return 1.0;
                #endif
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                o.color = v.color;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Get world position
                float2 worldPos = i.worldPos.xy;
                
                // Calculate grid UV
                float2 gridUV = GetGridUV(worldPos);
                
                // Get grid line intensity
                float gridLine = GetGridLine(gridUV, _LineWidth);
                
                // 简化版本：先只使用gridLine测试
                float finalAlpha = gridLine * _Alpha;
                
                // 如果启用了边缘渐变，应用它
                if (_EnableEdgeFade > 0.5)
                {
                    float edgeFade = GetEdgeFade(worldPos);
                    finalAlpha *= edgeFade;
                }
                
                // Final color
                fixed4 col = _GridColor;
                col.a = finalAlpha;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDHLSL
        }
    }
    
    SubShader
    {
        // Fallback for older hardware
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        
        LOD 50
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert_simple
            #pragma fragment frag_simple
            #include "UnityCG.cginc"
            
            struct appdata_simple
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
            
            struct v2f_simple
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            float4 _GridColor;
            float _Alpha;
            
            v2f_simple vert_simple (appdata_simple v)
            {
                v2f_simple o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag_simple (v2f_simple i) : SV_Target
            {
                fixed4 col = _GridColor * i.color;
                col.a *= _Alpha;
                return col;
            }
             ENDHLSL
        }
    }
    
    Fallback "Sprites/Default"
    CustomEditor "UGC.Grid2D.Editor.Grid2DShaderGUI"
}