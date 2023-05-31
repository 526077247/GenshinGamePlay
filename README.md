# GenshinGamePlay

通过研究原神逆向工程 https://github.com/fengjixuchui/WorldReverse ，推导出的一套GamePlay框架，包括战斗、解密，后续将加入AI、剧情

## 目前完成效果
动画文件有点问题，但是不影响测试效果。模型来源 [模之屋](https://www.aplaybox.com/details/model/MmroYfxfeCtc)

![战斗技能.gif](ReadMeRes%2FPreview.gif)

![寻宝解谜.gif](ReadMeRes%2FPreview2.gif)

## todo：
1. 怪物AI
2. 剧情演出
3. 二进制序列化多态支持

## 导表工具
打开 /Tools/ExcelExport/ExcelExport.sln 编译后可用

![ExcelExport.png](ReadMeRes%2FExcelExport.png)


## 引用或参考
1. [WorldReverse](https://github.com/fengjixuchui/WorldReverse) 原神逆向工程
2. [TaoTie](https://github.com/526077247/TaoTie) 轻量级Unity框架
3. [YooAsset](https://github.com/tuyoogame/YooAsset) Unity3D的资源管理系统
4. [Nino](https://github.com/JasonXuDeveloper/Nino) 实用的高性能C#序列化模块
5. [FernNPR](https://github.com/FernRender/FernNPR) NPR渲染库
6. [DaGenGraph](https://github.com/LiFang7/DaGenGraph) 节点编辑器
7. [ETTask](https://github.com/egametang/ET) 单线程异步、协程锁
8. [UnityScriptHotReload](https://github.com/Misaka-Mikoto-Tech/UnityScriptHotReload) 运行中无感重载C#代码