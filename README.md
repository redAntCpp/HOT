# HOT项目说明
WPF项目，使用C#实现的一个业务管理系统。（更新中...）
## 技术架构
|  层级   | 相关技术以及配置 |
|  ----  | ----  |
|界面|xaml（仿qq音乐）|
|中间层技术|WebApi（token技术、接口控流、业务逻辑）|
|数据交互|Json|
|数据库|SqlServer 2014，使用ado.net|
|服务器|windows2016. IIS web服务器|
## 项目架构
见下图

![image](https://user-images.githubusercontent.com/45525918/142761693-0a8563d7-6a1a-4d65-96ae-55c64b09665d.png)

**说明**

Config文件夹：自己新建的文件夹，一般代码中需要用到的数据，而不能写死的，从这里配置。（如数据库连接串、日志等级等）

HOTControl文件夹：自己新建的文件夹，用来放可能用到的第三方dll。

lib文件夹：自己新建的文件夹，为自己整理的c#帮助库，详情见：https://github.com/redAntCpp/CSharpTools

view文件夹：自己新建的文件夹，用于主界面的显示，通常为用户控件。
