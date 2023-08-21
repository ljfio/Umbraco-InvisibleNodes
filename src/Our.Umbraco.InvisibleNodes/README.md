# Our.Umbraco.VirtualNodes

## Installation

```pwsh
dotnet add package Our.Umbraco.VirtualNodes
```

## Usage

To make a node virtual, you can configure the app settings to make a content type a virtual node content type.

```json
{
  "VirtualNode": {
    "ContentTypes": [
      "hiddenNode"
    ]
  }
}
```

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
