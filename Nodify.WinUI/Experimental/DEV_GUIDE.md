# WinUI 3 节点编辑器 - 开发指南

> 本文档包含关键易错点和重要注意事项，帮助开发者避免常见陷阱。

## 📋 项目概述

这是一个基于 WinUI 3 的节点编辑器控件，采用 MVVM 架构。支持节点创建、拖拽、连接、序列化等完整功能。

**技术栈**：
- WinUI 3 (Windows App SDK)
- .NET 8.0
- MVVM 模式
- System.Text.Json

---

## ⚠️ 关键陷阱（必读）

### 🔥 陷阱 #1: Geometry 对象不能共享

**这是最容易导致运行时崩溃的问题！**

**错误代码**：
```csharp
// ❌ 这会导致 ArgumentException 崩溃
var geometry = CreateBezierGeometry(sourcePoint, targetPoint);
ConnectionPath.Data = geometry;  // ✅ 第一次赋值成功
SelectionPath.Data = geometry;   // 💥 崩溃！HResult=0x80070057
```

**正确代码**：
```csharp
// ✅ 每个 Path 必须有独立的 Geometry 实例
ConnectionPath.Data = CreateBezierGeometry(sourcePoint, targetPoint);
SelectionPath.Data = CreateBezierGeometry(sourcePoint, targetPoint);
```

**原因**：WinUI 3 不允许同一个 Geometry 对象被赋值给多个 Path 控件（WPF 允许）。

**影响文件**：`Experimental/View/ConnectionControl.xaml.cs`

---

### 🔥 陷阱 #2: StringFormat 不支持

**错误代码**：
```xml
<!-- ❌ WinUI 3 不支持 StringFormat -->
<TextBlock Text="{Binding ViewportScale, StringFormat='{}{0:P0}'}"/>
```

**正确代码**：
```xml
<!-- ✅ 必须使用转换器 -->
<TextBlock Text="{Binding ViewportScale, Converter={StaticResource PercentageConverter}}"/>
```

**解决方案**：创建 `PercentageConverter` 类（已在 `Converters/` 文件夹中）。

---

### 🔥 陷阱 #3: BoolToVisibilityConverter 不是内置的

**错误代码**：
```xml
<!-- ❌ WinUI 3 没有内置此转换器 -->
<Border Visibility="{Binding IsSelected, Converter={StaticResource BoolToVisibilityConverter}}"/>
```

**解决方案**：
1. 创建自定义 `BoolToVisibilityConverter`（已在 `Converters/` 文件夹中）
2. 在 XAML 资源中声明：
```xml
<UserControl.Resources>
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
</UserControl.Resources>
```

---

### 🔥 陷阱 #4: Window 不是 UIElement

**错误代码**：
```csharp
// ❌ 编译错误：Window 不继承自 UIElement
var parent = this.XamlRoot?.Content;
if (parent is Window w)  // CS8121 错误
    return w;
```

**正确代码**：
```csharp
// ✅ 通过 App 类获取窗口
var window = (Application.Current as App)?.m_window;
return window;
```

**原因**：WinUI 3 的 Window 不在可视化树中，不能通过 VisualTreeHelper 查找。

---

### 🔥 陷阱 #5: 文件对话框需要窗口句柄

**错误代码**：
```csharp
// ❌ 对话框不会显示
var picker = new FileSavePicker();
var file = await picker.PickSaveFileAsync();
```

**正确代码**：
```csharp
// ✅ 必须初始化窗口句柄
var picker = new FileSavePicker();
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
var file = await picker.PickSaveFileAsync();
```

**要求**：`App.xaml.cs` 中必须公开 `MainWindow` 属性。

---

## 📚 WinUI 3 vs WPF 核心差异

| 功能 | WPF | WinUI 3 | 解决方案 |
|------|-----|---------|---------|
| StringFormat | ✅ 支持 | ❌ 不支持 | 使用 IValueConverter |
| BoolToVisibilityConverter | ✅ 内置 | ❌ 需自定义 | 创建自定义转换器 |
| Geometry 共享 | ✅ 允许 | ❌ 禁止 | 每个 Path 独立实例 |
| Window 类型 | UIElement | 非 UIElement | 通过 App 访问 |
| Transform | TransformGroup | CompositeTransform | 使用 CompositeTransform |
| 文件对话框 | Microsoft.Win32 | Windows.Storage.Pickers | 需要窗口句柄 |

---

## 🚀 快速开始

### 1. 在 MainWindow.xaml 中添加

```xml
<Window
    xmlns:experimental="using:Nodify.WinUI.Experimental">
    
    <experimental:SamplePage/>
</Window>
```

### 2. 运行项目

按 F5 运行，即可看到节点编辑器。

### 3. 基本操作

- **创建节点**：点击"添加节点"按钮
- **移动节点**：拖拽节点标题栏
- **创建连接**：从输出端口拖到输入端口
- **删除连接**：双击连接线
- **平移画布**：鼠标中键拖拽
- **缩放画布**：Ctrl + 滚轮

---

## 📁 项目结构

