# WinUI 3 节点编辑器

功能完整的 WinUI 3 节点编辑器控件，采用 MVVM 架构。

## 🚀 快速开始

在 `MainWindow.xaml` 中添加：

```xml
<Window xmlns:experimental="using:Nodify.WinUI.Experimental">
    <experimental:SamplePage/>
</Window>
```

按 F5 运行即可。

## ⚠️ 重要提示

**在开始开发前，请务必阅读 [DEV_GUIDE.md](DEV_GUIDE.md)**

该文档包含：
- 🔥 关键陷阱（特别是 Geometry 共享问题）
- WinUI 3 vs WPF 核心差异
- 常见问题速查
- 开发检查清单

## 📋 功能特性

- ✅ 节点创建、移动、选中
- ✅ 端口连接（贝塞尔曲线）
- ✅ 画布平移、缩放
- ✅ 小地图导航
- ✅ 序列化/反序列化
- ✅ 完整的 MVVM 架构

## 📁 项目结构

```
Experimental/
├── Model/          # 数据模型
├── ViewModel/      # 视图模型
├── View/           # UI 控件
├── Converters/     # 值转换器
├── Helpers/        # 辅助类
└── Common/         # MVVM 基础类
```

## 🎮 基本操作

| 操作 | 方法 |
|------|------|
| 创建节点 | 点击"添加节点"按钮 |
| 移动节点 | 拖拽节点标题栏 |
| 创建连接 | 从输出端口拖到输入端口 |
| 删除连接 | 双击连接线 |
| 平移画布 | 鼠标中键拖拽 |
| 缩放画布 | Ctrl + 滚轮 |

## 📚 文档

- **[DEV_GUIDE.md](DEV_GUIDE.md)** - 开发指南（必读）

## 💡 代码示例

### 创建节点

```csharp
var node = new NodeViewModel(new NodeModel("节点标题", 100, 100));
node.InputPorts.Add(new PortViewModel(new PortModel("输入", PortDirection.Input)));
node.OutputPorts.Add(new PortViewModel(new PortModel("输出", PortDirection.Output)));
viewModel.Nodes.Add(node);
```

### 保存/加载

```csharp
// 保存
var state = viewModel.GetEditorState();
await SerializationHelper.SaveToFileAsync(state);

// 加载
var state = await SerializationHelper.LoadFromFileAsync();
viewModel.LoadEditorState(state);
```

## 🔧 技术栈

- WinUI 3 (Windows App SDK 1.5+)
- .NET 8.0
- System.Text.Json

## ⚠️ 关键注意事项

1. **Geometry 不能共享** - 每个 Path 需要独立的 Geometry 实例
2. **不支持 StringFormat** - 使用 IValueConverter
3. **BoolToVisibilityConverter 需自定义** - 不是内置的
4. **Window 不是 UIElement** - 通过 App 类访问
5. **文件对话框需要窗口句柄** - 使用 WinRT.Interop

详细说明请查看 [DEV_GUIDE.md](DEV_GUIDE.md)

---

**最后更新**: 2024  
**状态**: ✅ 生产就绪
