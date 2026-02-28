# WinUI 3 兼容性说明

本文档说明了节点编辑器在 WinUI 3 平台上的特殊实现和与 WPF 的主要差异。

## 📋 WinUI 3 vs WPF 差异对照

### 1. ❌ StringFormat 不可用

**WPF 写法：**
```xml
<TextBlock Text="{Binding ViewportScale, StringFormat='Zoom: {0:P0}'}"/>
```

**WinUI 3 写法：**
```xml
<!-- 在资源中声明转换器 -->
<converters:PercentageConverter x:Key="PercentageConverter"/>

<!-- 使用转换器 -->
<TextBlock Text="{Binding ViewportScale, Converter={StaticResource PercentageConverter}}"/>
```

**解决方案：**
- 创建了 `PercentageConverter` 类来替代 `StringFormat`
- 位置：`Experimental/Converters/PercentageConverter.cs`
- 使用场景：
  - `MiniMapControl.xaml` - 小地图缩放显示
  - `NodeEditorCanvas.xaml` - 状态栏缩放显示

---

### 2. 🔄 Transform 系统差异

**WPF：**
- 使用 `TransformGroup` 和多个 `Transform`
- 支持 `MatrixTransform`

**WinUI 3：**
- 主要使用 `CompositeTransform`（合并平移、缩放、旋转等）
- 性能更好，API 更简洁

**本项目实现：**
```xml
<Canvas.RenderTransform>
    <CompositeTransform x:Name="CanvasTransform"/>
</Canvas.RenderTransform>
```

```csharp
// 设置平移和缩放
CanvasTransform.TranslateX = offsetX;
CanvasTransform.TranslateY = offsetY;
CanvasTransform.ScaleX = scale;
CanvasTransform.ScaleY = scale;
```

---

### 3. 📂 文件对话框 API

**WPF：**
```csharp
var dialog = new Microsoft.Win32.SaveFileDialog();
if (dialog.ShowDialog() == true)
{
    string path = dialog.FileName;
}
```

**WinUI 3：**
```csharp
var picker = new Windows.Storage.Pickers.FileSavePicker();
picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
picker.FileTypeChoices.Add("Node Graph", new List<string> { ".nodegraph" });

// 需要设置窗口句柄 (WinUI 3 特殊要求)
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

var file = await picker.PickSaveFileAsync();
if (file != null)
{
    string path = file.Path;
}
```

**本项目实现：**
- 在 `SerializationHelper.cs` 中封装了文件保存/加载逻辑
- 在 `App.xaml.cs` 中公开了 `MainWindow` 属性供文件对话框使用

---

### 4. 🎨 主题资源命名

**WinUI 3 使用不同的资源键名：**

| 用途 | WPF | WinUI 3 |
|------|-----|---------|
| 卡片背景 | `CardBackgroundBrush` | `CardBackgroundFillColorDefaultBrush` |
| 控件边框 | `BorderBrush` | `ControlStrokeColorDefaultBrush` |
| 文本颜色 | `TextBrush` | `TextFillColorPrimaryBrush` |
| 次要文本 | `SecondaryTextBrush` | `TextFillColorSecondaryBrush` |
| 强调色 | `AccentBrush` | `AccentFillColorDefaultBrush` |

**本项目实现：**
```xml
<!-- 节点背景 -->
<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{ThemeResource ControlStrokeColorDefaultBrush}">

<!-- 标题文本 -->
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}"/>

<!-- 强调元素（端口、选中状态） -->
<Ellipse Fill="{ThemeResource AccentFillColorDefaultBrush}"/>
```

---

### 5. ⚠️ XAML 编译器差异

**WinUI 3 更严格的类型检查：**

```xml
<!-- ❌ WinUI 3 不允许 -->
<Run Text="{Binding Count, StringFormat='Count: {0}'}"/>

<!-- ✅ 必须使用转换器 -->
<Run Text="{Binding Count, Converter={StaticResource CountConverter}}"/>
```

**本项目处理：**
- 所有需要格式化的绑定都使用了转换器
- 不依赖 `StringFormat` 特性

---

### 6. 🪟 Window 类型层次结构

**WPF：**
- `Window` 继承自 `ContentControl` → `Control` → `FrameworkElement` → `UIElement`
- 可以在可视化树中查找

**WinUI 3：**
- `Window` 不是 `UIElement` 的子类
- 不能通过 `VisualTreeHelper` 查找
- 必须通过 `App` 类或其他方式访问

