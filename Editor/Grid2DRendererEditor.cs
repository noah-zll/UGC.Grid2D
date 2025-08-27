using UnityEngine;
using UnityEditor;
using UGC.Grid2D;

namespace UGC.Grid2D.Editor
{
    /// <summary>
    /// Grid2DRenderer组件的自定义编辑器
    /// </summary>
    [CustomEditor(typeof(Grid2DRenderer))]
    [CanEditMultipleObjects]
    public class Grid2DRendererEditor : UnityEditor.Editor
    {
        #region SerializedProperties
        
        // 网格设置
        private SerializedProperty _gridSize;
        private SerializedProperty _displayArea;
        private SerializedProperty _gridOffset;
        private SerializedProperty _gridType;
        private SerializedProperty _alignmentMode;
        
        // 外观设置
        private SerializedProperty _gridColor;
        private SerializedProperty _lineWidth;
        private SerializedProperty _alpha;
        
        // 渐变设置
        private SerializedProperty _enableEdgeFade;
        private SerializedProperty _fadeDistance;
        private SerializedProperty _fadeCurve;
        
        // 性能设置
        private SerializedProperty _enableLOD;
        private SerializedProperty _maxGridLines;
        private SerializedProperty _enableDynamicUpdate;
        private SerializedProperty _updateFrequency;
        
        // 渲染设置
        private SerializedProperty _sortingLayerID;
        private SerializedProperty _sortingLayerName;
        private SerializedProperty _sortingOrder;
        private SerializedProperty _enableAntiAliasing;
        private SerializedProperty _enableSmoothLines;
        
        // 调试设置
        private SerializedProperty _showDebugInfo;
        private SerializedProperty _showBounds;
        private SerializedProperty _debugColor;
        
        #endregion
        
        #region Foldout States
        
        private bool _gridSettingsFoldout = true;
        private bool _appearanceSettingsFoldout = true;
        private bool _fadeSettingsFoldout = true;
        private bool _performanceSettingsFoldout = false;
        private bool _renderingSettingsFoldout = false;
        private bool _debugSettingsFoldout = false;
        
        #endregion
        
        #region Unity Callbacks
        
