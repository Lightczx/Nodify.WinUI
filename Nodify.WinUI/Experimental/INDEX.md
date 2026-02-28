# 📚 文件索引和快速导航

## 🗂️ 文件组织结构

```
Experimental/
│
├── 📖 文档
│   ├── README.md .................... 完整功能文档和 API 说明
│   ├── QUICK_START.md ............... 快速入门指南 (用户友好版) 🆕
│   ├── QUICKSTART.md ................ 5分钟快速入门指南
│   ├── FEATURES.md .................. 功能清单和开发路线图 🆕
│   ├── TESTING_GUIDE.md ............. 详细测试指南 🆕
│   ├── PROJECT_SUMMARY.md ........... 项目总结和架构说明
│   ├── TESTING_CHECKLIST.md ......... 90+ 项功能测试清单
│   ├── WINUI3_COMPATIBILITY.md ...... WinUI 3 兼容性说明
│   ├── COMPILATION_FIXES.md ......... 编译错误修复记录 🔧
│   ├── RUNTIME_FIXES.md ............. 运行时错误修复记录 🆕
│   ├── TROUBLESHOOTING.md ........... 常见问题排查指南 🆕
│   ├── CONVERTERS_REFERENCE.md ...... 转换器参考文档 🆕
│   ├── FIXES_QUICK_REFERENCE.md ..... 修复快速参考
│   └── INDEX.md ..................... 本文件（导航索引）
│
├── 📁 Common/ ....................... MVVM 基础设施
│   ├── ObservableObject.cs .......... 实现 INotifyPropertyChanged
│   └── RelayCommand.cs .............. ICommand 实现
│
├── 📁 Model/ ........................ 数据模型（可序列化）
│   ├── PortModel.cs ................. 端口模型
│   ├── NodeModel.cs ................. 节点模型
│   ├── ConnectionModel.cs ........... 连接模型
│   └── EditorStateModel.cs .......... 编辑器状态（用于保存/加载）
│
├── 📁 ViewModel/ .................... 视图模型（业务逻辑）
│   ├── PortViewModel.cs ............. 端口视图模型
│   ├── NodeViewModel.cs ............. 节点视图模型
│   ├── ConnectionViewModel.cs ....... 连接视图模型
│   └── NodeEditorViewModel.cs ....... 主编辑器视图模型 ⭐
│
├── 📁 View/ ......................... UI 控件
│   ├── PortControl.xaml/cs .......... 端口 UI
│   ├── NodeControl.xaml/cs .......... 节点 UI
│   ├── ConnectionControl.xaml/cs .... 连接线 UI
│   ├── NodeEditorCanvas.xaml/cs ..... 主画布 ⭐
│   ├── PropertiesPanel.xaml/cs ...... 属性编辑面板 🆕
│   └── MiniMapControl.xaml/cs ....... 小地图
│
├── 📁 Helpers/ ...................... 辅助工具
│   └── SerializationHelper.cs ....... JSON 序列化助手
│
├── 📁 Converters/ ................... 值转换器
│   ├── BoolToVisibilityConverter.cs . 布尔到可见性转换 🆕
│   ├── NullToBoolConverter.cs ....... Null 检查转换 🆕
│   └── PercentageConverter.cs ....... 数值到百分比字符串 (WinUI 3)
│
└── 📁 示例
    ├── SamplePage.xaml .............. 示例页面 UI
    └── SamplePage.xaml.cs ........... 示例页面代码
```

---

## 📖 文档导航

### 我该看哪个文档？

| 你的需求 | 推荐文档 | 阅读时间 |
|---------|---------|---------|
| 🚀 快速开始使用 | `QUICK_START.md` ⭐ | 5 分钟 |
| ✅ 测试所有功能 | `TESTING_GUIDE.md` | 15 分钟 |
| 📋 查看功能列表 | `FEATURES.md` | 10 分钟 |
| 📚 了解所有功能 | `README.md` | 15 分钟 |
| 🏗️ 理解架构设计 | `PROJECT_SUMMARY.md` | 10 分钟 |
| 🔧 WinUI 3 兼容性 | `WINUI3_COMPATIBILITY.md` | 8 分钟 |
| 🔍 查找特定文件 | `INDEX.md` (本文件) | 2 分钟 |

---

## 🎯 常见任务快速查找

### 任务：集成到我的项目
👉 查看：`QUICKSTART.md` 步骤 1-3

### 任务：创建自定义节点
👉 查看：`README.md` → "自定义" 部分
👉 参考：`SamplePage.xaml.cs` → `CreateSampleNode` 方法

### 任务：修改节点外观
👉 编辑：`View/NodeControl.xaml`
👉 参考：`README.md` → "自定义节点外观"

### 任务：添加新的端口类型
👉 编辑：`Model/PortModel.cs` → `PortType` 枚举
👉 查看：`README.md` → "扩展端口类型"

### 任务：实现保存/加载
👉 查看：`Helpers/SerializationHelper.cs`
👉 参考：`View/NodeEditorCanvas.xaml.cs` → `SaveButton_Click`

### 任务：修改连接验证逻辑
👉 编辑：`ViewModel/NodeEditorViewModel.cs` → `CompleteConnection` 方法

### 任务：改变颜色主题
👉 编辑：
- 节点背景：`View/NodeControl.xaml`
- 连接线颜色：`View/ConnectionControl.xaml`
- 端口颜色：`View/PortControl.xaml`

---

