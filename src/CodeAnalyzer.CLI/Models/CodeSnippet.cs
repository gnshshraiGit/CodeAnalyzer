using System;
using Microsoft.Extensions.VectorData;

namespace CodeAnalyzer.Models
{
public class CodeSnippet
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [VectorStoreData(IsIndexed = true)]
        public string FileName { get; set; }
        [VectorStoreData(IsFullTextIndexed = true)]
        public string Content { get; set; }
        [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance, IndexKind = IndexKind.Hnsw)] // Dimensions for text-embedding-3-small
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}