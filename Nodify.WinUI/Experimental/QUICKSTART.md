# 快速入门指南

## 如何集成到你的 WinUI 3 项目

### 步骤 1: 在 MainWindow 中使用

修改 `MainWindow.xaml`:

```xml
<Window
    x:Class="Nodify.WinUI.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:experimental="using:Nodify.WinUI.Experimental">

    <experimental:SamplePage/>
</Window>
```

或者直接使用编辑器控件：

```xml
<Window
    x:Class="Nodify.WinUI.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:view="using:Nodify.WinUI.Experimental.View"
    xmlns:converters="using:Nodify.WinUI.Experimental.Converters">

    <Window.Resources>
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    </Window.Resources>

    <view:NodeEditorCanvas x:Name="NodeEditor"/>
</Window>
```

### 步骤 2: 在 MainWindow.xaml.cs 中初始化

```csharp
using Nodify.WinUI.Experimental.ViewModel;
using Nodify.WinUI.Experimental.Model;
using Windows.Foundation;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        InitializeNodeEditor();
    }

    private void InitializeNodeEditor()
    {
        var viewModel = new NodeEditorViewModel();
        
        // 添加一些示例节点
        var node1 = CreateNode("Start Node", new Point(100, 100));
        var node2 = CreateNode("Process Node", new Point(400, 100));
        
        viewModel.Nodes.Add(node1);
        viewModel.Nodes.Add(node2);
        
        NodeEditor.ViewModel = viewModel;
    }

    private NodeViewModel CreateNode(string title, Point position)
    {
        var model = new NodeModel(title, position.X, position.Y);
        model.InputPorts.Add(new PortModel("Input", PortDirection.Input) 
        { 
            NodeId = model.Id 
        });
        model.OutputPorts.Add(new PortModel("Output", PortDirection.Output) 
        { 
            NodeId = model.Id 
        });
        
        return new NodeViewModel(model);
    }
}
```

### 步骤 3: 测试功能

运行应用后，你应该能够：

1. ✅ 看到两个节点显示在画布上
2. ✅ 拖拽节点标题栏移动节点
3. ✅ 从一个节点的输出端口拖线到另一个节点的输入端口
4. ✅ 使用鼠标中键拖拽画布
5. ✅ 使用 Ctrl+滚轮缩放
6. ✅ 双击画布创建新节点
7. ✅ 双击连接线删除连接
8. ✅ 使用工具栏按钮保存/加载

## 常见问题

### Q: 为什么 BoolToVisibilityConverter 找不到？

A: 确保在 XAML 中添加了资源：

```xml
<Window.Resources>
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
</Window.Resources>
```

### Q: 为什么保存/加载功能不工作？

A: 检查 `App.xaml.cs` 是否有公共的 `m_window` 字段：

```csharp
public partial class App : Application
{
    public Window m_window; // 确保是 public

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }
}
```

### Q: 如何自定义节点的默认大小？

A: 修改 `Model/NodeModel.cs` 的构造函数：

```csharp
public NodeModel()
{
    // ...
    Width = 250;  // 修改默认宽度
    Height = 200; // 修改默认高度
    // ...
}
```

### Q: 如何改变连接线的颜色？

A: 修改 `View/ConnectionControl.xaml`:

```xml
<Path
    x:Name="ConnectionPath"
    Stroke="Red"  <!-- 改为红色 -->
    StrokeThickness="3"
    .../>
```

## 下一步

- 查看 `README.md` 了解所有功能
- 修改 `NodeControl.xaml` 自定义节点外观
- 扩展 `PortType` 添加自定义端口类型
- 在 `NodeEditorViewModel` 中添加自定义命令

## 示例：创建自定义节点类型

```csharp
public class MathNodeViewModel : NodeViewModel
{
    public MathNodeViewModel() : base(new NodeModel("Math", 0, 0))
    {
        this.AddInputPort("A", PortType.Data);
        this.AddInputPort("B", PortType.Data);
        this.AddOutputPort("Result", PortType.Data);
        Content = "A + B";
    }
}

// 使用
viewModel.Nodes.Add(new MathNodeViewModel { X = 100, Y = 100 });
```

## 调试技巧

1. 在 `NodeEditorViewModel.CompleteConnection` 设置断点查看连接创建过程
2. 检查 `ViewModel.UpdateConnectionPositions()` 如果连接线不更新
3. 查看输出窗口的绑定错误
