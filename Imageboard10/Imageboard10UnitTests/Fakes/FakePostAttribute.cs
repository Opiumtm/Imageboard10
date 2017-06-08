using System;
using Imageboard10.Core.ModelInterface.Posts;
using Newtonsoft.Json;

namespace Imageboard10UnitTests
{
    public class FakePostAttribute : IPostBasicAttribute
    {
        public Type GetTypeForSerializer() => GetType();

        [JsonProperty("a")]
        public string Attribute { get; set; }
    }
}