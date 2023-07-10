# 狼人杀

抖音小游戏狼人杀是火山引擎实时音视频提供的一个开源示例项目。本文介绍如何快速跑通该示例项目，体验 RTC 抖音小游戏狼人杀效果。

## 应用使用说明

使用该工程文件构建应用后，即可使用构建的应用体验抖音即玩小游戏狼人杀demo。
你和你的同事必须加入同一个房间，才能一同进行狼人杀demo体验。

## 前置条件

- Unity 2021.2.19
	
- iOS抖音版本号需要>=17.7.0，安卓抖音版本号需要>=19.0.0
	
- iOS版本>=13.4（且不能是15.4.0、15.4.1），安卓版本>=6.0
	
- 有效的[Unity开发者账号](https://id.unity.com/en/account/edit) 
	
- 有效的[字节小程序开发者平台账号](https://microapp.bytedance.com/)
	
- 有效的[火山引擎开发者账号](https://console.volcengine.com/auth/login)
	

## 操作步骤

### 步骤 1：获取火山引擎 AppID 和 AppKey

在火山引擎控制台->[应用管理](https://console.volcengine.com/rtc/listRTC)页面创建应用或使用已创建应用获取 AppID 和 AppAppKey

### 步骤 2：获取 游戏 AppID

在[小程序开发者平台](https://microapp.bytedance.com/)创建游戏。选择即玩 Unity&UE 小游戏，并获取 AppID。或使用已创建即玩小游戏获取AppID

> 首次创建用户请联系火山引擎技术支持为 AppID 开通白名单

<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_a413a6efb34965e5cd8f3ec95a2ffe8f.png" width="700px" >


### 步骤 3：接入Stark SDK Unity Tools

1. 在Unity Hub中点击Open，打开`RTC_WolfGame_Demo`目录
	
2. 下载并导入 bgdt package 并重启 Unity
 [ com.bytedance.bgdt-cp-3.0.250.unitypackage · 279.29 KB](https://lf3-stark-cdn.bdgp.cc/obj/ide-updateserver-bytegame/bgdt/com.bytedance.bgdt/3.0.250/com.bytedance.bgdt.unitypackage)
	
3. 安装发布工具并重启 Unity

	点击 Bytegame -> Bytegame Developer Tools，安装 Bytegame Developer Tools、StarkSDK 和 StarkSDKUnityTools。
	<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_fb994a675e827fc74c03aa9e5668c129.png" width="700px" >

### 步骤4: 安装 WebGL 和 Android 构建平台

点击 File -> Build Settings，安装WebGL 和Android。

<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_8e8325490ec76eb533b793d79fd78f9b.jpg" width="700px" >

### 步骤5：构建工程

1. 打开 `Assets/Scripts/Constants.cs`文件
	
2. 填写 LoginUrl、火山引擎 AppID 和 AppKey
> 当前你可以使用 `wss://rtcio.bytedance.com` 作为测试服务器域名，仅提供跑通测试服务，无法保障正式需求。

<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_ffa9c254c7f8b87d6c5909a95e5484c6.png" width="700px" >

### 步骤 6：编译运行

1. 从菜单栏选择 ByteGame -> StarkSDKTools -> Build Tool。

	<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_239134827a2c738b604e44222c77bd2d.png" width="700px" >
	
2. 运行框架选择：WebGL，点击“构建WebGL”->Build。

	<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_28fdf018804e787d5e8582b1681db9d2.png" width="700px" >
	
3. 编译成功之后点击“发布WebGL”，
	
4. 填入 Uid、游戏AppId，屏幕方向选择“竖屏”，点击“自动版本号”，Android 与 iOS 端根据需要选择 WebGL 或不发布，点击发布。发布成功之后，使用抖音App扫描生成的二维码即可运行demo。

	<img src="https://portal.volccdn.com/obj/volcfe/cloud-universal-doc/upload_8489534805ba108e9773e9e0194e1133.jpg" width="700px" >
