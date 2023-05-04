# TableCraft

[![Commitizen friendly](https://img.shields.io/badge/commitizen-friendly-brightgreen.svg)](http://commitizen.github.io/cz-cli/)

## 简介

TableCraft是一个通用、可拓展的解析配置源文件，生成配置描述文件与客制化配置读取代码解决方案。

项目提供了一个.NET6运行时库，同时也提供了一个基于此库的命令行工具，与一个基于此库的[Avalonia](https://avaloniaui.net/)可视化编辑器作为使用范例。

## 特性

### TableCraft.Core

* 规定字段的合法数值类型，集合类型
* 为字段添加标签进行特殊处理（例如表格的主键）
* 支持不同文件类型的数据源（目前支持：csv）
* 支持不同文件类型的数据描述（目前支持：json）
* 支持使用[T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022)与TableCraft提供的API生成任意语言的业务代码
* 支持版本控制（目前支持：perforce）

### TableCraft.Editor

*运行设备需要安装.NET 6.0

* 基于Avalonia的可视化编辑器
* 支持配置数据源文件目录与数据描述文件目录
* 支持配置不同使用途径下的代码导出目录
* 自动化版本控制（perforce）

## 配置方式

### TableCraft.Core 配置

#### libenv.json

使用运行时库TableCraft.Core需要配置`libenv.json`文件，并在使用前通过接口`TableCraft.Core.Configuration.ReadConfigurationFromJson`进行初始化

```json
{
    // 规定数值类型
    "DataValueType": ["int", "uint", "float", "boolean", "string"],
    // 规定集合类型
    "DataCollectionType": ["none", "array"],
    // 规定可用的字段标签
    "AttributeTag": ["primary", "label1", "label2"],
    // TableCraft默认使用UTF8编码，在此指定是否需要BOM头
    "UTF8BOM": false,
    // 对csv类型数据源文件，规定字段名所在的行数，与注释所在的行数（若不存在则填-1）
    "CsvSource":{
        "HeaderLineIndex": 0,
        "CommentLineIndex": 1
    },
    // 规定各种导出的代码途径
    "ConfigUsageType":{
        "usage0":{
            // 用于生成代码的T4模板文件，此文件需要放在可执行文件同级Templates目录下
            "CodeTemplateName": "usage0-template.tt",
            // 生成文件的类型，此例生成的文件为c#代码
            "TargetFileType": ".cs"
        }
    }
}
```

### TableCraft.Editor 配置

#### appsettings.json

`appsettings.json`中配置了一些重要的文件目录，也用于保存版本控制相关的用户信息

```json
{
    // 配置文件所在的共同根目录，用于读取配置文件
    "ConfigHomePath": "",
    // 配置描述文件的共同根目录，用于保存描述文件
    "JsonHomePath": "",
    // 生成目录
    "ExportPath": {
        "usage0": ""
    },
    // Perforce版本控制相关信息，不需要在此处手动配置，在应用内编辑后进行保存即可
    "P4Config": {
        "P4PORT": "",
        "P4USER": "",
        "P4CLIENT": "",
        "P4PASSWDBASE64": ""
    }
}
```

## TableCraft.Editor使用方式

按照下述步骤操作前，请先保证上述的两个json文件已配置正确；若存在错误配置，可在Editor可执行文件同级Logs目录下查看具体问题。

![image-20230504222628230](https://s2.loli.net/2023/05/04/oFwejhrCAliOXpc.png)

1. 查看工具配置情况
2. 可点击此按钮添加新的配置项

![image-20230504223100478](https://s2.loli.net/2023/05/04/cuHms6nqNBZSr7X.png)

3. 选择已加入到工具中的配置表
4. 选中配置表后，此处会显示配置表的字段信息
5. 展示当前配置表的原文件名，在“Usage”选中使用途径后，可在“ExportName”指定该途径下的客制化名称

![image-20230504223332644](https://s2.loli.net/2023/05/04/J8R2q1uhjpsDGoz.png)

6. 选中特定字段后，在此处可以编辑字段的描述（注释），数值类型，集合类型，默认值，特定使用途径下的导出名称（常用于不同命名风格的变量名），添加标签

![image-20230504223527993](https://s2.loli.net/2023/05/04/dB7HRi2McZmFx39.png)

7. 指定生成文件的目标使用途径
8. 点击此按钮导出文件（导出路径显示按钮下方）
9. 点击此按钮将数据描述文件保存至“JsonHome”

## 第三方依赖

### TableCraft.Core 依赖

* [LitJson](https://github.com/LitJSON/litjson)：json序列化与反序列化
* [Mono.TextTemplating](https://github.com/mono/t4)：.NET 6.0 可用的T4 Templating 语言引擎
* [p4api.net](https://www.nuget.org/packages/p4api.net)：支持Perforce版本控制

## License

[MIT](http://opensource.org/licenses/MIT)

Copyright (c) 2023 - Boming Chen