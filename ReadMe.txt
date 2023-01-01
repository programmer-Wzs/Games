
Box项目包含：
	ABLoder.cs AB包加载器
	背包系统（还未完成）：显示正常，部分逻辑还未完成


HotUpdate项目包含：
	a. 生成AB包(Assets/Editor)
	b. 热更新(Assets/Script)
		Tools工具类
		HotUpdate主要热更新逻辑类
		Data 通用数据类
		Test测试类，挂载到MainCamera，游戏运行主动调用
	c. 使用NetBox搭建简单的本地资源服务器，安装NetBox后，运行main.box即可启动服务器（文件在ServerByNetBox中）
	