**本项目实现：**
```csharp
// ✅ 正确方式
private Window GetWindow()
{
    return (Application.Current as App)?.m_window;
}

// ❌ 错误方式（编译错误 CS8121）
private Window GetWindow()
{
    var parent = this.XamlRoot?.Content;
    if (parent is Window w)  // 错误！Window 不是 UIElement
        return w;
}
```

---

## ✅ 已处理的兼容性问题

### 编译错误修复

#### 错误 1: `StringFormat` 不支持
```
XamlCompiler error WMC0011: Unknown member 'StringFormat' on element 'Binding'
```

**解决方案：**
1. 创建 `PercentageConverter.cs`
2. 修改 `MiniMapControl.xaml`（第38行）
3. 修改 `NodeEditorCanvas.xaml`（第115行）

#### 错误 2: 窗口句柄访问
```
文件对话框需要窗口句柄才能显示
```

**解决方案：**
- 修改 `App.xaml.cs`，将 `m_window` 改为 `public static Window MainWindow`
- 在 `SerializationHelper.cs` 中使用：
  ```csharp
  var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
  WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
  ```

#### 错误 3: Window 类型模式匹配
```
error CS8121: "Window"类型的模式无法处理"UIElement"类型的表达式
```

**解决方案：**
- 移除了从可视化树中查找 Window 的代码
- 直接使用 `App.m_window` 获取窗口引用
- 修改文件：`NodeEditorCanvas.xaml.cs`（第 445-450 行）

#### 错误 4: 缺少 `using System;` 指令
```
error CS0246: 未能找到类型或命名空间名"EventHandler<>"
```

**解决方案：**
在以下文件顶部添加 `using System;`：
- `NodeControl.xaml.cs`
- `PortControl.xaml.cs`
- `ConnectionControl.xaml.cs`

---

## 🎯 最佳实践

### 1. 使用转换器而非 StringFormat
```csharp
public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return $"{doubleValue:P0}"; // 转换为百分比字符串
        }
        return "0%";
    }
}
```

### 2. 使用 CompositeTransform 优化性能
```csharp
// ✅ 推荐：一次性设置所有变换
CanvasTransform.TranslateX = offsetX;
CanvasTransform.TranslateY = offsetY;
CanvasTransform.ScaleX = scale;
CanvasTransform.ScaleY = scale;

// ❌ 避免：频繁创建新的 Transform 对象
Canvas.RenderTransform = new TranslateTransform { X = offsetX, Y = offsetY };
```

### 3. 使用主题资源实现自动主题切换
```xml
<!-- ✅ 使用主题资源 - 自动适配浅色/深色主题 -->
<Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"/>

<!-- ❌ 硬编码颜色 - 主题切换时不会改变 -->
<Border Background="#FFFFFF"/>
```

### 4. 异步处理文件对话框
```csharp
// WinUI 3 的文件对话框是异步的
private async void SaveButton_Click(object sender, RoutedEventArgs e)
{
    var success = await SerializationHelper.SaveToFileAsync(ViewModel);
    if (success)
    {
        // 保存成功
    }
}
```

---

## 📦 所需 NuGet 包

本项目使用以下 WinUI 3 特定的包：

```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.5.*" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.*" />
<PackageReference Include="System.Text.Json" Version="8.0.*" />
```

---

## 🔧 迁移指南（从 WPF 到 WinUI 3）

如果你有 WPF 节点编辑器代码，需要以下修改：

### 步骤 1: 替换所有 StringFormat
```bash
查找: StringFormat=
替换: Converter=
```

### 步骤 2: 更新主题资源
```bash
查找: CardBackgroundBrush
替换: CardBackgroundFillColorDefaultBrush
```

### 步骤 3: 更新 Transform
```bash
查找: TransformGroup
替换: CompositeTransform
```

### 步骤 4: 更新文件对话框
```bash
查找: Microsoft.Win32.SaveFileDialog
替换: Windows.Storage.Pickers.FileSavePicker
```

---

## 📚 参考资源

- [WinUI 3 官方文档](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [WinUI 3 迁移指南](https://learn.microsoft.com/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/winui3)
- [WinUI 3 主题资源](https://learn.microsoft.com/windows/apps/design/style/xaml-theme-resources)

---

## ✅ 验证清单

- [x] 所有 StringFormat 已替换为 Converter
- [x] 使用 CompositeTransform 进行变换
- [x] 文件对话框使用 WinUI 3 API
- [x] 使用 WinUI 3 主题资源
- [x] Window 访问使用 App 类而非可视化树
- [x] 所有文件都有必要的 using 指令
- [x] 项目可以成功编译
- [x] 运行时无错误

---

**最后更新**: 2024
**状态**: ✅ 所有已知兼容性问题已解决