        private void OnEnable()
        {
            FindProperties();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawHeader();
            
            EditorGUILayout.Space();
            
            DrawGridSettings();
            DrawAppearanceSettings();
            DrawFadeSettings();
            DrawPerformanceSettings();
            DrawRenderingSettings();
            DrawDebugSettings();
            
            EditorGUILayout.Space();
            
            DrawStats();
            DrawButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        #endregion
        
        #region Private Methods
        
        private void FindProperties()
        {
            // 网格设置
            _gridSize = serializedObject.FindProperty("_gridSize");
            _displayArea = serializedObject.FindProperty("_displayArea");
            _gridOffset = serializedObject.FindProperty("_gridOffset");
            // 注意：_gridType 和 _alignmentMode 在当前版本中不存在
            _gridType = serializedObject.FindProperty("_gridType");
            _alignmentMode = serializedObject.FindProperty("_alignmentMode");
            
            // 外观设置
            _gridColor = serializedObject.FindProperty("_gridColor");
            _lineWidth = serializedObject.FindProperty("_lineWidth");
            _alpha = serializedObject.FindProperty("_alpha");
            
            // 渐变设置
            _enableEdgeFade = serializedObject.FindProperty("_enableEdgeFade");
            _fadeDistance = serializedObject.FindProperty("_fadeDistance");
            _fadeCurve = serializedObject.FindProperty("_fadeCurve");
            
            // 性能设置
            _enableLOD = serializedObject.FindProperty("_enableLOD");
            _maxGridLines = serializedObject.FindProperty("_maxGridLines");
            _enableDynamicUpdate = serializedObject.FindProperty("_dynamicUpdate");
            _updateFrequency = serializedObject.FindProperty("_lodDistance");
            
            // 渲染设置
            _sortingLayerID = serializedObject.FindProperty("_sortingLayerID");
            _sortingLayerName = serializedObject.FindProperty("_sortingLayerName");
            _sortingOrder = serializedObject.FindProperty("_sortingOrder");
            _enableAntiAliasing = serializedObject.FindProperty("_antiAliasing");
            _enableSmoothLines = serializedObject.FindProperty("_smoothLines");
            
            // 调试设置
            _showDebugInfo = serializedObject.FindProperty("_showDebugInfo");
            _showBounds = serializedObject.FindProperty("_showBounds");
            _debugColor = serializedObject.FindProperty("_debugColor");
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal("box");
            
            GUILayout.Label("Grid2D Renderer", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            var renderer = target as Grid2DRenderer;
            if (renderer != null)
            {
                GUI.enabled = renderer.enabled;
                GUILayout.Label(renderer.enabled ? "Active" : "Inactive", 
                    renderer.enabled ? EditorStyles.miniLabel : EditorStyles.centeredGreyMiniLabel);
                GUI.enabled = true;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawGridSettings()
        {
            _gridSettingsFoldout = EditorGUILayout.Foldout(_gridSettingsFoldout, "Grid Settings", true);
            
            if (_gridSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                // 只显示存在的属性，避免NullReferenceException
                if (_gridType != null)
                    EditorGUILayout.PropertyField(_gridType, new GUIContent("Grid Type", "网格类型"));
                if (_gridSize != null)
                    EditorGUILayout.PropertyField(_gridSize, new GUIContent("Grid Size", "网格大小"));
                if (_displayArea != null)
                    EditorGUILayout.PropertyField(_displayArea, new GUIContent("Display Area", "显示区域大小"));
                if (_gridOffset != null)
                    EditorGUILayout.PropertyField(_gridOffset, new GUIContent("Grid Offset", "网格偏移"));
                if (_alignmentMode != null)
                    EditorGUILayout.PropertyField(_alignmentMode, new GUIContent("Alignment", "对齐方式"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawAppearanceSettings()
        {
            _appearanceSettingsFoldout = EditorGUILayout.Foldout(_appearanceSettingsFoldout, "Appearance Settings", true);
            
            if (_appearanceSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_gridColor != null)
                    EditorGUILayout.PropertyField(_gridColor, new GUIContent("Grid Color", "网格颜色"));
                if (_lineWidth != null)
                    EditorGUILayout.PropertyField(_lineWidth, new GUIContent("Line Width", "线条宽度"));
                if (_alpha != null)
                    EditorGUILayout.PropertyField(_alpha, new GUIContent("Alpha", "透明度"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawFadeSettings()
        {
            _fadeSettingsFoldout = EditorGUILayout.Foldout(_fadeSettingsFoldout, "Fade Settings", true);
            
            if (_fadeSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_enableEdgeFade != null)
                {
                    EditorGUILayout.PropertyField(_enableEdgeFade, new GUIContent("Enable Edge Fade", "启用边缘渐变"));
                    
                    if (_enableEdgeFade.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        if (_fadeDistance != null)
                            EditorGUILayout.PropertyField(_fadeDistance, new GUIContent("Fade Distance", "渐变距离"));
                        if (_fadeCurve != null)
                            EditorGUILayout.PropertyField(_fadeCurve, new GUIContent("Fade Curve", "渐变曲线"));
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawPerformanceSettings()
        {
            _performanceSettingsFoldout = EditorGUILayout.Foldout(_performanceSettingsFoldout, "Performance Settings", true);
            
            if (_performanceSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_enableLOD != null)
                    EditorGUILayout.PropertyField(_enableLOD, new GUIContent("Enable LOD", "启用LOD系统"));
                if (_maxGridLines != null)
                    EditorGUILayout.PropertyField(_maxGridLines, new GUIContent("Max Grid Lines", "最大网格线数"));
                if (_enableDynamicUpdate != null)
                {
                    EditorGUILayout.PropertyField(_enableDynamicUpdate, new GUIContent("Dynamic Update", "动态更新"));
                    
                    if (_enableDynamicUpdate.boolValue && _updateFrequency != null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_updateFrequency, new GUIContent("LOD Distance", "LOD距离"));
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawRenderingSettings()
        {
            _renderingSettingsFoldout = EditorGUILayout.Foldout(_renderingSettingsFoldout, "Rendering Settings", true);
            
            if (_renderingSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                // 排序层
                DrawSortingLayerField();
                if (_sortingOrder != null)
                    EditorGUILayout.PropertyField(_sortingOrder, new GUIContent("Order in Layer", "层级内排序"));
                
                EditorGUILayout.Space();
                
                if (_enableAntiAliasing != null)
                    EditorGUILayout.PropertyField(_enableAntiAliasing, new GUIContent("Anti Aliasing", "抗锯齿"));
                if (_enableSmoothLines != null)
                    EditorGUILayout.PropertyField(_enableSmoothLines, new GUIContent("Smooth Lines", "平滑线条"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawDebugSettings()
        {
            _debugSettingsFoldout = EditorGUILayout.Foldout(_debugSettingsFoldout, "Debug Settings", true);
            
            if (_debugSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_showDebugInfo != null)
                    EditorGUILayout.PropertyField(_showDebugInfo, new GUIContent("Show Debug Info", "显示调试信息"));
                if (_showBounds != null)
                {
                    EditorGUILayout.PropertyField(_showBounds, new GUIContent("Show Bounds", "显示边界"));
                    
                    if (_showBounds.boolValue && _debugColor != null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_debugColor, new GUIContent("Debug Color", "调试颜色"));
                        EditorGUI.indentLevel--;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawSortingLayerField()
        {
            // 检查必要的属性是否存在
            if (_sortingLayerID == null || _sortingLayerName == null)
                return;
                
            var sortingLayerNames = SortingLayer.layers;
            var layerNames = new string[sortingLayerNames.Length];
            var layerIDs = new int[sortingLayerNames.Length];
            
            for (int i = 0; i < sortingLayerNames.Length; i++)
            {
                layerNames[i] = sortingLayerNames[i].name;
                layerIDs[i] = sortingLayerNames[i].id;
            }
            
            int currentIndex = 0;
            for (int i = 0; i < layerIDs.Length; i++)
            {
                if (layerIDs[i] == _sortingLayerID.intValue)
                {
                    currentIndex = i;
                    break;
                }
            }
            
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Sorting Layer", currentIndex, layerNames);
            
            if (EditorGUI.EndChangeCheck())
            {
                _sortingLayerID.intValue = layerIDs[newIndex];
                _sortingLayerName.stringValue = layerNames[newIndex];
            }
        }
        
        private void DrawStats()
        {
            var renderer = target as Grid2DRenderer;
            if (renderer == null || !renderer.showDebugInfo) return;
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            
            var stats = renderer.GetRenderStats();
            
            EditorGUILayout.LabelField($"Vertices: {stats.vertexCount}");
            EditorGUILayout.LabelField($"Triangles: {stats.triangleCount}");
            EditorGUILayout.LabelField($"Grid Lines: {stats.gridLineCount}");
            EditorGUILayout.LabelField($"LOD Level: {stats.lodLevel}");
            EditorGUILayout.LabelField($"Render Time: {stats.renderTime:F2}ms");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Regenerate Grid"))
            {
                var renderer = target as Grid2DRenderer;
                if (renderer != null)
                {
                    renderer.RegenerateGrid();
                }
            }
            
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset to Default", 
                    "Are you sure you want to reset all settings to default values?", 
                    "Yes", "No"))
                {
                    var renderer = target as Grid2DRenderer;
                    if (renderer != null)
                    {
                        Undo.RecordObject(renderer, "Reset Grid2D Renderer");
                        renderer.ResetToDefault();
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Scene GUI
        
        private void OnSceneGUI()
        {
            var renderer = target as Grid2DRenderer;
            if (renderer == null || !renderer.showBounds) return;
            
            Handles.color = renderer.debugColor;
            
            var transform = renderer.transform;
            var center = transform.position + (Vector3)renderer.gridOffset;
            var size = renderer.displayArea;
            
            // 绘制显示区域边界
            var bounds = new Bounds(center, new Vector3(size.x, size.y, 0));
            Handles.DrawWireCube(bounds.center, bounds.size);
            
            // 绘制网格中心点
            Handles.DrawWireDisc(center, Vector3.forward, 0.5f);
            
            Handles.color = Color.white;
        }
        
        #endregion
    }
}