using UnityEngine;

namespace UGC.Grid2D
{
    /// <summary>
    /// 网格渲染模式
    /// </summary>
    public enum GridRenderMode
    {
        /// <summary>线条模式</summary>
        Lines,
        /// <summary>点模式</summary>
        Points,
        /// <summary>混合模式</summary>
        Mixed
    }
    
    /// <summary>
    /// 网格对齐方式
    /// </summary>
    public enum GridAlignment
    {
        /// <summary>中心对齐</summary>
        Center,
        /// <summary>左下角对齐</summary>
        BottomLeft,
        /// <summary>左上角对齐</summary>
        TopLeft,
        /// <summary>右下角对齐</summary>
        BottomRight,
        /// <summary>右上角对齐</summary>
        TopRight
    }
    
    /// <summary>
    /// 网格类型
    /// </summary>
    public enum GridType
    {
        /// <summary>正方形网格</summary>
        Square,
        /// <summary>矩形网格</summary>
        Rectangle,
        /// <summary>六边形网格</summary>
        Hexagon,
        /// <summary>三角形网格</summary>
        Triangle
    }
    
    /// <summary>
    /// Grid2D配置设置
    /// </summary>
    [System.Serializable]
    public class Grid2DSettings
    {
        [Header("基础设置")]
        [Tooltip("网格渲染模式")]
        public GridRenderMode renderMode = GridRenderMode.Lines;
        
        [Tooltip("网格类型")]
        public GridType gridType = GridType.Square;
        
        [Tooltip("网格对齐方式")]
        public GridAlignment alignment = GridAlignment.Center;
        
        [Header("更新设置")]
        [Tooltip("是否启用动态更新")]
        public bool dynamicUpdate = true;
        
        [Tooltip("更新频率（秒）")]
        [Range(0.01f, 1f)]
        public float updateInterval = 0.1f;
        
        [Header("剔除设置")]
        [Tooltip("剔除遮罩")]
        public LayerMask cullingMask = -1;
        
        [Tooltip("视锥剔除")]
        public bool frustumCulling = true;
        
        [Tooltip("距离剔除")]
        public bool distanceCulling = true;
        
        [Tooltip("最大渲染距离")]
        public float maxRenderDistance = 1000f;
        
        [Header("质量设置")]
        [Tooltip("抗锯齿")]
        public bool antiAliasing = true;
        
        [Tooltip("线条平滑")]
        public bool smoothLines = true;
        
        [Tooltip("渲染质量")]
        [Range(0.1f, 2f)]
        public float renderQuality = 1f;
        
        [Header("调试设置")]
        [Tooltip("显示调试信息")]
        public bool showDebugInfo = false;
        
        [Tooltip("显示性能统计")]
        public bool showPerformanceStats = false;
        
        [Tooltip("显示边界框")]
        public bool showBounds = false;
        
        /// <summary>
        /// 创建默认设置
        /// </summary>
        public static Grid2DSettings CreateDefault()
        {
            return new Grid2DSettings
            {
                renderMode = GridRenderMode.Lines,
                gridType = GridType.Square,
                alignment = GridAlignment.Center,
                dynamicUpdate = true,
                updateInterval = 0.1f,
                cullingMask = -1,
                frustumCulling = true,
                distanceCulling = true,
                maxRenderDistance = 1000f,
                antiAliasing = true,
                smoothLines = true,
                renderQuality = 1f,
                showDebugInfo = false,
                showPerformanceStats = false,
                showBounds = false
            };
        }
        
        /// <summary>
        /// 创建高性能设置
        /// </summary>
        public static Grid2DSettings CreateHighPerformance()
        {
            return new Grid2DSettings
            {
                renderMode = GridRenderMode.Lines,
                gridType = GridType.Square,
                alignment = GridAlignment.Center,
                dynamicUpdate = false,
                updateInterval = 0.2f,
                cullingMask = -1,
                frustumCulling = true,
                distanceCulling = true,
                maxRenderDistance = 500f,
                antiAliasing = false,
                smoothLines = false,
                renderQuality = 0.5f,
                showDebugInfo = false,
                showPerformanceStats = false,
                showBounds = false
            };
        }
        
        /// <summary>
        /// 创建高质量设置
        /// </summary>
        public static Grid2DSettings CreateHighQuality()
        {
            return new Grid2DSettings
            {
                renderMode = GridRenderMode.Lines,
                gridType = GridType.Square,
                alignment = GridAlignment.Center,
                dynamicUpdate = true,
                updateInterval = 0.05f,
                cullingMask = -1,
                frustumCulling = true,
                distanceCulling = false,
                maxRenderDistance = 2000f,
                antiAliasing = true,
                smoothLines = true,
                renderQuality = 2f,
                showDebugInfo = false,
                showPerformanceStats = false,
                showBounds = false
            };
        }
        
