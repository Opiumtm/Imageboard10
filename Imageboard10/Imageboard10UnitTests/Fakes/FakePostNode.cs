using System;
using Imageboard10.Core.ModelInterface.Posts;
using Newtonsoft.Json;

namespace Imageboard10UnitTests
{
    public class FakePostNode : ITextPostNode
    {
        public Type GetTypeForSerializer() => GetType();

        [JsonProperty("t")]
        public string Text { get; set; }
    }
}