## 🔑 核心类速查

### 主入口点
```
NodeEditorCanvas (View)
    ↓
NodeEditorViewModel (ViewModel) ← 从这里开始
    ↓
NodeViewModel / ConnectionViewModel
```

### 创建节点
```csharp
// 文件: ViewModel/NodeEditorViewModel.cs
public void AddNode(Point position)
```

### 创建连接
```csharp
// 文件: ViewModel/NodeEditorViewModel.cs
public void CompleteConnection(PortViewModel port)
```

### 保存状态
```csharp
// 文件: ViewModel/NodeEditorViewModel.cs
public EditorStateModel GetEditorState()
```

### 加载状态
```csharp
// 文件: ViewModel/NodeEditorViewModel.cs
public void LoadEditorState(EditorStateModel state)
```

---

## 📐 架构层次

```
┌─────────────────────────────────────────┐
│  View Layer (XAML + CodeBehind)        │
│  - NodeEditorCanvas                     │
│  - NodeControl, PortControl, etc.      │
└────────────────┬────────────────────────┘
                 │ DataBinding
┌────────────────▼────────────────────────┐
│  ViewModel Layer (Business Logic)      │
│  - NodeEditorViewModel ⭐               │
│  - NodeViewModel, ConnectionViewModel   │
└────────────────┬────────────────────────┘
                 │ References
┌────────────────▼────────────────────────┐
│  Model Layer (Data)                     │
│  - NodeModel, PortModel, etc.           │
└─────────────────────────────────────────┘
```

---

## 🎨 样式文件位置

| 元素 | XAML 文件 | 关键样式 |
|-----|----------|---------|
| 节点外框 | `NodeControl.xaml` | `Border` 元素 |
| 节点标题 | `NodeControl.xaml` | `TitleBar` Grid |
| 端口圆形 | `PortControl.xaml` | `PortEllipse` |
| 连接线 | `ConnectionControl.xaml` | `ConnectionPath` |
| 画布背景 | `NodeEditorCanvas.xaml` | `BackgroundCanvas` |
| 工具栏 | `NodeEditorCanvas.xaml` | `StackPanel` (top-left) |
| 小地图 | `MiniMapControl.xaml` | 整个文件 |

---

## 🔧 扩展点位置

### 添加新命令
📍 `ViewModel/NodeEditorViewModel.cs`
```csharp
// 构造函数中添加
public ICommand MyNewCommand { get; }
```

### 添加节点属性
📍 `Model/NodeModel.cs` (数据)
📍 `ViewModel/NodeViewModel.cs` (包装)

### 修改连接线绘制
📍 `View/ConnectionControl.xaml.cs` → `CreateBezierGeometry` 方法

### 修改小地图渲染
📍 `View/MiniMapControl.xaml.cs` → `UpdateMiniMap` 方法

---

## 📊 代码度量

| 类别 | 文件数 | 代码行数 |
|-----|-------|---------|
| Model | 4 | ~250 |
| ViewModel | 4 | ~550 |
| View | 10 (5×2) | ~1200 |
| Helpers | 2 | ~150 |
| 文档 | 5 | ~1500 |
| **总计** | **25** | **~3650** |

---

## 🎓 学习路径

### 第一天：基础理解
1. 📖 阅读 `QUICKSTART.md`
2. 🏃 运行 `SamplePage`
3. 🎮 测试所有交互功能

### 第二天：深入学习
1. 📖 阅读 `README.md` 全部内容
2. 🔍 浏览 ViewModel 代码
3. 🎨 修改一个样式试试

### 第三天：架构掌握
1. 📖 阅读 `PROJECT_SUMMARY.md`
2. 🏗️ 理解 MVVM 数据流
3. 🔧 实现一个自定义功能

### 第四天：专家级
1. 🧪 完成 `TESTING_CHECKLIST.md`
2. 🚀 创建自己的节点类型
3. 💾 实现自定义序列化

---

## 🆘 问题排查

### 编译错误
1. 检查所有文件是否添加到项目
2. 检查命名空间是否正确
3. 查看 `QUICKSTART.md` → "常见问题"

### 运行时错误
1. 检查 `App.xaml.cs` 中的 `m_window` 是否为 public
2. 检查是否添加了 `BoolToVisibilityConverter` 资源
3. 查看输出窗口的绑定错误

### 功能不工作
1. 使用 `TESTING_CHECKLIST.md` 逐项检查
2. 在关键方法设置断点调试
3. 检查 ViewModel 是否正确绑定

---

## 📞 获取帮助

1. 📖 首先查阅相关文档
2. 🔍 使用此索引文件快速定位
3. 🐛 检查 `TESTING_CHECKLIST.md`
4. 💡 参考 `SamplePage.xaml.cs` 示例

---

## ✨ 快速开始（复制粘贴版）

```xml
<!-- MainWindow.xaml -->
<Window xmlns:view="using:Nodify.WinUI.Experimental.View"
        xmlns:converters="using:Nodify.WinUI.Experimental.Converters">
    <Window.Resources>
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    </Window.Resources>
    <view:NodeEditorCanvas x:Name="NodeEditor"/>
</Window>
```

```csharp
// MainWindow.xaml.cs
using Nodify.WinUI.Experimental.ViewModel;

public MainWindow()
{
    InitializeComponent();
    NodeEditor.ViewModel = new NodeEditorViewModel();
}
```

完成！🎉

---

**提示**: 将此文件添加为书签，方便快速查找！
