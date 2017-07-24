using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Isam.Esent.Interop;

namespace CodeTemplates.Esent.Model
{
    public class EsentIndex
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Grbit")]
        [DefaultValue(EsentIndexGrbit.None)]
        public EsentIndexGrbit Grbit { get; set; }

        [XmlArray(ElementName = "Fields")]
        [XmlArrayItem(ElementName = "Field")]
        public EsentIndexField[] Fields { get; set; }

        [XmlArray("Views")]
        [XmlArrayItem("ViewName")]
        public string[] Views { get; set; }

        public static string MapToJetIndexGrbit(EsentIndexGrbit grbit)
        {
            switch (grbit)
            {
                case EsentIndexGrbit.None:
                    return "CreateIndexGrbit.None";
                case EsentIndexGrbit.Unique:
                    return "CreateIndexGrbit.IndexUnique";
                case EsentIndexGrbit.Primary:
                    return "CreateIndexGrbit.IndexPrimary";
                case EsentIndexGrbit.DisallowNull:
                    return "CreateIndexGrbit.IndexDisallowNull";
                case EsentIndexGrbit.IgnoreNull:
                    return "CreateIndexGrbit.IndexIgnoreNull";
                case EsentIndexGrbit.IgnoreAnyNull:
                    return "CreateIndexGrbit.IndexIgnoreAnyNull";
                case EsentIndexGrbit.IgnoreFirstNull:
                    return "CreateIndexGrbit.IndexIgnoreFirstNull";
                case EsentIndexGrbit.LazyFlush:
                    return "CreateIndexGrbit.IndexLazyFlush";
                case EsentIndexGrbit.Empty:
                    return "CreateIndexGrbit.IndexEmpty";
                case EsentIndexGrbit.Unversioned:
                    return "CreateIndexGrbit.IndexUnversioned";
                case EsentIndexGrbit.SortNullsHigh:
                    return "CreateIndexGrbit.IndexSortNullsHigh";
                default:
                    throw new ArgumentException($"Unknown grbit {grbit}");
            }
        }

        public static string GetJetIndexGrbitsString(EsentIndexGrbit grbit)
        {
            if (grbit == EsentIndexGrbit.None)
            {
                return MapToJetIndexGrbit(EsentIndexGrbit.None);
            }
            var sb = new StringBuilder();

            void AddFlag(EsentIndexGrbit g)
            {
                if (grbit.HasFlag(g))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" | ");
                    }
                    sb.Append(MapToJetIndexGrbit(g));
                }
            }

            AddFlag(EsentIndexGrbit.Unique);
            AddFlag(EsentIndexGrbit.Primary);
            AddFlag(EsentIndexGrbit.DisallowNull);
            AddFlag(EsentIndexGrbit.IgnoreNull);
            AddFlag(EsentIndexGrbit.IgnoreAnyNull);
            AddFlag(EsentIndexGrbit.IgnoreFirstNull);
            AddFlag(EsentIndexGrbit.LazyFlush);
            AddFlag(EsentIndexGrbit.Empty);
            AddFlag(EsentIndexGrbit.Unversioned);
            AddFlag(EsentIndexGrbit.SortNullsHigh);
            return sb.ToString();
        }

        public string GetJetIndexGrbitsString()
        {
            return GetJetIndexGrbitsString(Grbit);
        }

        public string GetIndexDefString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var f in Fields)
            {
                switch (f.Sort)
                {
                    case EsentIndexSort.Asc:
                        sb.Append("+");
                        break;
                    case EsentIndexSort.Desc:
                        sb.Append("-");
                        break;
                    default:
                        throw new ArgumentException($"Invalid sort {f.Sort}");
                }
                sb.Append(f.Name);
                sb.Append("\\0");
            }
            sb.Append("\\0");
            return sb.ToString();
        }
    }
}