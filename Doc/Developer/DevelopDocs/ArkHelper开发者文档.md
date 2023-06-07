# 项目说明
## 开发使用
 - WPF
 - c#
 - .NET

## 项目结构
 - **ArkHelper.sln所在目录**即为根目录
    - **External**文件夹存放着应用的资源文件。该文件夹在生成时会自动复制到生成目录，当应用运行时会使用它。
    - **Asset**文件夹存放着应用的资源文件。该文件夹在生成时会被打包进二进制文件内部。
    - **Modules**文件夹是提取出的一些相近功能整合出的模块。
    - **Style**存储着一些样式文件。
    - **Xaml**是程序大部分UI和逻辑所在的文件夹。
        - **Control**文件夹下存储控件。
        - **MainWindow.xaml**是主窗口UI文件，**MainWindow.xaml.cs**是主窗口逻辑代码。
    - 程序入口点在**App.xaml.cs**中，在该文件中的Application_Startup方法中，储存着程序启动后加载界面前的动作，如读取配置、启动监听等。
    - **ArkHelper.cs**中存在着ArkHelper的绝大部分供调用的方法。ADB操作方法、标准名称获取方法、地址存储等代码都写在这里。命名空间是ArkHelper。
        

# 源码编译
## 流程
1. 使用git将本仓库克隆到本地
1. 使用Visual Studio打开其中的ArkHelper.sln文件
1. 点击工具栏中“生成”-“生成ArkHelper”，即完成编译。
## 注
1. 项目目录中的external文件夹在生成时会自动拷贝到生成文件夹，作为软件运行的必要文件。
1. 请**不要**使用Github自带的“Download Zip”功能直接下载代码，**而是**使用```git clone```。这样可以避免丢失仓库信息。

