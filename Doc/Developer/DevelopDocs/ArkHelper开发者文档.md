# 项目说明
## 开发使用
 - WPF
 - c#
 - .NET

# 项目结构
 - **ArkHelper.sln所在目录**即为根目录
    - **Asset**文件夹存放着一些必要的资源文件。应用在编译时会读取这些文件。
    - **Modules**文件夹是提取出的一些相近功能整合出的模块。
    - **Style**存储着一些样式文件，如果你有网页开发的经历，你可以把它们理解为css文件。
    - **Xaml**是程序大部分UI和逻辑所在的文件夹。
        - **MainWindow.xaml**是主窗口。
        - **Control**文件夹下存储控件。
    - 程序入口点在**App.xaml.cs**中，在该文件中的Application_Startup方法中，储存着程序启动后加载界面前的动作，如读取配置、启动监听等。
    - **ArkHelper.cs**中存在着ArkHelper的绝大部分供调用的方法。命名空间是ArkHelper。
        

# 源码编译
## 流程
1. 使用git将本仓库克隆到本地
1. 使用Visual Studio打开其中的ArkHelper.sln文件
1. 点击工具栏中“生成”-“生成ArkHelper”，即完成编译。
## 注
1. 项目目录中的external文件夹在生成时会自动拷贝到生成文件夹，作为软件运行的必要文件。

