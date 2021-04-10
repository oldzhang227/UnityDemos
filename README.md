# UnityDemos

1：DownloadDemo：多线程下载示例

使用C#的HttpWebRequset来下载文件时，同时只执行一个下载任务，下载速度达不到整个带宽的最大速度，因此采用多线程执行，同时下载多个文件，充分利用带宽，达到最高下载速度。

2：ResourcesDemo：资源管理示例

基于AssetBundle的加载，卸载，引用计数，缓存池模式，采用弱引用来实现卸载未使用的AssetBundle

3：FullScreenDemo：全面屏适配示例

Android下系统版本在AndoridP以下的分不同的手机厂商获取刘海的尺寸，AndroidP以上采用自带方法获取，iOS下采用Unity的方法Screen.safeArea来获取

4：GifDecodeDemo：Gif格式的图片转为Texture2D

接入腾讯相关组件时，会有显示QQ头像的需求，有部分QQ头像为Gif格式的，Texture2D.LoadImage无法解析，只能手动解析完像素数据再创建Texture2D

5：BuildLibraryDemo：C++多平台编译库工程

将C++代码编译为各个平台的库文件，在Unity中调用

6：SkinAnimDemo：蒙皮动画的演示demo
尝试用cpu来实现蒙皮动画

7：InjectFixDemo：Injectfix热修复C#的演示demo
定义了良好的热修复工作流