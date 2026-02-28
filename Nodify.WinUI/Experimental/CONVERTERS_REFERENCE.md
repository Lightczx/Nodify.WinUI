# 转换器参考文档

本文档列出了节点编辑器项目中所有可用的值转换器（Value Converters）。

---

## 📚 转换器列表

### 1. PercentageConverter

**文件位置：** `Experimental/Converters/PercentageConverter.cs`

**用途：** 将 double 类型的小数（0.0 - 1.0）转换为百分比字符串格式。

**输入类型：** `double`  
**输出类型：** `string`

**使用示例：**
```xml
<Page.Resources>
    <converters:PercentageConverter x:Key="PercentageConverter"/>
</Page.Resources>

<!-- 显示缩放比例 -->
<TextBlock Text="{Binding ViewportScale, Converter={StaticResource PercentageConverter}}"/>
```

**输入/输出示例：**
| 输入 (double) | 输出 (string) |
|--------------|--------------|
| 0.5 | "50%" |
| 1.0 | "100%" |
| 1.5 | "150%" |
| 0.25 | "25%" |

**特点：**
- ✅ 自动四舍五入到整数百分比
- ✅ 自动添加 % 符号
- ✅ 支持任意比例值
- ✅ 替代了 WPF 中的 `StringFormat='{}{0:P0}'`

**源代码：**
```csharp
public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return $"{Math.Round(d * 100)}%";
        }
        return "100%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && str.EndsWith("%"))
        {
            var numberPart = str.Substring(0, str.Length - 1);
            if (double.TryParse(numberPart, out double result))
            {
                return result / 100.0;
            }
        }
        return 1.0;
    }
}
```

**使用场景：**
- 🗺️ 小地图缩放显示（`MiniMapControl.xaml`）
- 🎨 画布状态栏缩放显示（`NodeEditorCanvas.xaml`）

---

### 2. BoolToVisibilityConverter

**文件位置：** `Experimental/Converters/BoolToVisibilityConverter.cs`

**用途：** 将布尔值转换为 Visibility 枚举值，控制元素的显示/隐藏。

**输入类型：** `bool`  
**输出类型：** `Visibility`

**使用示例：**
```xml
<UserControl.Resources>
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    
    <!-- 反转逻辑的转换器 -->
    <converters:BoolToVisibilityConverter x:Key="InvertedBoolToVisibilityConverter" 
                                          IsInverted="True"/>
</UserControl.Resources>

<!-- 当 IsSelected 为 true 时显示 -->
<Border Visibility="{Binding IsSelected, Converter={StaticResource BoolToVisibilityConverter}}"/>

<!-- 当 IsSelected 为 false 时显示（反转逻辑） -->
<Border Visibility="{Binding IsSelected, Converter={StaticResource InvertedBoolToVisibilityConverter}}"/>
```

**输入/输出示例（正常模式）：**
| 输入 (bool) | 输出 (Visibility) |
|------------|------------------|
| true | Visibility.Visible |
| false | Visibility.Collapsed |

**输入/输出示例（反转模式 IsInverted=True）：**
| 输入 (bool) | 输出 (Visibility) |
|------------|------------------|
| true | Visibility.Collapsed |
| false | Visibility.Visible |

**特点：**
- ✅ 支持正常和反转两种模式
- ✅ 支持双向绑定（ConvertBack）
- ✅ 使用 Collapsed 而非 Hidden（更好的性能）
- ✅ 替代了 WPF/UWP 的内置 `BooleanToVisibilityConverter`

**源代码：**
```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 获取或设置是否反转转换逻辑（默认 false）
    /// </summary>
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue = value is bool b && b;
        if (IsInverted) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            bool result = visibility == Visibility.Visible;
            return IsInverted ? !result : result;
        }
        return false;
    }
}
```

**使用场景：**
- 🔲 节点选中状态边框显示（`NodeControl.xaml`）
- 🎯 条件性显示/隐藏 UI 元素

---

## 🎨 如何创建自定义转换器

