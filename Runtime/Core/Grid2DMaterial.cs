using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace UGC.Grid2D
{
    /// <summary>
    /// Grid2D材质管理器
    /// 负责创建、更新和管理网格渲染材质
    /// </summary>
    public class Grid2DMaterial
    {
        #region Constants
        
        private const string SHADER_NAME = "UGC/Grid2D/Grid2D";
        private const string FALLBACK_SHADER_NAME = "Sprites/Default";
        
        // Shader属性ID
        private static readonly int GridSizeID = Shader.PropertyToID("_GridSize");
        private static readonly int GridColorID = Shader.PropertyToID("_GridColor");
        private static readonly int LineWidthID = Shader.PropertyToID("_LineWidth");
        private static readonly int AlphaID = Shader.PropertyToID("_Alpha");
        private static readonly int EnableEdgeFadeID = Shader.PropertyToID("_EnableEdgeFade");
        private static readonly int FadeDistanceID = Shader.PropertyToID("_FadeDistance");
        private static readonly int DisplayAreaID = Shader.PropertyToID("_DisplayArea");
        private static readonly int FadeTextureID = Shader.PropertyToID("_FadeTexture");
        private static readonly int AntiAliasingID = Shader.PropertyToID("_AntiAliasing");
        private static readonly int SmoothLinesID = Shader.PropertyToID("_SmoothLines");
        
        // Shader关键字
        private const string EDGE_FADE_KEYWORD = "_EDGE_FADE_ON";
        private const string ANTI_ALIASING_KEYWORD = "_ANTI_ALIASING_ON";
        private const string SMOOTH_LINES_KEYWORD = "_SMOOTH_LINES_ON";
        
        #endregion
        
        #region Fields
        
        private Material _material;
        private Shader _shader;
        private Texture2D _fadeTexture;
        private Grid2DMaterialProperties _lastProperties;
        private bool _isDirty = true;
        
        // 材质池
        private static readonly Dictionary<int, Grid2DMaterial> MaterialPool = new Dictionary<int, Grid2DMaterial>();
        private static int _nextInstanceId = 0;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 材质实例
        /// </summary>
        public Material Material
        {
            get
            {
                if (_material == null)
                {
                    CreateMaterial();
                }
                return _material;
            }
        }
        
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => _material != null && _shader != null;
        
        /// <summary>
        /// 实例ID
        /// </summary>
        public int InstanceId { get; private set; }
        
        #endregion
        
        #region Constructor
        
        private Grid2DMaterial()
        {
            InstanceId = _nextInstanceId++;
            LoadShader();
            CreateMaterial();
        }
        
        #endregion
        
        #region Static Methods
        
        /// <summary>
        /// 获取或创建材质实例
        /// </summary>
        public static Grid2DMaterial GetOrCreate(Grid2DMaterialProperties properties)
        {
            int hash = properties.GetHashCode();
            
            if (MaterialPool.TryGetValue(hash, out Grid2DMaterial material))
            {
                material.UpdateProperties(properties);
                return material;
            }
            
            material = new Grid2DMaterial();
            material.UpdateProperties(properties);
            MaterialPool[hash] = material;
            
            return material;
        }
        
        /// <summary>
        /// 清理材质池
        /// </summary>
        public static void ClearPool()
        {
            foreach (var kvp in MaterialPool)
            {
                kvp.Value.Dispose();
            }
            MaterialPool.Clear();
        }
        
        /// <summary>
        /// 移除未使用的材质
        /// </summary>
        public static void CleanupUnused()
        {
            var toRemove = new List<int>();
            
            foreach (var kvp in MaterialPool)
            {
                if (kvp.Value._material == null)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (int key in toRemove)
            {
                MaterialPool.Remove(key);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 更新材质属性
        /// </summary>
        public void UpdateProperties(Grid2DMaterialProperties properties)
        {
            if (_lastProperties.Equals(properties) && !_isDirty)
                return;
                
            _lastProperties = properties;
            _isDirty = false;
            
            if (!IsValid)
            {
                CreateMaterial();
                if (!IsValid) return;
            }
            
            // 更新基础属性
            _material.SetVector(GridSizeID, new Vector4(properties.gridSize.x, properties.gridSize.y, 0, 0));
            _material.SetColor(GridColorID, properties.gridColor);
            _material.SetFloat(LineWidthID, properties.lineWidth);
            _material.SetFloat(AlphaID, properties.alpha);
            
            // 更新渐变属性
            _material.SetFloat(EnableEdgeFadeID, properties.enableEdgeFade ? 1.0f : 0.0f);
            _material.SetFloat(FadeDistanceID, properties.fadeDistance);
            _material.SetVector(DisplayAreaID, new Vector4(properties.displayArea.x, properties.displayArea.y, 0, 0));
            
            // 更新渐变纹理
            UpdateFadeTexture(properties);
            
            // 更新渲染属性
            _material.SetFloat(AntiAliasingID, properties.antiAliasing ? 1.0f : 0.0f);
            _material.SetFloat(SmoothLinesID, properties.smoothLines ? 1.0f : 0.0f);
            
            // 更新关键字
            UpdateShaderKeywords(properties);
            
            // 更新渲染状态
            UpdateRenderState(properties);
        }
        
        /// <summary>
        /// 标记为脏数据
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_material != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(_material);
                }
                else
                {
                    Object.DestroyImmediate(_material);
                }
                _material = null;
            }
            
            if (_fadeTexture != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(_fadeTexture);
                }
                else
                {
                    Object.DestroyImmediate(_fadeTexture);
                }
                _fadeTexture = null;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 加载Shader
        /// </summary>
        private void LoadShader()
        {
            _shader = Shader.Find(SHADER_NAME);
            
            if (_shader == null)
            {
                Debug.LogWarning($"[Grid2DMaterial] 无法找到Shader: {SHADER_NAME}，使用备用Shader");
                _shader = Shader.Find(FALLBACK_SHADER_NAME);
            }
            
            if (_shader == null)
            {
                Debug.LogError($"[Grid2DMaterial] 无法找到备用Shader: {FALLBACK_SHADER_NAME}");
            }
        }
        
        /// <summary>
        /// 创建材质
        /// </summary>
        private void CreateMaterial()
        {
            if (_shader == null)
            {
                LoadShader();
                if (_shader == null) return;
            }
            
            _material = new Material(_shader)
            {
                name = $"Grid2D Material (Instance {InstanceId})",
                hideFlags = HideFlags.DontSave
            };
            
            _isDirty = true;
        }
        
        /// <summary>
        /// 更新渐变纹理
        /// </summary>
        private void UpdateFadeTexture(Grid2DMaterialProperties properties)
        {
            if (!properties.enableEdgeFade)
            {
                if (_fadeTexture != null)
                {
                    _material.SetTexture(FadeTextureID, Texture2D.whiteTexture);
                }
                return;
            }
            
            // 生成渐变纹理
            if (_fadeTexture == null)
            {
                _fadeTexture = new Texture2D(256, 1, TextureFormat.Alpha8, false)
                {
                    name = $"Grid2D Fade Texture (Instance {InstanceId})",
                    hideFlags = HideFlags.DontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }
            
            // 填充纹理数据
            var pixels = new Color32[256];
            for (int i = 0; i < 256; i++)
            {
                float t = i / 255.0f;
                float alpha = properties.fadeCurve.Evaluate(t);
                byte alphaValue = (byte)(alpha * 255);
                pixels[i] = new Color32(255, 255, 255, alphaValue);
            }
            
            _fadeTexture.SetPixels32(pixels);
            _fadeTexture.Apply();
            
            _material.SetTexture(FadeTextureID, _fadeTexture);
        }
        
        /// <summary>
        /// 更新Shader关键字
        /// </summary>
        private void UpdateShaderKeywords(Grid2DMaterialProperties properties)
        {
            // 边缘渐变
            if (properties.enableEdgeFade)
                _material.EnableKeyword(EDGE_FADE_KEYWORD);
            else
                _material.DisableKeyword(EDGE_FADE_KEYWORD);
            
            // 抗锯齿
            if (properties.antiAliasing)
                _material.EnableKeyword(ANTI_ALIASING_KEYWORD);
            else
                _material.DisableKeyword(ANTI_ALIASING_KEYWORD);
            
            // 平滑线条
            if (properties.smoothLines)
                _material.EnableKeyword(SMOOTH_LINES_KEYWORD);
            else
                _material.DisableKeyword(SMOOTH_LINES_KEYWORD);
        }
        
        /// <summary>
        /// 更新渲染状态
        /// </summary>
        private void UpdateRenderState(Grid2DMaterialProperties properties)
        {
            // 设置渲染队列
            _material.renderQueue = (int)RenderQueue.Transparent + properties.sortingOrder;
            
            // 设置混合模式
            _material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            
            // 设置深度测试
            _material.SetInt("_ZWrite", 0);
            _material.SetInt("_ZTest", (int)CompareFunction.LessEqual);
            
            // 设置剔除模式
            _material.SetInt("_Cull", (int)CullMode.Off);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 材质属性缓存
    /// </summary>
    public struct Grid2DMaterialCache
    {
        public Grid2DMaterial material;
        public Grid2DMaterialProperties properties;
        public float lastUpdateTime;
        
        public bool IsValid => material != null && material.IsValid;
        public bool IsExpired(float currentTime, float expireTime) => currentTime - lastUpdateTime > expireTime;
    }
}