```
Experimental/
├── Common/                     # MVVM 基础类
│   ├── ObservableObject.cs    # INotifyPropertyChanged 实现
│   └── RelayCommand.cs        # ICommand 实现
├── Model/                      # 数据模型（可序列化）
│   ├── PortModel.cs
│   ├── NodeModel.cs
│   ├── ConnectionModel.cs
│   └── EditorStateModel.cs
├── ViewModel/                  # 视图模型（业务逻辑）
│   ├── PortViewModel.cs
│   ├── NodeViewModel.cs
│   ├── ConnectionViewModel.cs
│   └── NodeEditorViewModel.cs
├── View/                       # UI 控件
│   ├── PortControl.xaml/cs
│   ├── NodeControl.xaml/cs
│   ├── ConnectionControl.xaml/cs
│   ├── NodeEditorCanvas.xaml/cs
│   └── MiniMapControl.xaml/cs
├── Converters/                 # 值转换器（WinUI 3 必需）
│   ├── PercentageConverter.cs
│   └── BoolToVisibilityConverter.cs
├── Helpers/
│   └── SerializationHelper.cs
└── SamplePage.xaml/cs         # 示例页面
```

---

## 🔍 常见问题速查

### Q1: 拖拽连接线时崩溃

**症状**：`ArgumentException: Value does not fall within the expected range`

**原因**：Geometry 对象被共享给多个 Path

**解决**：参考陷阱 #1，为每个 Path 创建独立的 Geometry 实例

---

### Q2: XAML 编译错误 "Unknown member 'StringFormat'"

**原因**：WinUI 3 不支持 StringFormat

**解决**：参考陷阱 #2，使用转换器

---

### Q3: 运行时错误 "Cannot find a Resource with the Name/Key BoolToVisibilityConverter"

**原因**：WinUI 3 没有内置此转换器

**解决**：参考陷阱 #3，创建并注册自定义转换器

---

### Q4: 编译错误 "Window 类型的模式无法处理 UIElement 类型的表达式"

**原因**：Window 不继承自 UIElement

**解决**：参考陷阱 #4，通过 App 类获取窗口

---

### Q5: 文件对话框不显示

**原因**：未初始化窗口句柄

**解决**：参考陷阱 #5，使用 WinRT.Interop 初始化

---

### Q6: 连接线位置不正确

**可能原因**：
1. PortControl 未调用 UpdatePosition()
2. 坐标转换未考虑 CompositeTransform

**解决**：
- 确保 PortControl 在 Loaded 和 LayoutUpdated 时更新位置
- 使用 TransformToVisual() 进行坐标转换

---

### Q7: 节点不显示

**检查清单**：
- [ ] NodeViewModel.X 和 Y 已设置
- [ ] Canvas.Left 和 Canvas.Top 绑定正确
- [ ] Canvas.RenderTransform 已设置
- [ ] DataContext 绑定正确

---

## 💡 开发建议

### 1. 必须使用的转换器

项目中已创建以下转换器，直接使用：
- `PercentageConverter` - 百分比格式化
- `BoolToVisibilityConverter` - 布尔到可见性转换
- `EmptyObjectToVisibilityConverter` - 空对象检测

### 2. 使用 CompositeTransform

```csharp
// ✅ 推荐：一次性设置所有变换
CanvasTransform.TranslateX = offsetX;
CanvasTransform.TranslateY = offsetY;
CanvasTransform.ScaleX = scale;
CanvasTransform.ScaleY = scale;
```

### 3. 使用主题资源

```xml
<!-- ✅ 使用主题资源 - 自动适配浅色/深色主题 -->
<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"/>

<!-- ❌ 避免硬编码颜色 -->
<Border Background="#FFFFFF"/>
```

### 4. 异步处理文件操作

```csharp
// WinUI 3 的文件对话框是异步的
private async void SaveButton_Click(object sender, RoutedEventArgs e)
{
    var success = await SerializationHelper.SaveToFileAsync(ViewModel);
}
```

---

## 🐛 调试技巧

### 1. 启用 XAML 绑定调试

在 Visual Studio 输出窗口查看绑定错误：
- 调试 → 窗口 → 输出
- 筛选：显示来自"调试"的输出

### 2. 使用 Live Visual Tree

调试时：
- 调试 → 窗口 → Live Visual Tree
- 检查控件层次结构和 DataContext

### 3. 添加日志

```csharp
System.Diagnostics.Debug.WriteLine($"Node at ({X}, {Y})");
```

### 4. 清理并重新生成

90% 的问题可以通过以下步骤解决：
1. 生成 → 清理解决方案
2. 生成 → 重新生成解决方案

---

## 📦 所需 NuGet 包

```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.5.*" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.*" />
<PackageReference Include="System.Text.Json" Version="8.0.*" />
```

---

## ✅ 开发检查清单

开始开发前确认：
- [ ] 了解 Geometry 不能共享的限制
- [ ] 知道 StringFormat 不可用，需要转换器
- [ ] 知道 BoolToVisibilityConverter 需要自定义
- [ ] 知道 Window 不是 UIElement
- [ ] 文件对话框需要窗口句柄
- [ ] 使用 CompositeTransform 而非 TransformGroup
- [ ] 使用主题资源而非硬编码颜色

---

**最后更新**: 2024  
**项目状态**: ✅ 编译通过，运行正常  
**维护者**: Nodify.WinUI 开发团队
