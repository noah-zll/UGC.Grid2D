using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UGC.Grid2D
{
    /// <summary>
    /// Grid2D网格生成器
    /// 负责生成网格线的顶点、UV和三角形数据
    /// </summary>
    public class Grid2DMesh : System.IDisposable
    {
        private List<Vector3> _vertices;
        private List<Vector2> _uvs;
        private List<int> _triangles;
        private List<Color> _colors;
        private Mesh _cachedMesh;
        private int _lastLineCount;
        private bool _disposed = false;
        
        // 对象池
        private static readonly Queue<List<Vector3>> _vertexPool = new Queue<List<Vector3>>();
        private static readonly Queue<List<Vector2>> _uvPool = new Queue<List<Vector2>>();
        private static readonly Queue<List<int>> _trianglePool = new Queue<List<int>>();
        private static readonly Queue<List<Color>> _colorPool = new Queue<List<Color>>();
        
        public Grid2DMesh()
        {
            _vertices = GetPooledList<Vector3>(_vertexPool);
            _uvs = GetPooledList<Vector2>(_uvPool);
            _triangles = GetPooledList<int>(_trianglePool);
            _colors = GetPooledList<Color>(_colorPool);
        }
        
        /// <summary>
        /// 生成网格
        /// </summary>
        public Mesh GenerateMesh(Grid2DData gridData)
        {
            if (_disposed)
            {
                Debug.LogError("Grid2DMesh has been disposed!");
                return null;
            }
            
            ClearLists();
            
            switch (gridData.gridType)
            {
                case GridType.Square:
                case GridType.Rectangle:
                    GenerateRectangularGrid(gridData);
                    break;
                case GridType.Hexagon:
                    GenerateHexagonalGrid(gridData);
                    break;
                case GridType.Triangle:
                    GenerateTriangularGrid(gridData);
                    break;
                default:
                    GenerateRectangularGrid(gridData);
                    break;
            }
            
            return CreateMesh();
        }
        
        /// <summary>
        /// 获取当前网格线数量
        /// </summary>
        public int GetLineCount()
        {
            return _lastLineCount;
        }
        
        /// <summary>
        /// 生成矩形网格
        /// </summary>
        private void GenerateRectangularGrid(Grid2DData gridData)
        {
            Vector2Int lineCount = gridData.CalculateLineCount();
            _lastLineCount = lineCount.x + lineCount.y;
            
            Vector2 halfArea = gridData.displayArea * 0.5f;
            Vector2 startPos = gridData.offset - halfArea;
            Vector2 endPos = gridData.offset + halfArea;
            
            // 应用对齐方式
            ApplyAlignment(ref startPos, ref endPos, gridData.alignment, gridData.displayArea);
            
            // 生成一个覆盖整个显示区域的四边形
            // 让着色器来处理网格线的渲染，而不是生成大量的线条几何体
            GenerateQuad(startPos, endPos);
        }
        
        /// <summary>
        /// 生成六边形网格
        /// </summary>
        private void GenerateHexagonalGrid(Grid2DData gridData)
        {
            Vector2 halfArea = gridData.displayArea * 0.5f;
            Vector2 center = gridData.offset;
            
            float hexRadius = Mathf.Min(gridData.gridSize.x, gridData.gridSize.y) * 0.5f;
            float hexWidth = hexRadius * 2f;
            float hexHeight = hexRadius * Mathf.Sqrt(3f);
            
            int hexCountX = Mathf.CeilToInt(gridData.displayArea.x / (hexWidth * 0.75f));
            int hexCountY = Mathf.CeilToInt(gridData.displayArea.y / hexHeight);
            
            _lastLineCount = 0;
            
            for (int x = -hexCountX; x <= hexCountX; x++)
            {
                for (int y = -hexCountY; y <= hexCountY; y++)
                {
                    Vector2 hexCenter = GetHexagonCenter(x, y, hexWidth, hexHeight);
                    hexCenter += center;
                    
                    // 检查是否在显示区域内
                    if (Mathf.Abs(hexCenter.x - center.x) > halfArea.x ||
                        Mathf.Abs(hexCenter.y - center.y) > halfArea.y)
                        continue;
                    
                    GenerateHexagon(hexCenter, hexRadius);
                    _lastLineCount += 6;
                }
            }
        }
        
        /// <summary>
        /// 生成三角形网格
        /// </summary>
        private void GenerateTriangularGrid(Grid2DData gridData)
        {
            Vector2 halfArea = gridData.displayArea * 0.5f;
            Vector2 center = gridData.offset;
            
            float triSize = Mathf.Min(gridData.gridSize.x, gridData.gridSize.y);
            float triHeight = triSize * Mathf.Sqrt(3f) * 0.5f;
            
            int triCountX = Mathf.CeilToInt(gridData.displayArea.x / triSize);
            int triCountY = Mathf.CeilToInt(gridData.displayArea.y / triHeight);
            
            _lastLineCount = 0;
            
            for (int x = -triCountX; x <= triCountX; x++)
            {
                for (int y = -triCountY; y <= triCountY; y++)
                {
                    Vector2 triCenter = new Vector2(
                        x * triSize + (y % 2) * triSize * 0.5f,
                        y * triHeight
                    );
                    triCenter += center;
                    
                    // 检查是否在显示区域内
                    if (Mathf.Abs(triCenter.x - center.x) > halfArea.x ||
                        Mathf.Abs(triCenter.y - center.y) > halfArea.y)
                        continue;
                    
                    GenerateTriangle(triCenter, triSize, y % 2 == 0);
                    _lastLineCount += 3;
                }
            }
        }
        
        /// <summary>
        /// 生成四边形
        /// </summary>
        private void GenerateQuad(Vector2 startPos, Vector2 endPos)
        {
            int startIndex = _vertices.Count;
            
            // 添加四个顶点形成四边形
            _vertices.Add(new Vector3(startPos.x, startPos.y, 0));
            _vertices.Add(new Vector3(endPos.x, startPos.y, 0));
            _vertices.Add(new Vector3(endPos.x, endPos.y, 0));
            _vertices.Add(new Vector3(startPos.x, endPos.y, 0));
            
            // 添加UV坐标
            _uvs.Add(new Vector2(0, 0));
            _uvs.Add(new Vector2(1, 0));
            _uvs.Add(new Vector2(1, 1));
            _uvs.Add(new Vector2(0, 1));
            
            // 添加颜色
            for (int i = 0; i < 4; i++)
            {
                _colors.Add(Color.white);
            }
            
            // 添加三角形索引
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 1);
            _triangles.Add(startIndex + 2);
            
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex + 3);
        }
        
        /// <summary>
        /// 生成单条线（已弃用，现在使用着色器渲染）
        /// </summary>
        private void GenerateLine(Vector3 start, Vector3 end, float width)
        {
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * width * 0.5f;
            
            int startIndex = _vertices.Count;
            
            // 添加四个顶点形成矩形
            _vertices.Add(start - perpendicular);
            _vertices.Add(start + perpendicular);
            _vertices.Add(end + perpendicular);
            _vertices.Add(end - perpendicular);
            
            // 添加UV坐标
            _uvs.Add(new Vector2(0, 0));
            _uvs.Add(new Vector2(0, 1));
            _uvs.Add(new Vector2(1, 1));
            _uvs.Add(new Vector2(1, 0));
            
            // 添加颜色
            for (int i = 0; i < 4; i++)
            {
                _colors.Add(Color.white);
            }
            
            // 添加三角形索引
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 1);
            _triangles.Add(startIndex + 2);
            
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex + 3);
        }
        
        /// <summary>
        /// 生成六边形
        /// </summary>
        private void GenerateHexagon(Vector2 center, float radius)
        {
            Vector3[] hexVertices = new Vector3[6];
            
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                hexVertices[i] = new Vector3(
                    center.x + radius * Mathf.Cos(angle),
                    center.y + radius * Mathf.Sin(angle),
                    0
                );
            }
            
            // 生成六边形的边
            for (int i = 0; i < 6; i++)
            {
                int nextIndex = (i + 1) % 6;
                GenerateLine(hexVertices[i], hexVertices[nextIndex], radius * 0.1f);
            }
        }
        
        /// <summary>
        /// 生成三角形
        /// </summary>
        private void GenerateTriangle(Vector2 center, float size, bool pointUp)
        {
            float height = size * Mathf.Sqrt(3f) * 0.5f;
            Vector3[] triVertices = new Vector3[3];
            
            if (pointUp)
            {
                triVertices[0] = new Vector3(center.x, center.y + height * 0.67f, 0);
                triVertices[1] = new Vector3(center.x - size * 0.5f, center.y - height * 0.33f, 0);
                triVertices[2] = new Vector3(center.x + size * 0.5f, center.y - height * 0.33f, 0);
            }
            else
            {
                triVertices[0] = new Vector3(center.x, center.y - height * 0.67f, 0);
                triVertices[1] = new Vector3(center.x - size * 0.5f, center.y + height * 0.33f, 0);
                triVertices[2] = new Vector3(center.x + size * 0.5f, center.y + height * 0.33f, 0);
            }
            
            // 生成三角形的边
            for (int i = 0; i < 3; i++)
            {
                int nextIndex = (i + 1) % 3;
                GenerateLine(triVertices[i], triVertices[nextIndex], size * 0.05f);
            }
        }
        
        /// <summary>
        /// 获取六边形中心位置
        /// </summary>
        private Vector2 GetHexagonCenter(int x, int y, float hexWidth, float hexHeight)
        {
            float posX = x * hexWidth * 0.75f;
            float posY = y * hexHeight + (x % 2) * hexHeight * 0.5f;
            return new Vector2(posX, posY);
        }
        
        /// <summary>
        /// 应用对齐方式
        /// </summary>
        private void ApplyAlignment(ref Vector2 startPos, ref Vector2 endPos, GridAlignment alignment, Vector2 displayArea)
        {
            Vector2 offset = Vector2.zero;
            
            switch (alignment)
            {
                case GridAlignment.BottomLeft:
                    offset = new Vector2(displayArea.x * 0.5f, displayArea.y * 0.5f);
                    break;
                case GridAlignment.TopLeft:
                    offset = new Vector2(displayArea.x * 0.5f, -displayArea.y * 0.5f);
                    break;
                case GridAlignment.BottomRight:
                    offset = new Vector2(-displayArea.x * 0.5f, displayArea.y * 0.5f);
                    break;
                case GridAlignment.TopRight:
                    offset = new Vector2(-displayArea.x * 0.5f, -displayArea.y * 0.5f);
                    break;
                case GridAlignment.Center:
                default:
                    offset = Vector2.zero;
                    break;
            }
            
            startPos += offset;
            endPos += offset;
        }
        
        /// <summary>
        /// 创建Mesh对象
        /// </summary>
        private Mesh CreateMesh()
        {
            if (_cachedMesh == null)
            {
                _cachedMesh = new Mesh();
                _cachedMesh.name = "Grid2D Mesh";
            }
            
            _cachedMesh.Clear();
            
            if (_vertices.Count > 0)
            {
                _cachedMesh.SetVertices(_vertices);
                _cachedMesh.SetUVs(0, _uvs);
                _cachedMesh.SetColors(_colors);
                _cachedMesh.SetTriangles(_triangles, 0);
                
                _cachedMesh.RecalculateNormals();
                _cachedMesh.RecalculateBounds();
            }
            
            return _cachedMesh;
        }
        
        /// <summary>
        /// 清空列表
        /// </summary>
        private void ClearLists()
        {
            _vertices.Clear();
            _uvs.Clear();
            _triangles.Clear();
            _colors.Clear();
        }
        
        /// <summary>
        /// 从对象池获取列表
        /// </summary>
        private static List<T> GetPooledList<T>(Queue<List<T>> pool)
        {
            if (pool.Count > 0)
            {
                var list = pool.Dequeue();
                list.Clear();
                return list;
            }
            return new List<T>();
        }
        
        /// <summary>
        /// 返回列表到对象池
        /// </summary>
        private static void ReturnToPool<T>(List<T> list, Queue<List<T>> pool)
        {
            if (list != null && pool.Count < 10) // 限制池大小
            {
                list.Clear();
                pool.Enqueue(list);
            }
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            ReturnToPool(_vertices, _vertexPool);
            ReturnToPool(_uvs, _uvPool);
            ReturnToPool(_triangles, _trianglePool);
            ReturnToPool(_colors, _colorPool);
            
            if (_cachedMesh != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_cachedMesh);
                else
                    Object.DestroyImmediate(_cachedMesh);
                _cachedMesh = null;
            }
            
            _disposed = true;
        }
        
        ~Grid2DMesh()
        {
            if (!_disposed)
            {
                Debug.LogWarning("Grid2DMesh was not properly disposed!");
            }
        }
    }
}