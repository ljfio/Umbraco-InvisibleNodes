# Umbraco VirtualNodes

The Virtual Nodes package makes nodes in Umbraco invisible.
They can be seen from the backoffice but are excluded from the generated URLs. 

## Installation

Currently both Umbraco 8 and 10 are supported.

For Umbraco 10:

```pwsh
dotnet add package Our.Umbraco.VirtualNodes
```

For Umbraco 8:

```pwsh
Install-Package Our.UmbracoCms.VirtualNodes
```

## Usage

To make a node virtual, you can configure the app settings to make a content type a virtual node content type.

For Umbraco 10:

```json
{
  "VirtualNode": {
    "ContentTypes": [
      "hiddenNode"
    ]
  }
}
```

For Umbraco 8:

```xml
<add key="VirtualNode" value="hiddenNode" />
```

## Contributing

Please raise any issues with the package on [GitHub][github].
Pull requests are also welcome.

## License

Copyright 2023 Luke Fisher

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

[github]: https://github.com/ljfio/Umbraco-VirtualNodes
