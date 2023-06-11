# SemanticKernel Extension

[![NuGet Version](https://img.shields.io/nuget/v/SS.SemanticKernel.Extensions.svg?style=flat)](https://www.nuget.org/packages?q=SS.SemanticKernel.Extensions) 

This is a SemanticKernel extension built on the [Embedding](https://github.com/chenrensong/Embedding) codebase. This extension enables you to generate text embeddings with a semantic-based kernel function.

## Installation

Ensure you have the Embedding codebase installed. You can then install this extension by cloning this repository:

```
Install-Package SS.SemanticKernel.Extensions
```

## Configuration

To configure this extension, use the following code:

```csharp
kernelBuilder = kernelBuilder
.WithCoreTextEmbeddingGenerationService("text2vec-base-chinese", "http://127.0.0.1:8000/embeddings");
```

This configuration allows the SemanticKernel extension to utilize the `text2vec-base-chinese` model and the embedding service located at `http://127.0.0.1:8000/embeddings`.

## Contributing

Contributions of any kind are welcome. If you have any suggestions or issues with this project, feel free to open an issue or pull request.

## License

This project is licensed under the MIT License.

## Contact

If you have any questions, feel free to contact us at: chenrensong@outlook.com

