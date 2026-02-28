# 🚀 立即开始 - 3 分钟集成

## ⚡ 超快速开始（复制粘贴即可）

### 步骤 1: 打开 MainWindow.xaml

在文件顶部找到 `<Window>` 标签，添加命名空间：

```xml
<Window
    ...
    xmlns:experimental="using:Nodify.WinUI.Experimental">
```

### 步骤 2: 添加节点编辑器

在 Window 内容区域添加：

```xml
<experimental:SamplePage/>
```

### 步骤 3: 运行！

按 `F5` 运行项目，享受节点编辑器！

---

## 📋 完整示例

```xml
<Window
    x:Class="Nodify.WinUI.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:experimental="using:Nodify.WinUI.Experimental"
    Title="节点编辑器">
    
    <experimental:SamplePage/>
    
</Window>
```

---

## 🎮 基本操作

### 节点操作
- **拖动节点**: 点击标题栏拖动
- **选中节点**: 点击节点任意位置
- **创建节点**: 点击工具栏的 "添加节点" 按钮

### 连接操作
- **创建连接**: 从输出端口拖到输入端口
- **删除连接**: 双击连接线

### 画布操作
- **平移画布**: 按住鼠标中键拖动（或按住空格+左键）
- **缩放画布**: `Ctrl` + 鼠标滚轮
- **重置视图**: 点击工具栏的 "重置视图" 按钮

### 文件操作
- **保存**: 点击 "保存" 按钮或按 `Ctrl+S`
- **加载**: 点击 "加载" 按钮或按 `Ctrl+O`
- **新建**: 点击 "清空" 按钮

---

## 🎯 测试功能

### 快速测试清单

1. ✅ 点击 "添加节点" 创建几个节点
2. ✅ 拖动节点标题栏移动它们
3. ✅ 从一个节点的输出端口拖线到另一个节点的输入端口
4. ✅ 双击连接线删除它
5. ✅ 用鼠标中键拖动画布平移
6. ✅ 用 Ctrl+滚轮缩放画布
7. ✅ 点击 "保存" 保存到文件
8. ✅ 点击 "清空" 清除所有节点
9. ✅ 点击 "加载" 重新加载保存的文件
10. ✅ 查看小地图显示整个画布

---

## 🔧 如果遇到编译错误

### 已修复的问题
所有编译错误都已修复！如果仍有问题：

1. **清理并重新生成**
   ```
   生成 -> 清理解决方案
   生成 -> 重新生成解决方案
   ```

2. **检查 NuGet 包**
   ```
   工具 -> NuGet 包管理器 -> 管理解决方案的 NuGet 包
   点击 "还原"
   ```

3. **查看修复文档**
   - `FIXES_QUICK_REFERENCE.md` - 快速参考
   - `COMPILATION_FIXES.md` - 详细说明

---

## 📚 下一步学习

### 初级（5 分钟）
✅ `QUICKSTART.md` - 详细的快速入门

### 中级（15 分钟）
✅ `README.md` - 完整功能文档
✅ `TESTING_CHECKLIST.md` - 90+ 测试项

### 高级（30 分钟）
✅ `PROJECT_SUMMARY.md` - 架构设计
✅ `WINUI3_COMPATIBILITY.md` - 平台兼容性

---

## 💡 自定义节点

想创建自己的节点？参考 `SamplePage.xaml.cs`:

```csharp
// 创建一个新节点
var node = new NodeViewModel
{
    Id = Guid.NewGuid(),
    Title = "我的节点",
    Content = "节点内容",
    X = 100,
    Y = 100
};

// 添加输入端口
node.InputPorts.Add(new PortViewModel
{
    Id = Guid.NewGuid(),
    Name = "输入1",
    IsInput = true
});

// 添加输出端口
node.OutputPorts.Add(new PortViewModel
{
    Id = Guid.NewGuid(),
    Name = "输出1",
    IsInput = false
});

// 添加到编辑器
ViewModel.Nodes.Add(node);
```

---

## 🎨 自定义样式

所有控件都支持 WinUI 3 样式自定义。查看 XAML 文件：
- `NodeControl.xaml` - 节点样式
- `ConnectionControl.xaml` - 连接线样式
- `PortControl.xaml` - 端口样式

---

## ✨ 完成！

现在您已经成功集成了节点编辑器！

**享受强大的可视化编程体验！** 🎉

---

## 📞 需要帮助？

查看文档：
- `FINAL_REPORT.md` - 完整总结报告
- `INDEX.md` - 文件导航索引
- `README.md` - 完整功能文档

---

**祝开发愉快！** 🚀
