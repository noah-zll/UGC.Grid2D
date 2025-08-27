using UnityEngine;
using System.Collections.Generic;

namespace UGC.Grid2D
{
    /// <summary>
    /// Unity 2D网格线渲染组件
    /// 为GameObject添加此组件即可显示可配置的网格线
    /// </summary>
    [AddComponentMenu("UGC/Grid2D/Grid2D Renderer")]
    [ExecuteInEditMode]
    public class Grid2DRenderer : MonoBehaviour
    {
        [Header("网格设置")]
        [SerializeField] private Vector2 _gridSize = Vector2.one;
        [SerializeField] private Vector2 _displayArea = Vector2.one * 100f;
        [SerializeField] private Vector2 _offset = Vector2.zero;
        
        [Header("外观设置")]
        [SerializeField] private Color _gridColor = Color.white;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private float _alpha = 1f;
        
        [Header("渐变设置")]
        [SerializeField] private bool _enableEdgeFade = true;
        [SerializeField] private float _fadeDistance = 10f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("性能设置")]
        [SerializeField] private bool _enableLOD = true;
        [SerializeField] private float _lodDistance = 50f;
        [SerializeField] private int _maxGridLines = 1000;
        [SerializeField] private bool _dynamicUpdate = true;
        
        [Header("渲染设置")]
        [SerializeField] private int _sortingOrder = 0;
        [SerializeField] private string _sortingLayerName = "Default";
        [SerializeField] private bool _antiAliasing = true;
        [SerializeField] private bool _smoothLines = true;
        
