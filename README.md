# rh-basemods

## Demo Structure
- BaseMod - 提供了基础的mod功能，如注册自定义触发器，动作，属性等
- UnlockMod - 解锁内置的风灵月影
- CustomTriggerMod - 定义了一个战吼触发器，即首次置入轮盘时触发其他动作
- CustomActionMod - 依赖CustomTriggerMod，创建了一个小机器人单位，能够在战吼时执行自定义动作

## Demo Quickstart

1. 获取完整版未pruned游戏
2. 安装[bepinex](https://docs.bepinex.dev/articles/user_guide/installation/index.html)
3. 将`artifacts/ModDLLPreloader.dll`复制到游戏目录下的`BepInEx\patchers`文件夹
4. 将所有`artifacts/xxxMod`复制到游戏目录下的`Mod\ModDebug`文件夹