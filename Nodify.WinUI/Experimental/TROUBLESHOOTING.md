# 🐛 常见问题排查指南

本文档提供快速的问题排查步骤和解决方案。

---

## 🚨 运行时崩溃

### 问题：拖拽连接线时崩溃 - ArgumentException

**症状**：
```
System.ArgumentException: Value does not fall within the expected range.
at ConnectionControl.UpdatePath()
```

**快速修复**：
✅ **已修复** - 请参考 [RUNTIME_FIXES.md](RUNTIME_FIXES.md#问题-1-拖拽连接线时-argumentexception-崩溃)

**验证**：
1. 重新编译项目
2. 从端口拖拽连接线
3. 确认不再崩溃

---

### 问题：XAML 解析异常 - BoolToVisibilityConverter

**症状**：
```
Microsoft.UI.Xaml.Markup.XamlParseException
Cannot find a Resource with the Name/Key BoolToVisibilityConverter
```

**快速修复**：
✅ **已修复** - `BoolToVisibilityConverter` 已创建在 `Converters/` 文件夹

**验证**：
1. 检查 `Experimental/Converters/BoolToVisibilityConverter.cs` 存在
2. 重新编译项目

---

## 🔧 编译错误

### 问题：StringFormat 不支持

**症状**：
```
XamlCompiler error WMC0011: Unknown member 'StringFormat' on element 'Binding'
```

**原因**：WinUI 3 不支持 `Binding.StringFormat`

**解决方案**：
✅ **已修复** - 使用 `PercentageConverter` 替代

**详细信息**: [COMPILATION_FIXES.md](COMPILATION_FIXES.md)

---

### 问题：缺少 using System;

**症状**：
```
error CS0246: 未能找到类型或命名空间名"EventHandler<>"
```

**快速修复**：
✅ **已修复** - 所有文件已添加必要的 `using System;`

**影响的文件**：
- `NodeControl.xaml.cs`
- `PortControl.xaml.cs`
- `ConnectionControl.xaml.cs`

---

### 问题：Window 类型匹配错误

**症状**：
```
error CS8121: "Window"类型的模式无法处理"UIElement"类型的表达式
```

**原因**：WinUI 3 中 `Window` 不继承自 `UIElement`

**解决方案**：
✅ **已修复** - 简化了 `GetWindow()` 方法

---

## 🎨 UI 问题

### 问题：节点不显示或位置错误

**可能原因**：
1. Canvas 变换未正确应用
2. 节点位置未初始化
3. 数据绑定问题

**排查步骤**：
```csharp
// 在 NodeControl.xaml.cs 中添加调试
private void NodeControl_Loaded(object sender, RoutedEventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"Node loaded at ({ViewModel.X}, {ViewModel.Y})");
}
```

**解决方案**：
1. 确认 `NodeViewModel.X` 和 `Y` 已设置
2. 检查 `Canvas.Left` 和 `Canvas.Top` 绑定
3. 验证 Canvas 的 `RenderTransform` 正确

---

### 问题：连接线不显示

**可能原因**：
1. 端口位置未更新
2. 连接点值无效
3. Path 的 Stroke 颜色与背景相同

**排查步骤**：
```csharp
// 在 PortControl.xaml.cs 中验证位置
private void UpdatePosition()
{
    var position = this.TransformToVisual(canvas).TransformPoint(new Point(0, 0));
    System.Diagnostics.Debug.WriteLine($"Port {ViewModel.Name} at ({position.X}, {position.Y})");
    ViewModel.Position = position;
}
```

**解决方案**：
1. 确认 `PortControl.UpdatePosition()` 被调用
2. 检查 `ConnectionViewModel.UpdatePoints()` 被正确调用
3. 验证 Path 的 Stroke 颜色设置

---

### 问题：小地图不更新

**可能原因**：
1. 数据绑定未正确设置
2. PropertyChanged 事件未触发

**解决方案**：
1. 检查 `MiniMapControl` 的 DataContext
2. 验证 `NodeEditorViewModel` 的 `Nodes` 和 `Connections` 是 `ObservableCollection`
3. 确认节点移动时调用 `OnPropertyChanged(nameof(X))` 和 `OnPropertyChanged(nameof(Y))`

---

## 🔍 调试技巧

### 1. 启用 XAML 绑定调试

在 Visual Studio 输出窗口中查看绑定错误：
- 调试 → 窗口 → 输出
- 筛选：显示来自"调试"的输出

### 2. 添加断点验证

关键位置：
- `NodeControl_PointerPressed` - 节点选中
- `Port_ConnectionStarted` - 开始连接
- `UpdatePendingConnection` - 更新连接线
- `CompleteConnection` - 完成连接

### 3. 使用 Live Visual Tree

调试时：
- 调试 → 窗口 → Live Visual Tree
- 检查控件层次结构
- 验证 DataContext 绑定

### 4. 性能分析

如果 UI 卡顿：
- 调试 → 性能探查器
- 选择"XAML UI 响应性"
- 查找慢速布局或渲染

---

## 📝 检查清单

遇到问题时，按顺序检查：

### 编译前
- [ ] 所有文件都已保存
- [ ] 清理解决方案（生成 → 清理解决方案）
- [ ] 重新生成（生成 → 重新生成解决方案）

### 运行前
- [ ] 检查 `SamplePage.xaml` 是否在 `MainWindow.xaml` 中引用
- [ ] 确认所有转换器已注册
- [ ] 验证 XAML 命名空间引用正确

### 测试时
- [ ] 逐个测试功能（创建节点 → 移动 → 连接）
- [ ] 检查输出窗口的错误信息
- [ ] 使用 Live Visual Tree 验证 UI 结构

---

## 🆘 仍然有问题？

### 1. 查看详细文档

- 📖 [编译错误修复](COMPILATION_FIXES.md)
- 📖 [运行时错误修复](RUNTIME_FIXES.md)
- 📖 [WinUI 3 兼容性](WINUI3_COMPATIBILITY.md)

### 2. 启用详细日志

在 `NodeEditorViewModel.cs` 中添加：
```csharp
private void LogDebug(string message)
{
    System.Diagnostics.Debug.WriteLine($"[NodeEditor] {message}");
}
```

### 3. 验证完整性

运行测试清单：
- 📋 [测试清单](TESTING_CHECKLIST.md)

### 4. 重置到干净状态

```bash
# 在项目目录执行
git clean -xfd
# 然后重新编译
```

---

## 🎯 快速参考

| 问题类型 | 查看文档 |
|---------|---------|
| 编译错误 | [COMPILATION_FIXES.md](COMPILATION_FIXES.md) |
| 运行时崩溃 | [RUNTIME_FIXES.md](RUNTIME_FIXES.md) |
| WinUI 3 差异 | [WINUI3_COMPATIBILITY.md](WINUI3_COMPATIBILITY.md) |
| 功能不工作 | [TESTING_CHECKLIST.md](TESTING_CHECKLIST.md) |
| 如何使用 | [QUICKSTART.md](QUICKSTART.md) |
| API 说明 | [README.md](README.md) |

---

**提示**：90% 的问题都可以通过"清理解决方案 + 重新生成"解决！

**最后更新**: 2024  
**维护者**: Nodify.WinUI 开发团队
