# GenshinGamePlay

参考原神的 GamePlay 框架，基于 TaoTie 轻量级 Unity 框架构建，涵盖战斗（AbilitySystem）、解谜（SceneGroup）、怪物 AI、剧情（Story）、相机控制等完整游戏玩法系统。

使用 Json 序列化的 Graph 编辑器进行可视化配置，二进制（Nino）/Json序列化的配置加载，YooAsset 管理资源,架构方便支持热更。

## 目录

- [功能概览](#功能概览)
- [项目结构](#项目结构)
- [快速开始](#快速开始)
- [配置工具链](#配置工具链)
- [技术栈](#技术栈)
- [引用或参考](#引用或参考)
- [付费插件](#付费插件)
- [使用文档](#使用文档)

## 功能概览

| 模块 | 说明 |
|------|------|
| **AbilitySystem** | GAS 风格的战斗框架。Ability / Modifier / Mixin 三层结构，支持状态触发、攻击判定、数值修改、Timeline 联动 |
| **FSM 状态机** | 基于 AnimatorController 编辑，导出 JSON 驱动运行时行为，支持 Timeline Marker 联动 |
| **AI 框架** | 知识-决策-控制三层架构。决策树 + 行为树，BrainModule 模块化（感知、目标、威胁、寻路、技能、移动、姿态） |
| **SceneGroup** | 机关/关卡系统。舞台-演员-剧本隐喻，支持实体、触发区域、流程阶段、事件监听、寻路路径 |
| **Story 剧情系统** | Graph 编辑器编排，支持分支节点、并行节点、内嵌 Timeline |
| **Camera 系统** | 基于状态栈的相机框架，head/body/other 三层插件，支持过渡混合、震动、遮挡前推 |
| **Numeric 数值系统** | 属性公式计算，支持 FormulaStringFx 表达式替换 |
| **Move 移动系统** | 策略模式，支持 ORCA 动态避障寻路（DotRecast）、Pathfinding 寻路组件 |
| **Environment 环境** | 光照、天空盒管理，优先级调度 |
| **Billboard 血条** | 头顶信息显示，插件化架构 |
| **Camp 阵营** | 敌友关系管理 |
| **UI 框架** | UIManager 窗口管理，Loading/Main/Lobby/Story 等面板 |
| **资源管理** | YooAsset，支持 EditorSimulate/HostPlay/WebPlay 多模式 |

## 项目结构

```
GenshinGamePlay/
├── Assets/
│   ├── AssetsPackage/          # 游戏资源包（YooAsset 管理）
│   │   ├── Audio/              # 音频
│   │   ├── Code/               # 热更 DLL（Hotfix + AOT）
│   │   ├── Config/             # 二进制配置文件
│   │   ├── EditConfig/         # 编辑器配置（Abilities 等）
│   │   ├── Effect/             # 特效
│   │   ├── Fonts/              # 字体
│   │   ├── GraphAssets/        # Graph 编辑器资产
│   │   ├── Navmesh/            # 寻路网格
│   │   ├── RenderAssets/       # 渲染资产
│   │   ├── Scenes/             # 场景文件
│   │   ├── Shaders/            # 着色器
│   │   ├── SkyBox/             # 天空盒
│   │   ├── StoryTimeline/      # 剧情 Timeline 资产
│   │   ├── UI/                 # UI 资源
│   │   ├── UIGame/             # 游戏 UI 资源
│   │   └── Unit/               # 角色模型、动画、FSM 配置
│   ├── Scripts/
│   │   ├── Code/               # 热更新代码（GamePlay 逻辑层）
│   │   │   ├── Game/
│   │   │   │   ├── Component/  # 组件（Ability/AI/Combat/Fsm/Move/Numeric/SceneGroup/...）
│   │   │   │   ├── Entity/     # 实体（Actor/Avatar/Character/Monster/Gadget/Unit/Zone/...）
│   │   │   │   ├── Scene/      # 场景（LoginScene/MapScene）
│   │   │   │   ├── System/     # 系统（Ability/AI/Billboard/Camp/Combat/Entity/Environment/Fsm/Move/Navmesh/Numeric/SceneGroup/Story/View）
│   │   │   │   ├── UI/         # 通用 UI（Loading/Main/Update/Common）
│   │   │   │   └── UIGame/     # 游戏 UI（Lobby/Main/Story）
│   │   │   ├── Module/         # 框架模块（Camera/Config/CoroutineLock/I18N/Input/Resource/Scene/UI/Update/...）
│   │   │   └── Entry.cs        # 程序入口
│   │   ├── Editor/             # 编辑器扩展
│   │   │   ├── ArtEditor/      # 美术工具（图集）
│   │   │   ├── BuildEditor/    # 打包工具
│   │   │   ├── Common/         # 通用编辑器
│   │   │   ├── DesignEditor/   # 策划工具
│   │   │   │   ├── ConfigEditor/   # 配置编辑器
│   │   │   │   ├── FsmEditor/      # 状态机编辑器
│   │   │   │   ├── GraphEditor/    # Graph 编辑器（AI/SceneGroup/Story）
│   │   │   │   └── TimeLineEditor/ # Timeline 编辑器
│   │   │   ├── UIManager/      # UI 管理编辑器
│   │   │   └── YooAssets/      # YooAsset 编辑器扩展
│   │   ├── Mono/               # 非热更代码（框架核心层）
│   │   │   ├── Core/           # 核心（ManagerProvider/ObjectPool/数据结构）
│   │   │   ├── Helper/         # 工具类（Platform/Physics/Mesh/Json/String/...）
│   │   │   ├── Module/         # 框架模块（AI/Entity/Input/Timer/Messager/UI/Http/Particle/Foot/Performance/...）
│   │   │   ├── Define.cs       # 全局常量定义
│   │   │   └── Init.cs         # MonoBehaviour 入口
│   │   └── ThirdParty/         # 第三方库
│   │       ├── DotRecast/      # Recast 寻路库
│   │       ├── DynamicBone/    # 动态骨骼
│   │       ├── ETTask/         # 单线程异步任务
│   │       ├── LitJson/        # JSON 序列化
│   │       ├── Nino/           # 高性能二进制序列化
│   │       └── SuperScrollView/# UI 列表扩展
│   ├── Plugins/
│   ├── Resources/
│   └── StreamingAssets/
├── Book/                       # 使用文档
├── Excel/                      # 配置表（xlsx）
├── Tools/                      # 工具集
│   ├── ExcelExport/            # 导表工具
│   ├── FileServer/             # 文件服务器（CDN）
│   ├── fontsubset/             # 字体子集化工具
│   └── RecastNavMesh/          # Recast 寻路演示工具
├── Bin/                        # 编译输出
├── Bundles/                    # AssetBundle 输出
├── BuildinFiles/               # 内置文件
├── HybridCLRData/              # HybridCLR 数据
└── FileServer/                 # 文件服务
```

## 快速开始

### 环境要求

- Unity 2021.3+ (URP)
- Odin Inspector（付费插件）

### 运行项目

1. 克隆仓库后用 Unity 打开
2. 打开 `Init` 场景或 `EditScene`
3. 在 `Init` GameObject 上设置 `PlayMode`：
   - `EditorSimulateMode`：编辑器模拟模式（无需下载）
   - `HostPlayMode`：联机模式（从 CDN 下载资源）
   - `WebPlayMode`：WebGL 模式
4. 运行场景

### 导出配置

1. 打开 `/Tools/ExcelExport/ExcelExport.sln` 编译导表工具
2. 运行 `Excel/win_startExcelExport.bat` 导出 Excel 配置
3. 或在 Excel 文件上右键一键导出（需先运行 `注册右键一键导出.bat`）

## 配置工具链

### Excel 配置表

| 文件 | 说明 |
|------|------|
| `UnitConfig@cs.xlsx` | 模型配置 |
| `AvatarConfig@cs.xlsx` | 玩家角色配置 |
| `CharacterConfig@c.xlsx` | 角色配置 |
| `MonsterConfig@cs.xlsx` | 怪物配置 |
| `EquipConfig@cs.xlsx` | 装备配置（武器、翅膀等挂点） |
| `GadgetConfig@cs.xlsx` | 技能生成物、场景交互物 |
| `SkillConfig@cs.xlsx` | 技能配置 |
| `AttributeConfig@cs.xlsx` | 属性索引 |
| `SceneConfig@cs.xlsx` | 场景配置 |
| `I18NConfig@i.xlsx` | 多语言配置 |
| `ServerConfig@c.xlsx` | 服务器/CDN配置 |

### Graph 编辑器

通过 Unity 菜单 `Tools/Graph编辑器` 打开：

- **AI 编辑器**：可视化编辑 AI 决策树（Action/Condition/Root 节点）
- **关卡编辑器**：可视化编辑 SceneGroup（Actors/Zones/Suites/Triggers/Route）
- **剧情编辑器**：可视化编辑 Story（Clip/Branch/Parallel 节点）

### FSM 状态机编辑

在 `Assets/AssetsPackage/Unit/XXX/Edit/FsmConfig.controller` 编辑动画状态机，右键 `工具/状态机导出` 导出 JSON。

## 目前完成效果

动画文件有点问题，但是不影响测试效果。模型来源 [模之屋](https://www.aplaybox.com/details/model/MmroYfxfeCtc)

![战斗技能.gif](ReadMeRes%2FPreview.gif)

![寻宝解谜.gif](ReadMeRes%2FPreview2.gif)

![怪物AI.gif](ReadMeRes%2FPreview3.gif)

![导表工具](ReadMeRes%2FExcelExport.png)

## 技术栈

| 技术 | 用途 |
|------|------|
| **TaoTie** | 轻量级 Unity 框架（Entity/Component 生命周期、Manager 系统） |
| **YooAsset** | 资源管理系统 |
| **Nino** | 高性能二进制序列化（配置加载） |
| **ETTask** | 单线程异步任务系统 |
| **DotRecast** | Recast 导航网格寻路 |
| **ORCA** | 动态避障寻路 |
| **DaGenGraph** | 节点编辑器框架 |
| **LitJson** | JSON 序列化 |
| **URP** | 通用渲染管线 |

## 引用或参考

1. [WorldReverse](https://github.com/fengjixuchui/WorldReverse)
2. [TaoTie](https://github.com/526077247/TaoTie) — 轻量级 Unity 框架
3. [YooAsset](https://github.com/tuyoogame/YooAsset) — Unity3D 资源管理系统
4. [Nino](https://github.com/JasonXuDeveloper/Nino) — 高性能 C# 序列化模块
5. [obfuz](https://github.com/focus-creative-games/obfuz.git) — 代码混淆、内存加密
6. [DaGenGraph](https://github.com/LiFang7/DaGenGraph) — 节点编辑器
7. [ET](https://github.com/egametang/ET) — 单线程异步、协程锁、计时器、数值组件、导表工具、打包工具
8. [UnityScriptHotReload](https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload) — 运行中无感重载 C# 代码
9. [N:ORCA](https://github.com/Nebukam/com.nebukam.orca) — 动态避障寻路

## 付费插件

1. [Odin Inspector](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041) — 编辑器扩展
2. [SuperScrollView](https://assetstore.unity.com/packages/tools/gui/ugui-super-scrollview-86572) — UI 滑动列表扩展
3. [Dynamic Bone](https://assetstore.unity.com/packages/tools/animation/dynamic-bone-16743) — 动态骨骼

## 使用文档

0. [前置知识点](./Book/0.前置知识点.md)
1. [AbilitySystem 的战斗框架](./Book/1.AbilitySystem的战斗框架.md)
2. [Camera 控制](./Book/2.Camera控制.md)
3. [SceneGroup 机关、关卡](./Book/3.SceneGroup机关、关卡.md)
4. [剧情](./Book/4.剧情.md)
5. [AI 框架](./Book/5.AI框架.md)
