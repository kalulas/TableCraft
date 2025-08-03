# TableCraft

[![Commitizen friendly](https://img.shields.io/badge/commitizen-friendly-brightgreen.svg)](http://commitizen.github.io/cz-cli/)

## Overview

TableCraft is a general and extensible solution for parsing configuration source files, generating configuration description files and customizing configuration reading codes.

The project provides a .NET6 runtime library (TableCraft.Core), as well as a command-line tool (TableCraft.Console) based on this library, and an [Avalonia](https://avaloniaui.net/) visual editor (TableCraft.Editor) based on this library as a usage example.

TableCraft.Core is the core of the entire solution. It can extract key information (such as configuration file names and field names) from different types of configuration files based on rules. It also uses additional description files to supplement information about field types and labels, and outputs complete configuration file information in the specified JSON format. In addition, it can use this configuration file information to generate any text you want based on the template you provide (which is very useful for generating code files with repeated rules).

## Features

### TableCraft.Core

* Define valid value types and collection types for fields
* Add labels to fields for special handling (such as primary keys in tables)
* Support data sources of different file types (currently supports: csv)
* Support data descriptions of different file types (currently supports: json)
* Use [T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022) and TableCraft API to generate business code in any language
* Support version control (currently supports: perforce)

### TableCraft.Editor

*.NET 6.0 is required to run the editor.

* Visual editor based on Avalonia
* Configure directories for data source files and data description files
* Configure export directories for code under different usage scenarios
* Automated version control with perforce

## Example

Assuming we have a Students.csv file:

| ID   | ClassID | Name   | Age  | Courses                 |
| ---- | ------- | ------ | ---- | ----------------------- |
| 0    | 1       | Edward | 24   | CS61A;MIT18.01;MIT18.06 |
| 1    | 1       | Alex   | 24   | UCB CS61B;MIT 6.006     |

By supplementing field types, tags, etc. through an additional JSON file:

```jsonc
{
    "ConfigName" : "Students",
    "Usage"      : {
        "usage0" : {
            "ExportName" : "StudentsTable"
        }
    },
    "Attributes" : [
        {
            "AttributeName" : "ID",
            "Comment"       : "Identifier",
            "ValueType"     : "int",
            "DefaultValue"  : "",
            "CollectionType" : "none",
            "Usages"         : [
                {
                    "Usage" : "usage0",
                    "FieldName" : "ID"
                }
            ],
            "Tag"            : [
                "primary"
            ]
        },
        // ...
    ]
}
```

Completing such a JSON configuration file on your own may be frustrating. You can use the TableCraft.Editor visual tool to generate the JSON file.

Now by completing a text template (.tt) to simply describe your generation rules:

```text
<#@ template debug="true" hostspecific="true" language="C#" #>
<#@ parameter type="System.String" name="CurrentUsage"#>
<#
    var configInfoObject = Host.GetHostOption("CurrentConfigInfo");
    var configInfo = configInfoObject as ConfigInfo;
    if (configInfo == null) throw new Exception("null CurrentConfigInfo received, exit!");
#>
namespace Foo {
    public class <#= configInfo.GetExportName(CurrentUsage) #> {
<# 
    foreach( var info in configInfo.AttributeInfos ){ 
        if(info.IsValid() && info.HasUsage(CurrentUsage)){
#>
            public <#= info.ValueType #> <#= info.GetUsageFieldName(CurrentUsage) #> { get; private set; }
<#
        }
    }
#>
    }
}
```

Congratulations! TableCraft.Core will now output the following results for you:

```csharp
namespace Foo {
    public class StudentsTable {
            public int ID { get; private set; }
            public uint ClassID { get; private set; }
            public string Name { get; private set; }
            public uint Age { get; private set; }
            public string Courses { get; private set; }
    }
}
```

## Configuration

### TableCraft.Core Configuration

#### libenv.json

To use the runtime library TableCraft.Core, you need to configure the `libenv.json` file and initialize it before use through the interface `TableCraft.Core.Configuration.ReadConfigurationFromJson`.

```jsonc
{
    // Define data value types
    "DataValueType": ["int", "uint", "float", "boolean", "string"],
    // Define collection types
    "DataCollectionType": ["none", "array"],
    // Define available field tags
    "AttributeTag": ["primary", "label1", "label2"],
    // TableCraft uses UTF8 encoding by default. Specify whether a BOM header is needed here.
    "UTF8BOM": false,
     // For csv type data source files, specify the row number where field names are located and where comments are located (if they do not exist, fill in -1).
     "CsvSource":{
        "HeaderLineIndex": 0,
        "CommentLineIndex": 1
     },
     // Specify various export code methods.
    "ConfigUsageType":{
        "usage0":{
            // T4 template file used to generate code. This file needs to be placed in the Templates directory at the same level as the executable file.
            "CodeTemplateName":"usage0-template.tt",
            // The type of generated file. In this example, c# code is generated.
            "TargetFileType":".cs",
            // The format string of the generated file name, if this string contains a file type, it will be replaced by TargetFileType
            "OutputFormat": "{0}_base"
        }
     },
    // Specify group to support exporting multiple files for each usage
    "ConfigUsageGroup":{
        "group0":[
            "usage0",
            "usage1"
        ]
    }
}
```

### TableCraft.Editor Configuration

#### appsettings.json

`appsettings.json` configures some important directory paths and also saves user information related to version control.

```jsonc
{
    // Common root directory of configuration files for reading configuration files.
    "ConfigHomePath":"",
    // Common root directory of configuration description files for saving description files.
    "JsonHomePath":"",
    // Export directory
    "ExportPath":{
        "usage0":""
     },
     // Perforce version control related information. No need to manually configure here, just edit and save in the application.
    "P4Config":{
        "P4PORT":"",
        "P4USER":"",
        "P4CLIENT":"",
        "P4PASSWDBASE64":""
    }
}
```

## Usage of TableCraft.Editor

Before following the steps below, please ensure that the two JSON files mentioned above are configured correctly. If there is any misconfiguration, you can check the specific problem in the Logs directory at the same level as the Editor executable file.

![image-20230504222628230](https://s2.loli.net/2023/05/04/oFwejhrCAliOXpc.png)

1. Check tool configuration
2. Click this button to add a new configuration item

![image-20230504223100478](https://s2.loli.net/2023/05/04/cuHms6nqNBZSr7X.png)

3. Select a configuration table that has been added to the tool
4. After selecting a configuration table, field information will be displayed here
5. Displaying original filename of current configuration table; after selecting "Usage", specify customized name under this usage in "ExportName"

![image-20230504223332644](https://s2.loli.net/2023/05/04/J8R2q1uhjpsDGoz.png)

6. After selecting a specific field, you can edit its description (comment), data type, collection type, default value, export name under specific usage (often used for variable names with different naming styles), and add tags here.
7. Specify target usage, or a usage group for generated files.
8. Click this button to export files (if a target usage is selected, export path is shown below button).
9. Click this button to save data description file to "JsonHome".

## Third-party dependencies

### Dependencies of TableCraft.Core 

* [LitJson](https://github.com/LitJSON/litjson): JSON serialization and deserialization 
* [Mono.TextTemplating](https://github.com/Microsoft/t4): T4 Templating language engine available for .NET 6.0 
* [p4api.net](https://www.nuget.org/packages/p4api.net): Supports Perforce version control 

## License

[MIT](http://opensource.org/licenses/MIT)

Copyright (c) 2023 - Boming Chen