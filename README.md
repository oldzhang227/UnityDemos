# UnityDemos

1：DownloadDemo：多线程下载示例

使用C#的HttpWebRequset来下载文件时，同时只执行一个下载任务，下载速度达不到整个带宽的最大速度，因此采用多线程执行，同时下载多个文件，充分利用带宽，达到最高下载速度。

2：ResourcesDemo：资源管理示例

基于AssetBundle的加载，卸载，引用计数，缓存池模式，采用弱引用来实现卸载未使用的AssetBundle

2：FullScreenDemo：全面屏适配示例

Android下系统版本在AndoridP以下的分不同的手机厂商获取刘海的尺寸，AndroidP以上采用自带方法获取，iOS下采用Unity的方法Screen.safeArea来获取
