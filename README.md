# Umbraco Invisible Nodes

[![status](https://img.shields.io/github/actions/workflow/status/ljfio/Umbraco-InvisibleNodes/ci.yaml)][build]
[![license](https://img.shields.io/github/license/ljfio/Umbraco-VirtualNodes)][license]
[![nuget](https://img.shields.io/nuget/v/Our.Umbraco.InvisibleNodes)][nuget]

The package for Umbraco 8 and 10+ that hides nodes in the content tree making them 'invisible' to front end users.

Inspired by Sotiris Filippidis (DotSee)'s [VirtualNodes][virtualnodes] and [OmitSegmentsUrlProvider][omitsegments] packages.

## Introduction

Consider the following content tree, you want to hide the categories from the generated URL. You know the Products have a unique name. Using the InvisibleNodes package you can configure it to hide the Product Category document type.

```mermaid
flowchart
    home(Home)---products
    products(Products)---productCategory1(Product Category)
    products(Products)---productCategory2
    productCategory1(Product Category 1)---product1(Product 1)
    productCategory1---product2(Product 2)
    productCategory2(Product Category 2)---product3(Product 3)
    productCategory1---product4(Product 4)
```

Before the URL for Product 1 would be: `/products/product-category-1/product-1`.
After using and configuring Invisible Nodes package the URL would be: `/products/product-1`.

## Installation

Currently both Umbraco 8 and 10 are supported.

### Umbraco 10+

```pwsh
dotnet add package Our.Umbraco.InvisibleNodes
```

### Umbraco 8

```pwsh
Install-Package Our.UmbracoCms.InvisibleNodes
```

## Usage

To make a node invisible, you can configure the app settings to make a content type an invisible node content type.

### Umbraco 10+

```json
{
  "InvisibleNodes": {
    "ContentTypes": [
      "hiddenNode"
    ]
  }
}
```

### Umbraco 8

```xml
<add key="InvisibleNodes" value="hiddenNode" />
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

[github]: https://github.com/ljfio/Umbraco-InvisibleNodes
[virtualnodes]: https://github.com/sotirisf/Umbraco-VirtualNodes
[omitsegments]: https://github.com/sotirisf/Umbraco-OmitSegmentsUrlProvider
[build]: https://github.com/ljfio/Umbraco-InvisibleNodes/actions/workflows/ci.yaml
[license]: https://github.com/ljfio/Umbraco-InvisibleNodes/blob/main/LICENSE
[nuget]: https://www.nuget.org/packages/Our.Umbraco.InvisibleNodes/