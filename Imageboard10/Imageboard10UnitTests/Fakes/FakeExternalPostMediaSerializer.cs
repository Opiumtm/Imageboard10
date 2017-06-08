using System;
using System.IO;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Imageboard10UnitTests
{
    public class FakeExternalPostMediaSerializer : ModuleBase<IObjectSerializer>, IObjectSerializer, IStaticModuleQueryFilter
    {
        public string TypeId => "tests.fakemedia";

        public Type Type => typeof(FakeExternalPostMedia);

        public string SerializeToString(ISerializableObject media)
        {
            ((FakeExternalPostMedia)media).FillValuesBeforeSerialize(ModuleProvider);
            return JsonConvert.SerializeObject((FakeExternalPostMedia) media);
        }

        public byte[] SerializeToBytes(ISerializableObject media)
        {
            ((FakeExternalPostMedia)media).FillValuesBeforeSerialize(ModuleProvider);
            using (var str = new MemoryStream())
            {
                using (var wr = new BsonDataWriter(str))
                {
                    var s = new JsonSerializer();
                    s.Serialize(wr, (FakeExternalPostMedia)media);
                    wr.Flush();
                }
                return str.ToArray();
            }
        }

        public ISerializableObject Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<FakeExternalPostMedia>(data).FillValuesAfterDeserialize(ModuleProvider);
        }

        public ISerializableObject Deserialize(byte[] data)
        {
            using (var str = new MemoryStream(data))
            {
                using (var rd = new BsonDataReader(str))
                {
                    var s = new JsonSerializer();
                    return s.Deserialize<FakeExternalPostMedia>(rd).FillValuesAfterDeserialize(ModuleProvider);
                }
            }
        }

        public ISerializableObject BeforeSerialize(ISerializableObject obj)
        {
            return obj;
        }

        public ISerializableObject AfterDeserialize(ISerializableObject obj)
        {
            return obj;
        }

        public bool CheckQuery<T>(T query)
        {
            if (query is Type)
            {
                return (query as Type) == Type;
            }
            if (query is string)
            {
                return (query as string) == TypeId;
            }
            return false;
        }
    }
}