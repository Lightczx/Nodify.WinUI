# 快速测试指南

## 🚀 运行前检查清单

在运行应用程序之前，请确保：

- [x] 项目成功编译（0 个错误）
- [x] 所有 XAML 文件没有编译警告
- [x] 已安装 Windows App SDK
- [x] 目标平台设置为 `x64` 或 `x86`

---

## 🧪 关键功能测试

### 1. ✅ 启动测试

**测试步骤：**
1. 按 `F5` 启动应用程序
2. 验证主窗口正常显示
3. 验证示例页面加载成功

**预期结果：**
- ✅ 应用启动无崩溃
- ✅ 示例节点显示正常
- ✅ UI 布局正确

**常见问题：**
- ❌ 启动崩溃 → 检查 `App.xaml.cs` 和 `MainWindow.xaml`
- ❌ 白屏 → 检查 XAML 资源引用

---

### 2. 🔗 连接拖拽测试（关键！）

**这是最容易出错的功能，必须仔细测试！**

**测试步骤：**
1. 点击"创建节点"按钮创建 2-3 个节点
2. 从第一个节点的**输出端口**（右侧）拖出
3. 移动鼠标，观察连接线是否跟随
4. 连接到另一个节点的**输入端口**（左侧）
5. 释放鼠标

**预期结果：**
- ✅ 拖拽开始时不崩溃（这是之前修复的主要问题）
- ✅ 连接线平滑显示，跟随鼠标
- ✅ 连接线是贝塞尔曲线（不是直线）
- ✅ 成功连接后，连接线保持在两个端口之间
- ✅ 移动节点时，连接线跟随移动

**常见问题：**
- ❌ 拖拽瞬间崩溃 `ArgumentException` → 检查 `ConnectionControl.UpdatePath()` 是否为每个 Path 创建了独立的 Geometry
- ❌ 连接线不显示 → 检查点值验证逻辑
- ❌ 连接线是直线 → 检查 `CreateBezierGeometry()` 方法
- ❌ 连接线不跟随节点 → 检查 `PortControl.UpdatePosition()` 调用

**调试建议：**
```csharp
// 在 UpdatePath() 中添加日志
System.Diagnostics.Debug.WriteLine(
    $"UpdatePath: Source=({ViewModel.SourcePoint.X}, {ViewModel.SourcePoint.Y}), " +
    $"Target=({ViewModel.TargetPoint.X}, {ViewModel.TargetPoint.Y})");
```

---

### 3. 🎯 节点拖拽测试

**测试步骤：**
1. 点击节点的标题区域
2. 拖动节点到新位置
3. 释放鼠标

**预期结果：**
- ✅ 节点跟随鼠标移动
- ✅ 连接线实时更新位置
- ✅ 释放后节点停留在新位置

**常见问题：**
- ❌ 节点不移动 → 检查 `NodeControl` 的拖拽事件处理
- ❌ 节点移动但连接线不跟随 → 检查端口位置更新

---

### 4. 🔍 画布缩放和平移测试

**测试步骤：**
1. **缩放**：按住 `Ctrl` 并滚动鼠标滚轮
2. **平移**：按住 `Shift` 并拖动画布空白区域
3. **重置**：点击"重置视图"按钮

**预期结果：**
- ✅ 缩放平滑，范围 0.1x - 2.0x
- ✅ 平移流畅，无卡顿
- ✅ 状态栏显示当前缩放比例
- ✅ 小地图同步更新
- ✅ 重置视图恢复到 100% 缩放，居中位置

**常见问题：**
- ❌ 缩放不生效 → 检查 `CompositeTransform` 绑定
- ❌ 平移后位置错误 → 检查坐标转换逻辑

---

### 5. 🗺️ 小地图测试

**测试步骤：**
1. 创建多个节点，分散在画布上
2. 点击小地图切换可见性
3. 在小地图上点击不同位置

**预期结果：**
- ✅ 小地图显示所有节点的缩略图
- ✅ 视口矩形显示当前可见区域
- ✅ 点击小地图后，主画布跳转到对应位置
- ✅ 缩放比例显示正确（如 "100%"）

**常见问题：**
- ❌ 小地图不显示 → 检查转换器和数据绑定
- ❌ 缩放比例显示错误 → 检查 `PercentageConverter`

---

### 6. ⌨️ 键盘操作测试

**测试步骤：**
1. 选中一个节点
2. 按 `Delete` 键删除节点
3. 选中一个连接线（双击）
4. 按 `Delete` 键删除连接线

**预期结果：**
- ✅ 节点被删除
- ✅ 相关连接线一起删除
- ✅ 连接线单独删除

**常见问题：**
- ❌ `Delete` 不生效 → 检查键盘事件处理

---

### 7. 💾 保存和加载测试

