using UnityEngine;
using UnityEditor;

namespace UGC.Grid2D.Editor
{
    /// <summary>
    /// Grid2D Shader的自定义编辑器GUI
    /// </summary>
    public class Grid2DShaderGUI : ShaderGUI
    {
        #region Material Properties
        
        private MaterialProperty _gridSize;
        private MaterialProperty _gridColor;
        private MaterialProperty _lineWidth;
        private MaterialProperty _alpha;
        
        private MaterialProperty _enableEdgeFade;
        private MaterialProperty _fadeDistance;
        private MaterialProperty _displayArea;
        private MaterialProperty _fadeTexture;
        
        private MaterialProperty _zWrite;
        private MaterialProperty _zTest;
        private MaterialProperty _cull;
        
        private MaterialProperty _antiAliasing;
        private MaterialProperty _smoothLines;
        
        #endregion
        
        #region Foldout States
        
        private bool _gridSettingsFoldout = true;
        private bool _fadeSettingsFoldout = true;
        private bool _renderingSettingsFoldout = false;
        private bool _advancedSettingsFoldout = false;
        
        #endregion
        
        #region ShaderGUI Override
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);
            
            Material material = materialEditor.target as Material;
            
            EditorGUILayout.Space();
            
            DrawHeader();
            
            EditorGUILayout.Space();
            
            DrawGridSettings(materialEditor);
            DrawFadeSettings(materialEditor, material);
            DrawRenderingSettings(materialEditor);
            DrawAdvancedSettings(materialEditor, material);
            
            EditorGUILayout.Space();
            
