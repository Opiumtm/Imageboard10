using System;
using Windows.Graphics;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.ModelInterface.Links;
using Imageboard10.Core.ModelInterface.Posts;
using Imageboard10.Core.Models.Links;
using Imageboard10.Core.Modules;
using Newtonsoft.Json;

namespace Imageboard10UnitTests
{
    public class FakeExternalPostMedia : IPostMediaWithSize
    {
        [JsonIgnore]
        public ILink MediaLink { get; set; }

        [JsonProperty("MediaLink")]
        public string MediaLinkJson { get; set; }

        [JsonProperty("MediaType")]
        public Guid MediaType { get; set; }

        [JsonProperty("FileSize")]
        public ulong? FileSize { get; set; }

        public Type GetTypeForSerializer() => typeof(FakeExternalPostMedia);

        [JsonIgnore]
        public SizeOfInt32 Size { get; set; }

        [JsonProperty("Width")]
        public int Width { get; set; }

        [JsonProperty("Height")]
        public int Height { get; set; }

        public void FillValuesBeforeSerialize(IModuleProvider modules)
        {
            Height = Size.Height;
            Width = Size.Width;
            MediaLinkJson = MediaLink.Serialize(modules);
        }

        public FakeExternalPostMedia FillValuesAfterDeserialize(IModuleProvider modules)
        {
            Size = new SizeOfInt32()
            {
                Height = Height,
                Width = Width
            };
            MediaLink = modules.DeserializeLink(MediaLinkJson);
            MediaLinkJson = null;
            return this;
        }
    }
}