**测试步骤：**
1. 创建几个节点和连接线
2. 点击"保存"按钮
3. 选择保存位置，输入文件名
4. 点击"清空画布"按钮
5. 点击"加载"按钮
6. 选择刚才保存的文件

**预期结果：**
- ✅ 保存成功（.nodegraph 文件）
- ✅ 文件包含 JSON 数据
- ✅ 加载成功，节点和连接恢复原状
- ✅ 节点位置、连接关系完全一致

**常见问题：**
- ❌ 保存对话框不显示 → 检查窗口句柄初始化
- ❌ 加载后节点位置错误 → 检查序列化逻辑
- ❌ 连接线未恢复 → 检查端口匹配逻辑

---

### 8. 🎨 主题测试

**测试步骤：**
1. 打开 Windows 设置 → 个性化 → 颜色
2. 切换"浅色"和"深色"主题
3. 观察应用界面变化

**预期结果：**
- ✅ 界面立即响应主题切换
- ✅ 所有控件颜色正确
- ✅ 文本清晰可读
- ✅ 强调色符合系统主题

**常见问题：**
- ❌ 主题切换后颜色错误 → 检查 `ThemeResource` 使用

---

## 🐛 常见运行时错误

### 错误 1: ArgumentException - Geometry 共享

**错误信息：**
```
System.ArgumentException: Value does not fall within the expected range.
at Path.set_Data(Geometry value)
```

**原因：**
同一个 Geometry 对象被赋值给多个 Path 控件

**解决方案：**
```csharp
// ❌ 错误
var geometry = CreateBezierGeometry(...);
ConnectionPath.Data = geometry;
SelectionPath.Data = geometry;

// ✅ 正确
ConnectionPath.Data = CreateBezierGeometry(...);
SelectionPath.Data = CreateBezierGeometry(...);
```

**参考：** `RUNTIME_FIXES.md` 问题 #1

---

### 错误 2: XamlParseException - 找不到资源

**错误信息：**
```
Cannot find a Resource with the Name/Key BoolToVisibilityConverter
```

**原因：**
转换器未在 XAML 资源中声明

**解决方案：**
```xml
<UserControl.Resources>
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
</UserControl.Resources>
```

---

### 错误 3: NullReferenceException - ViewModel 为 null

**错误信息：**
```
System.NullReferenceException: Object reference not set to an instance of an object.
```

**原因：**
在 DataContext 设置前访问 ViewModel

**解决方案：**
```csharp
private void UpdateSomething()
{
    // ✅ 总是检查 ViewModel 是否为 null
    if (ViewModel == null) return;
    
    // 使用 ViewModel
}
```

---

## 📊 性能测试

### 大型图表测试

**测试步骤：**
1. 创建 50+ 个节点
2. 创建 100+ 个连接线
3. 拖拽节点、缩放、平移

**预期结果：**
- ✅ UI 响应流畅（60 FPS）
- ✅ 拖拽无延迟
- ✅ 内存使用稳定

**性能问题诊断：**
- 使用 Visual Studio 诊断工具
- 检查 CPU 和内存使用
- 查找性能瓶颈

---

## 🔧 调试技巧

### 1. 启用输出日志

在关键位置添加日志：
```csharp
System.Diagnostics.Debug.WriteLine($"[NodeControl] Dragging to ({x}, {y})");
```

### 2. 断点调试

在以下位置设置断点：
- `ConnectionControl.UpdatePath()` - 连接线更新
- `NodeEditorCanvas.OnPointerPressed()` - 鼠标事件
- `PortControl.UpdatePosition()` - 端口位置

### 3. 实时可视化树

Visual Studio → 调试 → Windows → 实时可视化树

可以查看：
- XAML 元素层次
- 属性值
- 数据绑定状态

---

## ✅ 测试通过标准

**基础功能：**
- [x] 应用启动成功
- [x] 创建节点
- [x] 删除节点
- [x] 拖拽节点
- [x] 创建连接
- [x] 删除连接
- [x] 画布缩放
- [x] 画布平移
- [x] 保存图表
- [x] 加载图表

**高级功能：**
- [x] 小地图显示
- [x] 选中状态高亮
- [x] 连接线平滑曲线
- [x] 主题切换
- [x] 键盘快捷键

**稳定性：**
- [x] 无崩溃
- [x] 无内存泄漏
- [x] 无 XAML 错误
- [x] 性能流畅

---

## 📚 相关文档

- 🔧 [运行时错误修复](RUNTIME_FIXES.md)
- 📖 [WinUI 3 兼容性](WinUI3_COMPATIBILITY.md)
- 🐛 [问题排查指南](TROUBLESHOOTING_GUIDE.md)
- 🚀 [快速开始](QUICKSTART.md)

---

**最后更新**: 2024  
**维护者**: Nodify.WinUI 开发团队