            DrawFooter(materialEditor, material);
        }
        
        #endregion
        
        #region Private Methods
        
        private void FindProperties(MaterialProperty[] properties)
        {
            _gridSize = FindProperty("_GridSize", properties, false);
            _gridColor = FindProperty("_GridColor", properties, false);
            _lineWidth = FindProperty("_LineWidth", properties, false);
            _alpha = FindProperty("_Alpha", properties, false);
            
            _enableEdgeFade = FindProperty("_EnableEdgeFade", properties, false);
            _fadeDistance = FindProperty("_FadeDistance", properties, false);
            _displayArea = FindProperty("_DisplayArea", properties, false);
            _fadeTexture = FindProperty("_FadeTexture", properties, false);
            
            _zWrite = FindProperty("_ZWrite", properties, false);
            _zTest = FindProperty("_ZTest", properties, false);
            _cull = FindProperty("_Cull", properties, false);
            
            _antiAliasing = FindProperty("_AntiAliasing", properties, false);
            _smoothLines = FindProperty("_SmoothLines", properties, false);
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label("Grid2D Shader", EditorStyles.boldLabel);
            GUILayout.Label("Unity 2D Grid Line Renderer Shader", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGridSettings(MaterialEditor materialEditor)
        {
            _gridSettingsFoldout = EditorGUILayout.Foldout(_gridSettingsFoldout, "Grid Settings", true);
            
            if (_gridSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_gridSize != null)
                {
                    Vector4 gridSize = _gridSize.vectorValue;
                    EditorGUI.BeginChangeCheck();
                    Vector2 newGridSize = EditorGUILayout.Vector2Field("Grid Size", new Vector2(gridSize.x, gridSize.y));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _gridSize.vectorValue = new Vector4(newGridSize.x, newGridSize.y, 0, 0);
                    }
                }
                
                if (_gridColor != null)
                    materialEditor.ColorProperty(_gridColor, "Grid Color");
                
                if (_lineWidth != null)
                    materialEditor.RangeProperty(_lineWidth, "Line Width");
                
                if (_alpha != null)
                    materialEditor.RangeProperty(_alpha, "Alpha");
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawFadeSettings(MaterialEditor materialEditor, Material material)
        {
            _fadeSettingsFoldout = EditorGUILayout.Foldout(_fadeSettingsFoldout, "Fade Settings", true);
            
            if (_fadeSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                bool enableEdgeFade = false;
                if (_enableEdgeFade != null)
                {
                    enableEdgeFade = _enableEdgeFade.floatValue > 0.5f;
                    EditorGUI.BeginChangeCheck();
                    enableEdgeFade = EditorGUILayout.Toggle("Enable Edge Fade", enableEdgeFade);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _enableEdgeFade.floatValue = enableEdgeFade ? 1.0f : 0.0f;
                        
                        // 更新关键字
                        if (enableEdgeFade)
                            material.EnableKeyword("_EDGE_FADE_ON");
                        else
                            material.DisableKeyword("_EDGE_FADE_ON");
                    }
                }
                
                if (enableEdgeFade)
                {
                    EditorGUI.indentLevel++;
                    
                    if (_fadeDistance != null)
                        materialEditor.FloatProperty(_fadeDistance, "Fade Distance");
                    
                    if (_displayArea != null)
                    {
                        Vector4 displayArea = _displayArea.vectorValue;
                        EditorGUI.BeginChangeCheck();
                        Vector2 newDisplayArea = EditorGUILayout.Vector2Field("Display Area", new Vector2(displayArea.x, displayArea.y));
                        if (EditorGUI.EndChangeCheck())
                        {
                            _displayArea.vectorValue = new Vector4(newDisplayArea.x, newDisplayArea.y, 0, 0);
                        }
                    }
                    
                    if (_fadeTexture != null)
                        materialEditor.TextureProperty(_fadeTexture, "Fade Texture");
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawRenderingSettings(MaterialEditor materialEditor)
        {
            _renderingSettingsFoldout = EditorGUILayout.Foldout(_renderingSettingsFoldout, "Rendering Settings", true);
            
            if (_renderingSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                if (_antiAliasing != null)
                {
                    bool antiAliasing = _antiAliasing.floatValue > 0.5f;
                    EditorGUI.BeginChangeCheck();
                    antiAliasing = EditorGUILayout.Toggle("Anti Aliasing", antiAliasing);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _antiAliasing.floatValue = antiAliasing ? 1.0f : 0.0f;
                        
                        Material material = materialEditor.target as Material;
                        if (antiAliasing)
                            material.EnableKeyword("_ANTI_ALIASING_ON");
                        else
                            material.DisableKeyword("_ANTI_ALIASING_ON");
                    }
                }
                
                if (_smoothLines != null)
                {
                    bool smoothLines = _smoothLines.floatValue > 0.5f;
                    EditorGUI.BeginChangeCheck();
                    smoothLines = EditorGUILayout.Toggle("Smooth Lines", smoothLines);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _smoothLines.floatValue = smoothLines ? 1.0f : 0.0f;
                        
                        Material material = materialEditor.target as Material;
                        if (smoothLines)
                            material.EnableKeyword("_SMOOTH_LINES_ON");
                        else
                            material.DisableKeyword("_SMOOTH_LINES_ON");
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawAdvancedSettings(MaterialEditor materialEditor, Material material)
        {
            _advancedSettingsFoldout = EditorGUILayout.Foldout(_advancedSettingsFoldout, "Advanced Settings", true);
            
            if (_advancedSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                
                // 渲染队列
                EditorGUI.BeginChangeCheck();
                int renderQueue = EditorGUILayout.IntField("Render Queue", material.renderQueue);
                if (EditorGUI.EndChangeCheck())
                {
                    material.renderQueue = renderQueue;
                }
                
                EditorGUILayout.Space();
                
                // Z写入
                if (_zWrite != null)
                {
                    bool zWrite = _zWrite.floatValue > 0.5f;
                    EditorGUI.BeginChangeCheck();
                    zWrite = EditorGUILayout.Toggle("Z Write", zWrite);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _zWrite.floatValue = zWrite ? 1.0f : 0.0f;
                    }
                }
                
                // Z测试
                if (_zTest != null)
                {
                    string[] zTestOptions = { "Disabled", "Never", "Less", "Equal", "LEqual", "Greater", "NotEqual", "GEqual", "Always" };
                    int zTestValue = Mathf.RoundToInt(_zTest.floatValue);
                    EditorGUI.BeginChangeCheck();
                    zTestValue = EditorGUILayout.Popup("Z Test", zTestValue, zTestOptions);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _zTest.floatValue = zTestValue;
                    }
                }
                
                // 剔除模式
                if (_cull != null)
                {
                    string[] cullOptions = { "Off", "Front", "Back" };
                    int cullValue = Mathf.RoundToInt(_cull.floatValue);
                    EditorGUI.BeginChangeCheck();
                    cullValue = EditorGUILayout.Popup("Cull Mode", cullValue, cullOptions);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _cull.floatValue = cullValue;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawFooter(MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Shader Keywords", EditorStyles.boldLabel);
            
            var keywords = material.shaderKeywords;
            if (keywords.Length > 0)
            {
                EditorGUI.indentLevel++;
                foreach (string keyword in keywords)
                {
                    EditorGUILayout.LabelField($"• {keyword}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("No keywords enabled", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Material", 
                    "Are you sure you want to reset this material to default values?", 
                    "Yes", "No"))
                {
                    ResetMaterialToDefault(material);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void ResetMaterialToDefault(Material material)
        {
            // 重置属性到默认值
            if (_gridSize != null)
                _gridSize.vectorValue = new Vector4(1, 1, 0, 0);
            
            if (_gridColor != null)
                _gridColor.colorValue = Color.white;
            
            if (_lineWidth != null)
                _lineWidth.floatValue = 0.1f;
            
            if (_alpha != null)
                _alpha.floatValue = 1.0f;
            
            if (_enableEdgeFade != null)
                _enableEdgeFade.floatValue = 1.0f;
            
            if (_fadeDistance != null)
                _fadeDistance.floatValue = 10.0f;
            
            if (_displayArea != null)
                _displayArea.vectorValue = new Vector4(100, 100, 0, 0);
            
            if (_antiAliasing != null)
                _antiAliasing.floatValue = 1.0f;
            
            if (_smoothLines != null)
                _smoothLines.floatValue = 1.0f;
            
            if (_zWrite != null)
                _zWrite.floatValue = 0.0f;
            
            if (_zTest != null)
                _zTest.floatValue = 4.0f; // LEqual
            
            if (_cull != null)
                _cull.floatValue = 0.0f; // Off
            
            // 重置关键字
            material.EnableKeyword("_EDGE_FADE_ON");
            material.EnableKeyword("_ANTI_ALIASING_ON");
            material.EnableKeyword("_SMOOTH_LINES_ON");
            
            // 重置渲染队列
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
            EditorUtility.SetDirty(material);
        }
        
        #endregion
    }
}