using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Imageboard10.Core.ModelInterface;
using Imageboard10.Core.Models.Serialization;
using Imageboard10.Core.Modules;

namespace Imageboard10.Core.Models
{
    /// <summary>
    /// Класс-помощник для реализаций сериализации.
    /// </summary>
    public static class SerializationImplHelper
    {
        /// <summary>
        /// Добавить информацию о типе.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <param name="typeId">Тип.</param>
        /// <returns>Результат с информацией о типе.</returns>
        public static byte[] WithTypeId(byte[] data, string typeId)
        {
            using (var str = new MemoryStream())
            {
                using (var wr = new BinaryWriter(str, Encoding.UTF8))
                {
                    if (typeId == null)
                    {
                        wr.Write((byte)0);
                    }
                    else
                    {
                        wr.Write((byte)1);
                        wr.Write(typeId);
                    }
                    var sz = data?.Length ?? -1;
                    wr.Write(sz);
                    if (data != null && data.Length > 0)
                    {
                        wr.Write(data);
                    }
                    wr.Flush();
                }
                return str.ToArray();
            }
        }

        /// <summary>
        /// Получить информацию о типе.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Данные и информация о типе.</returns>
        public static (byte[] data, string typeId) ExtractTypeId(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using (var str = new MemoryStream(data))
            {
                using (var rd = new BinaryReader(str, Encoding.UTF8))
                {
                    string typeId;
                    int sz;
                    byte[] rdata;
                    var b = rd.ReadByte();
                    switch (b)
                    {
                        case 0:
                            typeId = null;
                            break;
                        case 1:
                            typeId = rd.ReadString();
                            break;
                        default:
                            throw new SerializationException("Неверный формат входных данных");
                    }
                    sz = rd.ReadInt32();
                    if (sz < 0)
                    {
                        rdata = null;
                    } else if (sz == 0)
                    {
                        rdata = new byte[0];
                    }
                    else
                    {
                        rdata = rd.ReadBytes(sz);
                    }
                    return (rdata, typeId);
                }
            }
        }

        /// <summary>
        /// Добавить информацию о типе.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <param name="typeId">Тип.</param>
        /// <returns>Результат с информацией о типе.</returns>
        public static string WithTypeId(string data, string typeId)
        {
            using (var wr = new StringWriter())
            {
                if (typeId == null)
                {
                    wr.Write("t");
                }
                if (data == null)
                {
                    wr.Write("d");
                }
                wr.Write(":");
                wr.WriteLine(typeId ?? "");
                if (data != null)
                {
                    wr.Write(data);
                }
                wr.Flush();
                return wr.ToString();
            }
        }

        /// <summary>
        /// Получить информацию о типе.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <returns>Данные и информация о типе.</returns>
        public static (string data, string typeId) ExtractTypeId(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using (var str = new StringReader(data))
            {
                string typeId;
                string rdata;
                var fl = str.ReadLine();
                var idx = fl.IndexOf(':');
                if (idx < 0)
                {
                    throw new SerializationException("Неверный формат входных данных");
                }
                var flags = new HashSet<char>(fl.Remove(idx));
                if (flags.Contains('t'))
                {
                    typeId = null;
                }
                else
                {
                    typeId = fl.Substring(idx+1);
                }
                if (flags.Contains('d'))
                {
                    rdata = null;
                }
                else
                {
                    rdata = str.ReadToEnd();
                }
                return (rdata, typeId);
            }
        }

        /// <summary>
        /// Сериализовать во внешний контракт.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <param name="obj">Объект.</param>
        /// <returns>Внешний контракт.</returns>
        public static TExtern SerializeToExternalContract<TExtern>(this IModuleProvider modules, ISerializableObject obj)
            where TExtern : class, IExternalContractHost, new()
        {
            if (modules == null) throw new ArgumentNullException(nameof(modules));
            if (obj == null)
            {
                return null;
            }
            var t = obj.GetTypeForSerializer();
            var serializer = modules.QueryModule<IObjectSerializer, Type>(t)
                             ?? throw new ModuleNotFoundException($"Неизвестный тип медиа-объекта поста для сериализации {t.FullName}");
            var bytes = serializer.SerializeToBytes(obj);
            return new TExtern()
            {
                Contract = new ExternalContractData()
                {
                    BinaryData = bytes != null ? Convert.ToBase64String(bytes) : null,
                    TypeId = serializer.TypeId,
                }
            };
        }

        /// <summary>
        /// Десериализовать из внешнего контракта.
        /// </summary>
        /// <param name="modules">Модули.</param>
        /// <param name="contract">Контракт.</param>
        /// <returns>Объект.</returns>
        public static ISerializableObject DeserializeExternalContract<TExtern>(this IModuleProvider modules, TExtern contract)
            where TExtern: IExternalContractHost
        {
            if (contract?.Contract?.BinaryData == null)
            {
                return null;
            }
            var serializer = modules.QueryModule<IObjectSerializer, string>(contract.Contract.TypeId)
                             ?? throw new ModuleNotFoundException($"Неизвестный тип сериализованного медиа-объекта поста {contract.Contract.TypeId}");
            var bytes = contract.Contract.BinaryData != null ? Convert.FromBase64String(contract.Contract.BinaryData) : null;
            return serializer.Deserialize(bytes);
        }
    }
}