# TableCraft

## Overview

TableCraft is a general and extensible solution for parsing configuration source files, generating configuration description files and customizing configuration reading codes.

## Features

### TableCraft.Core

* Define valid value types and collection types for fields
* Add labels to fields for special handling (such as primary keys in tables)
* Support data sources of different file types (currently supports: csv)
* Support data descriptions of different file types (currently supports: json)
* Use [T4 Text Templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022) and TableCraft API to generate business code in any language
* Support version control (currently supports: perforce)

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

## License

[MIT](http://opensource.org/licenses/MIT)

Copyright (c) 2023 - Boming Chen