# 运行时错误修复日志

本文档记录了节点编辑器在运行时遇到的问题及其解决方案。

---

## 问题 #1: 拖拽连接线时 ArgumentException 崩溃

### 错误信息
```
System.ArgumentException: Value does not fall within the expected range.
HResult=0x80070057
at Path.set_Data(Geometry value)
at ConnectionControl.UpdatePath() line 55
```

### 问题描述
当用户尝试从节点端口拖出连接线时，应用程序立即崩溃。

### 根本原因
**🔥 核心问题：WinUI 3 不允许同一个 Geometry 对象被赋值给多个 Path 控件！**

在原始代码中：
```csharp
var geometry = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);
ConnectionPath.Data = geometry;  // ✅ 第一次赋值成功
SelectionPath.Data = geometry;   // ❌ 第二次赋值失败！
```

这在 WPF 中是允许的，但在 **WinUI 3 中每个 Path 必须有自己独立的 Geometry 实例**。

其他潜在原因（已验证不是主要原因）：
1. **未初始化的点值**：
   - `ConnectionViewModel` 创建时，`SourcePoint` 和 `TargetPoint` 都是默认值 `Point(0, 0)`
   - `PortViewModel.Position` 初始值也是 `default(Point)`
   - 当 `DataContextChanged` 触发时，`UpdatePath()` 尝试使用这些默认值创建 `PathGeometry`

2. **WinUI 3 的严格验证**：
   - WinUI 3 的 `Path.Data` 属性对无效几何体有严格验证
   - 某些情况下，默认值或未初始化的点可能导致无效的 `PathGeometry`

### 解决方案

#### ✅ 核心修复：为每个 Path 创建独立的 Geometry 实例

**文件**: `Experimental/View/ConnectionControl.xaml.cs`

```csharp
private void UpdatePath()
{
    if (ViewModel == null) return;

    // ✅ 验证点值有效性
    if (!IsValidPoint(ViewModel.SourcePoint) || !IsValidPoint(ViewModel.TargetPoint))
    {
        // 如果点值无效，不更新路径
        return;
    }

    // 🔥 关键修复：在 WinUI 3 中，每个 Path 需要自己的 Geometry 实例
    ConnectionPath.Data = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);
    SelectionPath.Data = CreateBezierGeometry(ViewModel.SourcePoint, ViewModel.TargetPoint);

    SelectionPath.Visibility = ViewModel.IsSelected ? Visibility.Visible : Visibility.Collapsed;
}

private bool IsValidPoint(Point point)
{
    // 检查 NaN、Infinity 或其他无效值
    return !double.IsNaN(point.X) && !double.IsNaN(point.Y) &&
           !double.IsInfinity(point.X) && !double.IsInfinity(point.Y);
}
```

**关键变化**：
- ❌ **错误**: `var geometry = CreateBezierGeometry(...); ConnectionPath.Data = geometry; SelectionPath.Data = geometry;`
- ✅ **正确**: `ConnectionPath.Data = CreateBezierGeometry(...); SelectionPath.Data = CreateBezierGeometry(...);`

**优点**：
- ✅ 符合 WinUI 3 的要求，每个 Path 有独立的 Geometry
- ✅ 防御性编程，避免无效值导致崩溃
- ✅ 支持延迟初始化
- ✅ 不会因为暂时的无效值而崩溃

#### 2. （可选）在 `NodeEditorViewModel.cs` 中初始化连接点值

**文件**: `Experimental/ViewModel/NodeEditorViewModel.cs`

```csharp
public void StartConnection(PortViewModel port)
{
    if (port == null) return;

    var model = new ConnectionModel();
    PendingConnection = new ConnectionViewModel(model);

    if (port.Direction == PortDirection.Output)
    {
        PendingConnection.SourcePort = port;
        // ✅ 立即初始化点值（可选，但推荐）
        PendingConnection.SourcePoint = port.Position;
        PendingConnection.TargetPoint = port.Position; // 从同一位置开始
    }
    else
    {
        PendingConnection.TargetPort = port;
        // ✅ 立即初始化点值（可选，但推荐）
        PendingConnection.SourcePoint = port.Position;
        PendingConnection.TargetPoint = port.Position;
    }
}
```

**优点**：
- ✅ 确保 `PendingConnection` 创建时就有有效的点值
- ✅ 连接线从端口位置开始，视觉效果更好
- ✅ 避免 `DataContextChanged` 时点值未初始化
- ℹ️ 此修复是可选的，因为 `IsValidPoint()` 验证已经足够

### 测试验证

**测试步骤**：
1. ✅ 运行应用程序
2. ✅ 点击示例中的"创建节点"按钮创建几个节点
3. ✅ 从一个节点的输出端口拖拽
4. ✅ 验证连接线正常显示
5. ✅ 移动鼠标，连接线跟随鼠标
6. ✅ 释放鼠标或连接到另一个输入端口

**预期结果**：
- ✅ 不再崩溃
- ✅ 连接线平滑显示
- ✅ 拖拽体验流畅

---

## 其他潜在问题及预防措施

### 问题 #2: Port.Position 未更新

**症状**：连接线位置不正确或不跟随节点移动

**原因**：`PortControl` 未调用 `UpdatePosition()` 更新其 ViewModel 的 `Position` 属性

**解决方案**：参考 `PortControl.xaml.cs` 中的 `Loaded` 和 `LayoutUpdated` 事件处理

### 问题 #3: 变换导致的坐标问题

**症状**：在缩放或平移后，拖拽连接或节点时位置不正确

**原因**：未考虑画布的 `CompositeTransform`

**解决方案**：
- 使用 `TransformToVisual()` 进行坐标转换
- 或在 `NodeEditorCanvas` 中处理变换后的坐标

---

## 调试技巧

### 1. 添加日志输出
```csharp
private void UpdatePath()
{
    if (ViewModel == null) 
    {
        System.Diagnostics.Debug.WriteLine("UpdatePath: ViewModel is null");
        return;
    }
    
    System.Diagnostics.Debug.WriteLine(
        $"UpdatePath: Source=({ViewModel.SourcePoint.X}, {ViewModel.SourcePoint.Y}), " +
        $"Target=({ViewModel.TargetPoint.X}, {ViewModel.TargetPoint.Y})");
    
    // ... rest of method
}
```

### 2. 使用条件断点
在 Visual Studio 中：
- 右键点击断点 → 条件
- 设置条件：`double.IsNaN(ViewModel.SourcePoint.X)`

### 3. 验证事件顺序
记录关键事件的调用顺序：
1. `DataContextChanged`
2. `PropertyChanged`
3. `UpdatePath()`
4. `Loaded` / `LayoutUpdated`

---

## 版本历史

| 版本 | 日期 | 问题 | 状态 |
|------|------|------|------|
| 1.0 | 2024 | ArgumentException in UpdatePath | ✅ 已修复 |

---

## 相关文档

- 📖 [WinUI 3 兼容性](WinUI3_COMPATIBILITY.md)
- 📖 [编译错误修复](COMPILATION_FIXES.md)
- 🧪 [测试清单](TESTING_CHECKLIST.md)

---

**最后更新**: 2024  
**维护者**: Nodify.WinUI 开发团队
