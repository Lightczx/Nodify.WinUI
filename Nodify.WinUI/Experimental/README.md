# WinUI 3 Node Editor - 节点编辑器控件

这是一个功能完整的 WinUI 3 节点编辑器控件实现，采用 MVVM 架构。

## 功能特性

### ✅ 已实现功能

1. **节点管理**
   - ✓ 节点可以通过拖拽标题栏移动
   - ✓ 节点包含标题、内容区域
   - ✓ 支持多个输入和输出端口
   - ✓ 节点选中状态显示

2. **连接功能**
   - ✓ 从端口拖拽创建连接线
   - ✓ 使用贝塞尔曲线绘制连接
   - ✓ 双击连接线取消连接
   - ✓ 自动验证连接（防止连接到同一节点）
   - ✓ 防止重复连接

3. **编辑器功能**
   - ✓ 鼠标中键拖拽平移视图
   - ✓ Ctrl+滚轮缩放视图
   - ✓ 网格背景
   - ✓ 小地图导航
   - ✓ 工具栏快捷操作

4. **序列化**
   - ✓ 保存编辑器状态到文件（.nodegraph）
   - ✓ 从文件加载编辑器状态
   - ✓ 使用 System.Text.Json

## 项目结构

```
Experimental/
├── Common/                     # 基础类
│   ├── ObservableObject.cs    # MVVM 基类
│   └── RelayCommand.cs        # 命令实现
├── Model/                      # 数据模型
│   ├── PortModel.cs           # 端口模型
│   ├── NodeModel.cs           # 节点模型
│   ├── ConnectionModel.cs     # 连接模型
│   └── EditorStateModel.cs    # 编辑器状态模型
├── ViewModel/                  # 视图模型
│   ├── PortViewModel.cs       # 端口视图模型
│   ├── NodeViewModel.cs       # 节点视图模型
│   ├── ConnectionViewModel.cs # 连接视图模型
│   └── NodeEditorViewModel.cs # 编辑器视图模型
├── View/                       # 视图控件
│   ├── PortControl.xaml/cs    # 端口控件
│   ├── NodeControl.xaml/cs    # 节点控件
│   ├── ConnectionControl.xaml/cs # 连接线控件
│   ├── NodeEditorCanvas.xaml/cs  # 编辑器画布
│   └── MiniMapControl.xaml/cs # 小地图控件
├── Helpers/                    # 辅助类
│   └── SerializationHelper.cs # 序列化助手
├── Converters/                 # 值转换器
│   └── BoolToVisibilityConverter.cs
└── SamplePage.xaml/cs         # 示例页面
```

## 使用方法

### 基本使用

1. **在 XAML 中使用编辑器**

```xml
<Page xmlns:view="using:Nodify.WinUI.Experimental.View">
    <view:NodeEditorCanvas x:Name="NodeEditor"/>
</Page>
```

2. **在代码中初始化**

```csharp
var viewModel = new NodeEditorViewModel();
NodeEditor.ViewModel = viewModel;
```

### 创建节点

```csharp
// 方式1: 使用命令
viewModel.AddNode(new Point(100, 100));

// 方式2: 手动创建
var nodeModel = new NodeModel("My Node", 100, 100)
{
    Content = "Node content"
};
nodeModel.InputPorts.Add(new PortModel("Input", PortDirection.Input));
nodeModel.OutputPorts.Add(new PortModel("Output", PortDirection.Output));

var nodeViewModel = new NodeViewModel(nodeModel);
viewModel.Nodes.Add(nodeViewModel);
```

### 创建连接

```csharp
// 连接是通过 UI 交互自动创建的
// 也可以编程方式创建：
var connection = new ConnectionViewModel(sourcePort, targetPort);
viewModel.Connections.Add(connection);
```

### 保存和加载

```csharp
// 保存
var state = viewModel.GetEditorState();
var json = SerializationHelper.Serialize(state);
// 或者保存到文件
await SerializationHelper.SaveToFileAsync(state, file);

// 加载
var state = SerializationHelper.Deserialize(json);
// 或者从文件加载
var state = await SerializationHelper.LoadFromFileAsync(file);
viewModel.LoadEditorState(state);
```

## 键盘快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl + N` | 添加新节点 |
| `Ctrl + +` | 放大视图 |
| `Ctrl + -` | 缩小视图 |
| `Ctrl + 0` | 重置视图 |
| `Ctrl + S` | 保存 |
| `Ctrl + O` | 打开 |
| `双击画布` | 在鼠标位置创建节点 |
| `双击连接线` | 删除连接 |

## 鼠标操作

| 操作 | 功能 |
|------|------|
| `拖拽节点标题栏` | 移动节点 |
| `拖拽端口` | 创建连接 |
| `鼠标中键拖拽` | 平移视图 |
| `Ctrl + 滚轮` | 缩放视图（朝向鼠标位置） |
| `双击连接线` | 删除连接 |
| `双击画布` | 创建节点 |

## 自定义

### 自定义节点外观

编辑 `View/NodeControl.xaml` 修改节点的视觉样式。

### 自定义连接线样式

编辑 `View/ConnectionControl.xaml` 修改连接线颜色、粗细等。

### 扩展端口类型

在 `Model/PortModel.cs` 中添加新的 `PortType` 枚举值：

```csharp
public enum PortType
{
    Default,
    Execution,
    Data,
    Event,
    // 添加自定义类型
    CustomType
}
```

### 添加节点验证

在 `NodeEditorViewModel.CompleteConnection` 方法中添加自定义验证逻辑：

```csharp
public void CompleteConnection(PortViewModel port)
{
    // 添加类型检查
    if (sourcePort.Type != targetPort.Type)
        return; // 拒绝不同类型的连接
    
    // ... 原有逻辑
}
```

## 示例

查看 `SamplePage.xaml.cs` 获取完整示例，展示如何：
- 初始化编辑器
- 创建示例节点
- 配置端口

## 技术细节

### MVVM 模式
- **Model**: 纯数据类，支持 JSON 序列化
- **ViewModel**: 实现 `INotifyPropertyChanged`，管理业务逻辑
- **View**: WinUI 3 用户控件，纯 UI 层

### 性能优化
- 使用 `ObservableCollection` 自动更新 UI
- 连接线使用贝塞尔曲线优化绘制
- 小地图使用简化渲染

### 坐标系统
- 画布使用 `CompositeTransform` 实现平移和缩放
- 端口位置实时计算并缓存在 ViewModel 中
- 连接线根据端口位置自动更新

## 已知限制

1. 小地图视口大小目前是硬编码的（800x600）
2. 网格大小固定为 20 像素
3. 缩放范围限制在 0.1 到 5.0 之间
4. 未实现撤销/重做功能
5. 未实现多选和框选功能

## 未来扩展建议

- [ ] 添加撤销/重做（命令模式）
- [ ] 实现多选和框选
- [ ] 添加节点分组功能
- [ ] 实现节点搜索和过滤
- [ ] 添加节点模板系统
- [ ] 支持节点折叠/展开
- [ ] 实现连接线路由算法
- [ ] 添加注释和便签功能

## 许可

此代码作为示例提供，可自由使用和修改。
