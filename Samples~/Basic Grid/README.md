# Basic Grid Example

这个示例展示了如何使用Grid2D组件创建基本的2D网格线效果。

## 场景内容

### BasicGridExample.unity

这个场景包含了一个配置好的Grid2D Renderer组件，展示了以下功能：

- **基础网格渲染**：1x1单位的网格
- **显示区域**：20x20单位的显示范围
- **边缘渐变**：5单位的渐变距离
- **LOD系统**：自动根据相机距离调整网格密度
- **动态更新**：实时响应相机移动

## 使用方法

1. 打开 `BasicGridExample.unity` 场景
2. 运行场景
3. 在Scene视图中移动相机观察网格效果
4. 在Inspector中调整Grid2D Renderer的参数来体验不同效果

## 参数说明

### 网格设置
- **Grid Size**: 网格单元大小 (1, 1)
- **Display Area**: 显示区域大小 (20, 20)
- **Grid Offset**: 网格偏移 (0, 0)

### 外观设置
- **Grid Color**: 网格颜色 (白色)
- **Line Width**: 线条宽度 (0.1)
- **Alpha**: 透明度 (0.8)

### 渐变设置
- **Enable Edge Fade**: 启用边缘渐变
- **Fade Distance**: 渐变距离 (5)
- **Fade Curve**: 渐变曲线 (平滑过渡)

### 性能设置
- **Enable LOD**: 启用LOD系统
- **Max Grid Lines**: 最大网格线数 (1000)
- **Enable Dynamic Update**: 启用动态更新
- **Update Frequency**: 更新频率 (0.1秒)

## 自定义网格

你可以通过以下方式自定义网格效果：

1. **改变网格大小**：调整 `Grid Size` 参数
2. **改变显示范围**：调整 `Display Area` 参数
3. **改变颜色**：调整 `Grid Color` 参数
4. **改变透明度**：调整 `Alpha` 参数
5. **调整渐变效果**：修改 `Fade Distance` 和 `Fade Curve` 参数

## 性能优化

- 启用LOD系统可以在相机距离较远时减少网格密度
- 调整 `Max Grid Lines` 可以限制最大渲染的网格线数量
- 禁用 `Dynamic Update` 可以提高性能，但网格不会响应相机移动

## 扩展示例

基于这个基础示例，你可以创建更复杂的效果：

- 多层网格（不同大小的网格叠加）
- 动画网格（通过脚本动态改变参数）
- 交互式网格（响应用户输入）
- 主题化网格（不同的颜色和样式）