### 基本模板

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Nodify.WinUI.Experimental.Converters
{
    public class MyCustomConverter : IValueConverter
    {
        // 可选：添加可配置属性
        public string SomeProperty { get; set; }

        // 从源到目标的转换
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // 实现转换逻辑
            if (value is SourceType source)
            {
                return /* 转换后的值 */;
            }
            return /* 默认值 */;
        }

        // 从目标到源的转换（双向绑定）
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // 实现反向转换逻辑
            if (value is TargetType target)
            {
                return /* 转换后的值 */;
            }
            return /* 默认值 */;
        }
    }
}
```

### 使用步骤

1. **创建转换器类**（如上所示）

2. **在 XAML 中引入命名空间：**
```xml
xmlns:converters="using:Nodify.WinUI.Experimental.Converters"
```

3. **在资源中声明：**
```xml
<Page.Resources>
    <converters:MyCustomConverter x:Key="MyConverter"/>
</Page.Resources>
```

4. **在绑定中使用：**
```xml
<TextBlock Text="{Binding SomeProperty, Converter={StaticResource MyConverter}}"/>
```

---

## 📊 常用转换器模式

### 1. 数值格式化转换器

```csharp
public class NumberFormatConverter : IValueConverter
{
    public string Format { get; set; } = "N2"; // 默认格式

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
            return d.ToString(Format);
        if (value is int i)
            return i.ToString(Format);
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && double.TryParse(str, out double result))
            return result;
        return 0.0;
    }
}
```

### 2. Null 检查转换器

```csharp
public class NullToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isNull = value == null;
        if (IsInverted) isNull = !isNull;
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

### 3. 枚举到字符串转换器

```csharp
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Enum enumValue)
            return enumValue.ToString();
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && targetType.IsEnum)
            return Enum.Parse(targetType, str);
        return null;
    }
}
```

### 4. 颜色转换器

```csharp
public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Windows.UI.Color color)
            return new SolidColorBrush(color);
        if (value is string hexColor)
        {
            // 解析十六进制颜色字符串
            // ...
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is SolidColorBrush brush)
            return brush.Color;
        return Colors.Transparent;
    }
}
```

---

## 🔍 调试转换器

### 在转换器中添加日志

```csharp
public object Convert(object value, Type targetType, object parameter, string language)
{
    System.Diagnostics.Debug.WriteLine($"Convert: {value} ({value?.GetType().Name}) to {targetType.Name}");
    
    // 转换逻辑...
    var result = /* ... */;
    
    System.Diagnostics.Debug.WriteLine($"Result: {result}");
    return result;
}
```

### 使用断点

在 Visual Studio 中：
1. 在转换器的 `Convert` 或 `ConvertBack` 方法中设置断点
2. 运行程序（F5）
3. 当绑定触发时，断点会命中，可以查看值

---

## ⚠️ 常见错误和解决方案

### 错误 1: 转换器返回 null

**问题：**
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
{
    return null; // ❌ 可能导致绑定失败
}
```

**解决方案：**
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
{
    return DependencyProperty.UnsetValue; // ✅ 使用 UnsetValue
}
```

### 错误 2: 未处理异常类型

**问题：**
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
{
    return ((double)value).ToString(); // ❌ 如果 value 不是 double 会崩溃
}
```

**解决方案：**
```csharp
public object Convert(object value, Type targetType, object parameter, string language)
{
    if (value is double d)
        return d.ToString();
    return DependencyProperty.UnsetValue; // ✅ 安全处理
}
```

### 错误 3: ConvertBack 未实现导致双向绑定失败

**问题：**
```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
{
    throw new NotImplementedException(); // ❌ 双向绑定会失败
}
```

**解决方案：**
```csharp
public object ConvertBack(object value, Type targetType, object parameter, string language)
{
    // 实现反向转换，或者返回默认值
    return DependencyProperty.UnsetValue;
}
```

---

## 📖 参考资料

### WinUI 3 官方文档
- [Data binding overview](https://learn.microsoft.com/en-us/windows/apps/develop/data-binding/)
- [IValueConverter Interface](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.data.ivalueconverter)

### 本项目相关文档
- `WinUI3_COMPATIBILITY.md` - WinUI 3 兼容性说明
- `COMPILATION_FIXES.md` - 编译问题修复日志
- `README.md` - 项目完整文档

---

**最后更新**: 2024  
**转换器数量**: 2 个  
**状态**: ✅ 完整且可用