        [Header("调试设置")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _showBounds = false;
        [SerializeField] private Color _debugColor = Color.green;
        [SerializeField] private Vector2 _gridOffset = Vector2.zero;
        
        // 私有字段
        private Grid2DMesh _meshGenerator;
        private Grid2DMaterial _materialManager;
        private Material _gridMaterial;
        private Mesh _gridMesh;
        private bool _isDirty = true;
        private Camera _currentCamera;
        private Vector3 _lastCameraPosition;
        private float _lastCameraSize;
        
        // 属性访问器
        public Vector2 gridSize
        {
            get => _gridSize;
            set
            {
                if (_gridSize != value)
                {
                    _gridSize = value;
                    MarkDirty();
                    UpdateMaterialProperties();
                }
            }
        }
        
        public Vector2 displayArea
        {
            get => _displayArea;
            set
            {
                if (_displayArea != value)
                {
                    _displayArea = value;
                    MarkDirty();
                    UpdateMaterialProperties();
                }
            }
        }
        
        public Vector2 offset
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    MarkDirty();
                }
            }
        }
        
        public Color gridColor
        {
            get => _gridColor;
            set
            {
                if (_gridColor != value)
                {
                    _gridColor = value;
                    UpdateMaterialProperties();
                }
            }
        }
        
        public float lineWidth
        {
            get => _lineWidth;
            set
            {
                if (_lineWidth != value)
                {
                    _lineWidth = Mathf.Max(0.001f, value);
                    UpdateMaterialProperties();
                }
            }
        }
        
        public float alpha
        {
            get => _alpha;
            set
            {
                if (_alpha != value)
                {
                    _alpha = Mathf.Clamp01(value);
                    UpdateMaterialProperties();
                }
            }
        }
        
        public bool enableEdgeFade
        {
            get => _enableEdgeFade;
            set
            {
                if (_enableEdgeFade != value)
                {
                    _enableEdgeFade = value;
                    UpdateMaterialProperties();
                }
            }
        }
        
        public float fadeDistance
        {
            get => _fadeDistance;
            set
            {
                if (_fadeDistance != value)
                {
                    _fadeDistance = Mathf.Max(0f, value);
                    UpdateMaterialProperties();
                }
            }
        }
        
        public AnimationCurve fadeCurve
        {
            get => _fadeCurve;
            set
            {
                _fadeCurve = value ?? AnimationCurve.EaseInOut(0, 1, 1, 0);
                UpdateMaterialProperties();
            }
        }
        
        public bool enableLOD
        {
            get => _enableLOD;
            set => _enableLOD = value;
        }
        
        public float lodDistance
        {
            get => _lodDistance;
            set => _lodDistance = Mathf.Max(0f, value);
        }
        
        public int maxGridLines
        {
            get => _maxGridLines;
            set => _maxGridLines = Mathf.Max(10, value);
        }
        
        public bool dynamicUpdate
        {
            get => _dynamicUpdate;
            set => _dynamicUpdate = value;
        }
        
        public int sortingOrder
        {
            get => _sortingOrder;
            set
            {
                if (_sortingOrder != value)
                {
                    _sortingOrder = value;
                    UpdateMaterialProperties();
                }
            }
        }
        
        public string sortingLayerName
        {
            get => _sortingLayerName;
            set
            {
                if (_sortingLayerName != value)
                {
                    _sortingLayerName = value;
                    UpdateMaterialProperties();
                }
            }
        }
        
        public bool showDebugInfo
        {
            get => _showDebugInfo;
            set => _showDebugInfo = value;
        }
        
        public bool showBounds
        {
            get => _showBounds;
            set => _showBounds = value;
        }
        
        public Color debugColor
        {
            get => _debugColor;
            set => _debugColor = value;
        }
        
        public Vector2 gridOffset
        {
            get => _gridOffset;
            set
            {
                if (_gridOffset != value)
                {
                    _gridOffset = value;
                    MarkDirty();
                }
            }
        }
        
        // Unity生命周期
        private void Awake()
        {
            Initialize();
        }
        
        private void Start()
        {
            UpdateGrid();
        }
        
        private void Update()
        {
            if (_dynamicUpdate)
            {
                CheckCameraChanges();
            }
            
            if (_isDirty)
            {
                UpdateGrid();
            }
            
            // 运行时渲染
            if (Application.isPlaying)
            {
                RenderGrid();
            }
        }
        
        private void OnEnable()
        {
            Initialize();
            MarkDirty();
        }
        
        private void OnDisable()
        {
            // 清理资源
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        private void OnValidate()
        {
            // 确保参数有效性
            _gridSize.x = Mathf.Max(0.001f, _gridSize.x);
            _gridSize.y = Mathf.Max(0.001f, _gridSize.y);
            _displayArea.x = Mathf.Max(0.1f, _displayArea.x);
            _displayArea.y = Mathf.Max(0.1f, _displayArea.y);
            _lineWidth = Mathf.Max(0.001f, _lineWidth);
            _alpha = Mathf.Clamp01(_alpha);
            _fadeDistance = Mathf.Max(0f, _fadeDistance);
            _lodDistance = Mathf.Max(0f, _lodDistance);
            _maxGridLines = Mathf.Max(10, _maxGridLines);
            
            if (_fadeCurve == null)
            {
                _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            }
            
            MarkDirty();
        }
        
        // 公共方法
        /// <summary>
        /// 手动更新网格
        /// </summary>
        public void UpdateGrid()
        {
            if (!isActiveAndEnabled) return;
            
            try
            {
                GenerateGrid();
                UpdateMaterialProperties();
                _isDirty = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Grid2DRenderer UpdateGrid failed: {e.Message}", this);
            }
        }
        
        /// <summary>
        /// 标记为需要更新
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// 获取当前网格线数量
        /// </summary>
        public int GetGridLineCount()
        {
            return _meshGenerator?.GetLineCount() ?? 0;
        }
        
        /// <summary>
        /// 获取当前渲染统计信息
        /// </summary>
        public Grid2DRenderStats GetRenderStats()
        {
            int gridLines = GetGridLineCount();
            return new Grid2DRenderStats
            {
                lineCount = gridLines,
                gridLineCount = gridLines,
                vertexCount = _gridMesh?.vertexCount ?? 0,
                triangleCount = _gridMesh?.triangles?.Length / 3 ?? 0,
                lodLevel = CalculateLODLevel(),
                isLODActive = IsLODActive(),
                renderTime = 0f // TODO: 实际渲染时间测量
            };
        }
        
        // 私有方法
        private void Initialize()
        {
            if (_meshGenerator == null)
            {
                _meshGenerator = new Grid2DMesh();
            }
            
            if (_currentCamera == null)
            {
                _currentCamera = Camera.main;
            }
            
            if (_fadeCurve == null)
            {
                _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            }
            
            // 确保材质被正确初始化
            UpdateMaterialProperties();
        }
        
        private void GenerateGrid()
        {
            if (_meshGenerator == null) return;
            
            // 计算LOD级别
            float lodLevel = CalculateLODLevel();
            
            // 生成网格数据
            var gridData = new Grid2DData
            {
                gridSize = _gridSize,
                displayArea = _displayArea,
                offset = _offset,
                transform = transform,
                lodLevel = lodLevel,
                maxLines = _maxGridLines,
                gridType = GridType.Rectangle,
                alignment = GridAlignment.Center
            };
            
            _gridMesh = _meshGenerator.GenerateMesh(gridData);
            
            // 创建或更新材质
            UpdateMaterialProperties();
        }
        
        private void UpdateMaterialProperties()
        {
            var properties = new Grid2DMaterialProperties
            {
                gridSize = _gridSize,
                gridColor = _gridColor,
                lineWidth = _lineWidth,
                alpha = _alpha,
                enableEdgeFade = _enableEdgeFade,
                fadeDistance = _fadeDistance,
                fadeCurve = _fadeCurve,
                displayArea = _displayArea,
                sortingOrder = _sortingOrder,
                sortingLayerName = _sortingLayerName,
                antiAliasing = _antiAliasing,
                smoothLines = _smoothLines
            };
            
            _materialManager = Grid2DMaterial.GetOrCreate(properties);
            _gridMaterial = _materialManager.Material;
        }
        
        private void CheckCameraChanges()
        {
            if (_currentCamera == null)
            {
                _currentCamera = Camera.main;
                return;
            }
            
            Vector3 currentPos = _currentCamera.transform.position;
            float currentSize = _currentCamera.orthographicSize;
            
            bool positionChanged = Vector3.Distance(currentPos, _lastCameraPosition) > 0.1f;
            bool sizeChanged = Mathf.Abs(currentSize - _lastCameraSize) > 0.1f;
            
            if (positionChanged || sizeChanged)
            {
                _lastCameraPosition = currentPos;
                _lastCameraSize = currentSize;
                MarkDirty();
            }
        }
        
        private float CalculateLODLevel()
        {
            if (!_enableLOD || _currentCamera == null) return 1f;
            
            float distance = Vector3.Distance(_currentCamera.transform.position, transform.position);
            return Mathf.Clamp01(1f - (distance / _lodDistance));
        }
        
        private bool IsLODActive()
        {
            return _enableLOD && CalculateLODLevel() < 1f;
        }
        
        private void Cleanup()
        {
            if (_gridMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_gridMesh);
                else
                    DestroyImmediate(_gridMesh);
            }
            
            if (_gridMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(_gridMaterial);
                else
                    DestroyImmediate(_gridMaterial);
            }
            
            _meshGenerator?.Dispose();
            _materialManager?.Dispose();
        }
        
        // 渲染
        private void OnRenderObject()
        {
            // 编辑器模式下的渲染
            if (!Application.isPlaying)
            {
                RenderGrid();
            }
        }
        
        private void RenderGrid()
        {
            if (!isActiveAndEnabled) return;
            
            // 确保网格和材质都已初始化
            if (_gridMesh == null)
            {
                if (_showDebugInfo)
                    Debug.LogWarning("Grid2DRenderer: _gridMesh is null, regenerating...", this);
                GenerateGrid();
            }
            
            if (_gridMaterial == null)
            {
                if (_showDebugInfo)
                    Debug.LogWarning("Grid2DRenderer: _gridMaterial is null, updating properties...", this);
                UpdateMaterialProperties();
            }
            
            if (_gridMesh == null || _gridMaterial == null)
            {
                if (_showDebugInfo)
                    Debug.LogError("Grid2DRenderer: Failed to initialize mesh or material", this);
                return;
            }
            
            // 使用Graphics.DrawMesh进行渲染
            Graphics.DrawMesh(_gridMesh, transform.localToWorldMatrix, _gridMaterial, gameObject.layer);
        }
        
        /// <summary>
        /// 重新生成网格
        /// </summary>
        public void RegenerateGrid()
        {
            MarkDirty();
            UpdateGrid();
        }
        
        /// <summary>
        /// 重置所有设置为默认值
        /// </summary>
        public void ResetToDefault()
        {
            _gridSize = Vector2.one;
            _displayArea = Vector2.one * 100f;
            _offset = Vector2.zero;
            _gridColor = Color.white;
            _lineWidth = 0.1f;
            _alpha = 1f;
            _enableEdgeFade = true;
            _fadeDistance = 10f;
            _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            _enableLOD = true;
            _lodDistance = 50f;
            _maxGridLines = 1000;
            _dynamicUpdate = true;
            _sortingOrder = 0;
            _sortingLayerName = "Default";
            _antiAliasing = true;
            _smoothLines = true;
            _showDebugInfo = false;
            _showBounds = false;
            _debugColor = Color.green;
            _gridOffset = Vector2.zero;
            
            MarkDirty();
            UpdateGrid();
        }
        
        // Gizmos绘制
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Vector3 center = new Vector3(_offset.x, _offset.y, 0);
            Vector3 size = new Vector3(_displayArea.x, _displayArea.y, 0);
            
            Gizmos.DrawWireCube(center, size);
        }
    }
    
    // 辅助数据结构
    [System.Serializable]
    public struct Grid2DRenderStats
    {
        public int lineCount;
        public int gridLineCount;
        public int vertexCount;
        public int triangleCount;
        public float lodLevel;
        public bool isLODActive;
        public float renderTime;
    }
}