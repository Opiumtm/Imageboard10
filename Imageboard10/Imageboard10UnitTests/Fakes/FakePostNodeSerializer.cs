using System;
using System.IO;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Imageboard10UnitTests
{
    public class FakePostNodeSerializer : ModuleBase<IObjectSerializer>, IObjectSerializer, IStaticModuleQueryFilter
    {
        public string TypeId => "tests.fakenode";

        public Type Type => typeof(FakePostNode);

        public string SerializeToString(ISerializableObject obj)
        {
            return JsonConvert.SerializeObject((FakePostNode)obj);
        }

        public byte[] SerializeToBytes(ISerializableObject obj)
        {
            using (var str = new MemoryStream())
            {
                using (var wr = new BsonDataWriter(str))
                {
                    var s = new JsonSerializer();
                    s.Serialize(wr, (FakePostNode)obj);
                    wr.Flush();
                }
                return str.ToArray();
            }
        }

        public ISerializableObject Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<FakePostNode>(data);
        }

        public ISerializableObject Deserialize(byte[] data)
        {
            using (var str = new MemoryStream(data))
            {
                using (var rd = new BsonDataReader(str))
                {
                    var s = new JsonSerializer();
                    return s.Deserialize<FakePostNode>(rd);
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