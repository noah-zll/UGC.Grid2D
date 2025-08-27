# UGC Grid2D

A flexible and performant Unity 2D grid rendering component that allows you to display customizable grid lines in your 2D scenes.

## Features

- **Easy Integration**: Simply add the Grid2DRenderer component to any GameObject
- **Customizable Grid Size**: Configure grid cell dimensions to match your needs
- **Flexible Display Area**: Set the visible grid area size
- **Edge Fade Effect**: Smooth gradient transparency at grid boundaries
- **Performance Optimized**: LOD system and efficient rendering
- **Runtime Configuration**: Modify grid parameters during gameplay

## Quick Start

### Installation

1. Open Unity Package Manager
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/ugf/UGC.Grid2D.git`

### Basic Usage

```csharp
// Add the component to a GameObject
var gridRenderer = gameObject.AddComponent<Grid2DRenderer>();

// Configure grid settings
gridRenderer.gridSize = new Vector2(1f, 1f);        // 1x1 unit grid cells
gridRenderer.displayArea = new Vector2(100f, 100f);  // 100x100 unit display area
gridRenderer.gridColor = Color.white;                // White grid lines
gridRenderer.lineWidth = 0.1f;                       // Thin lines

// Enable edge fade effect
gridRenderer.enableEdgeFade = true;
gridRenderer.fadeDistance = 10f;
```

### Advanced Configuration

```csharp
// Performance settings
gridRenderer.enableLOD = true;
gridRenderer.maxGridLines = 2000;

// Custom fade curve
gridRenderer.fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

// Update grid at runtime
gridRenderer.UpdateGrid();
```

## Component Properties

### Grid Settings
- **Grid Size**: Size of individual grid cells (Vector2)
- **Display Area**: Total area where grid lines are visible (Vector2)

### Appearance Settings
- **Grid Color**: Color of the grid lines (Color)
- **Line Width**: Thickness of grid lines (float)

### Fade Settings
- **Enable Edge Fade**: Toggle edge transparency effect (bool)
- **Fade Distance**: Distance over which fade occurs (float)
- **Fade Curve**: Custom animation curve for fade effect (AnimationCurve)

### Performance Settings
- **Enable LOD**: Level of detail optimization (bool)
- **LOD Distance**: Distance threshold for LOD switching (float)
- **Max Grid Lines**: Maximum number of grid lines to render (int)

## Requirements

- Unity 2021.3 LTS or later
- Built-in Render Pipeline or Universal Render Pipeline (URP)

## Samples

The package includes several sample scenes demonstrating different use cases:

- **Basic Grid Demo**: Simple grid setup
- **Animated Grid Demo**: Dynamic parameter changes
- **Custom Shader Grid Demo**: Advanced shader customization

To import samples, open Package Manager, select UGC Grid2D, and click "Import" next to the desired sample.

## Documentation

For detailed API documentation and advanced usage, see:
- [API Reference](Documentation/API.md)
- [Design Document](设计文档.md)

## Support

For questions, bug reports, or feature requests:
- GitHub Issues: [UGC.Grid2D Issues](https://github.com/ugf/UGC.Grid2D/issues)
- Email: support@ugf.com

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and updates.