        /// <summary>
        /// 验证设置有效性
        /// </summary>
        public void Validate()
        {
            updateInterval = Mathf.Clamp(updateInterval, 0.01f, 1f);
            maxRenderDistance = Mathf.Max(0f, maxRenderDistance);
            renderQuality = Mathf.Clamp(renderQuality, 0.1f, 2f);
        }
        
        /// <summary>
        /// 复制设置
        /// </summary>
        public Grid2DSettings Clone()
        {
            return new Grid2DSettings
            {
                renderMode = this.renderMode,
                gridType = this.gridType,
                alignment = this.alignment,
                dynamicUpdate = this.dynamicUpdate,
                updateInterval = this.updateInterval,
                cullingMask = this.cullingMask,
                frustumCulling = this.frustumCulling,
                distanceCulling = this.distanceCulling,
                maxRenderDistance = this.maxRenderDistance,
                antiAliasing = this.antiAliasing,
                smoothLines = this.smoothLines,
                renderQuality = this.renderQuality,
                showDebugInfo = this.showDebugInfo,
                showPerformanceStats = this.showPerformanceStats,
                showBounds = this.showBounds
            };
        }
    }
    
    /// <summary>
    /// 网格数据结构
    /// </summary>
    [System.Serializable]
    public struct Grid2DData
    {
        public Vector2 gridSize;
        public Vector2 displayArea;
        public Vector2 offset;
        public Transform transform;
        public float lodLevel;
        public int maxLines;
        public GridType gridType;
        public GridAlignment alignment;
        
        /// <summary>
        /// 获取网格边界
        /// </summary>
        public Bounds GetBounds()
        {
            Vector3 center = new Vector3(offset.x, offset.y, 0);
            Vector3 size = new Vector3(displayArea.x, displayArea.y, 0);
            
            if (transform != null)
            {
                center = transform.TransformPoint(center);
                size = transform.TransformVector(size);
            }
            
            return new Bounds(center, size);
        }
        
        /// <summary>
        /// 计算网格线数量
        /// </summary>
        public Vector2Int CalculateLineCount()
        {
            if (gridSize.x <= 0 || gridSize.y <= 0) return Vector2Int.zero;
            
            // 计算网格格子数量，然后加1得到线条数量
            // 例如：10x10的格子需要11条垂直线和11条水平线
            int xGrids = Mathf.RoundToInt(displayArea.x / gridSize.x);
            int yGrids = Mathf.RoundToInt(displayArea.y / gridSize.y);
            
            int xLines = xGrids + 1;
            int yLines = yGrids + 1;
            
            // 应用LOD
            xLines = Mathf.RoundToInt(xLines * lodLevel);
            yLines = Mathf.RoundToInt(yLines * lodLevel);
            
            // 限制最大数量
            int totalLines = xLines + yLines;
            if (totalLines > maxLines)
            {
                float scale = (float)maxLines / totalLines;
                xLines = Mathf.RoundToInt(xLines * scale);
                yLines = Mathf.RoundToInt(yLines * scale);
            }
            
            return new Vector2Int(Mathf.Max(2, xLines), Mathf.Max(2, yLines));
        }
    }
    
    /// <summary>
    /// 材质属性数据
    /// </summary>
    [System.Serializable]
    public struct Grid2DMaterialProperties
    {
        public Vector2 gridSize;
        public Color gridColor;
        public float lineWidth;
        public float alpha;
        public bool enableEdgeFade;
        public float fadeDistance;
        public AnimationCurve fadeCurve;
        public Vector2 displayArea;
        public int sortingOrder;
        public string sortingLayerName;
        public bool antiAliasing;
        public bool smoothLines;
        
        /// <summary>
        /// 获取最终颜色（包含透明度）
        /// </summary>
        public Color GetFinalColor()
        {
            Color color = gridColor;
            color.a *= alpha;
            return color;
        }
        
        /// <summary>
        /// 获取渐变纹理
        /// </summary>
        public Texture2D GetFadeTexture(int resolution = 256)
        {
            if (fadeCurve == null) return null;
            
            Texture2D texture = new Texture2D(resolution, 1, TextureFormat.Alpha8, false);
            
            for (int i = 0; i < resolution; i++)
            {
                float t = (float)i / (resolution - 1);
                float alpha = fadeCurve.Evaluate(t);
                texture.SetPixel(i, 0, new Color(1, 1, 1, alpha));
            }
            
            texture.Apply();
            return texture;
        }
    }
}