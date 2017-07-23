using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.Isam.Esent.Interop.Windows10;

namespace CodeTemplates.Esent.Model
{
    public class EsentColumndef
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public EsentColtyp Type { get; set; }

        [XmlAttribute("Codepage")]
        [DefaultValue(EsentCodepage.None)]
        public EsentCodepage Codepage { get; set; }

        [XmlAttribute("Size")]
        [DefaultValue(0)]
        public int Size { get; set; }

        [XmlAttribute("Grbit")]
        [DefaultValue(EsentColumndefGrbit.None)]
        public EsentColumndefGrbit Grbit { get; set; }

        [XmlElement("DefaultValue")]
        [DefaultValue("")]
        public string DefaultValueBytes { get; set; }

        public string GetNetType(bool? nullable = null)
        {
            return GetNetType(Type, nullable ?? IsNullable());
        }

        public bool IsNullable()
        {
            return !Grbit.HasFlag(EsentColumndefGrbit.NotNULL);
        }

        public bool IsReferenceType()
        {
            return ReferenceTypes.Contains(Type);
        }

        public static string GetNetType(EsentColtyp coltyp, bool nullable)
        {
            switch (coltyp)
            {
                case EsentColtyp.Byte:
                    return nullable ? "byte?" : "byte";
                case EsentColtyp.Boolean:
                    return nullable ? "bool?" : "bool";
                case EsentColtyp.SignedInt16:
                    return nullable ? "short?" : "short";
                case EsentColtyp.UnsignedInt16:
                    return nullable ? "ushort?" : "ushort";
                case EsentColtyp.SignedInt32:
                    return nullable ? "int?" : "int";
                case EsentColtyp.UnsignedInt32:
                    return nullable ? "uint?" : "uint";
                case EsentColtyp.SignedInt64:
                    return nullable ? "long?" : "long";
                case EsentColtyp.UnsignedInt64:
                    return nullable ? "ulong?" : "ulong";
                case EsentColtyp.Float:
                    return nullable ? "float?" : "float";
                case EsentColtyp.Double:
                    return nullable ? "double?" : "double";
                case EsentColtyp.DateTime:
                    return nullable ? "DateTime?" : "DateTime";
                case EsentColtyp.LongBinary:
                case EsentColtyp.Binary:
                    return "byte[]";
                case EsentColtyp.LongText:
                case EsentColtyp.Text:
                    return "string";
                case EsentColtyp.Guid:
                    return nullable ? "Guid?" : "Guid";
                default:
                    throw new ArgumentException($"Unknown coltyp {coltyp}");
            }
        }

        public string GetJetColtyp()
        {
            return GetJetColtyp(Type);
        }

        public static string GetJetColtyp(EsentColtyp coltyp)
        {
            switch (coltyp)
            {
                case EsentColtyp.Byte:
                    return "JET_coltyp.UnsignedByte";
                case EsentColtyp.Boolean:
                    return "JET_coltyp.Bit";
                case EsentColtyp.SignedInt16:
                    return "JET_coltyp.Short";
                case EsentColtyp.UnsignedInt16:
                    return "VistaColtyp.UnsignedShort";
                case EsentColtyp.SignedInt32:
                    return "JET_coltyp.Long";
                case EsentColtyp.UnsignedInt32:
                    return "VistaColtyp.UnsignedLong";
                case EsentColtyp.SignedInt64:
                    return "JET_coltyp.Currency";
                case EsentColtyp.UnsignedInt64:
                    return "Windows10Coltyp.UnsignedLongLong";
                case EsentColtyp.Float:
                    return "JET_coltyp.IEEESingle";
                case EsentColtyp.Double:
                    return "JET_coltyp.IEEEDouble";
                case EsentColtyp.DateTime:
                    return "JET_coltyp.DateTime";
                case EsentColtyp.Binary:
                    return "JET_coltyp.Binary";
                case EsentColtyp.Text:
                    return "JET_coltyp.Text";
                case EsentColtyp.LongBinary:
                    return "JET_coltyp.LongBinary";
                case EsentColtyp.LongText:
                    return "JET_coltyp.LongText";
                case EsentColtyp.Guid:
                    return "VistaColtyp.GUID";
                default:
                    throw new ArgumentException($"Unknown coltyp {coltyp}");
            }
        }

        public string GetRetrieveFuncName()
        {
            return GetRetrieveFuncName(Type);
        }

        public static string GetRetrieveFuncName(EsentColtyp coltyp)
        {
            switch (coltyp)
            {
                case EsentColtyp.Byte:
                    return "Api.RetrieveColumnAsByte";
                case EsentColtyp.Boolean:
                    return "Api.RetrieveColumnAsBoolean";
                case EsentColtyp.SignedInt16:
                    return "Api.RetrieveColumnAsInt16";
                case EsentColtyp.UnsignedInt16:
                    return "Api.RetrieveColumnAsUInt16";
                case EsentColtyp.SignedInt32:
                    return "Api.RetrieveColumnAsInt32";
                case EsentColtyp.UnsignedInt32:
                    return "Api.RetrieveColumnAsUInt32";
                case EsentColtyp.SignedInt64:
                    return "Api.RetrieveColumnAsInt64";
                case EsentColtyp.UnsignedInt64:
                    return "Api.RetrieveColumnAsUInt64";
                case EsentColtyp.Float:
                    return "Api.RetrieveColumnAsFloat";
                case EsentColtyp.Double:
                    return "Api.RetrieveColumnAsDouble";
                case EsentColtyp.DateTime:
                    return "Api.RetrieveColumnAsDateTime";
                case EsentColtyp.Binary:
                case EsentColtyp.LongBinary:
                    return "Api.RetrieveColumn";
                case EsentColtyp.Text:
                case EsentColtyp.LongText:
                    return "Api.RetrieveColumnAsString";
                case EsentColtyp.Guid:
                    return "Api.RetrieveColumnAsGuid";
                default:
                    throw new ArgumentException($"Unknown coltyp {coltyp}");
            }
        }

        public string GetColumnValueType()
        {
            return GetColumnValueType(Type);
        }

        public static string GetColumnValueType(EsentColtyp coltyp)
        {
            switch (coltyp)
            {
                case EsentColtyp.Byte:
                    return "ByteColumnValue";
                case EsentColtyp.Boolean:
                    return "BoolColumnValue";
                case EsentColtyp.SignedInt16:
                    return "Int16ColumnValue";
                case EsentColtyp.UnsignedInt16:
                    return "UInt16ColumnValue";
                case EsentColtyp.SignedInt32:
                    return "Int32ColumnValue";
                case EsentColtyp.UnsignedInt32:
                    return "UInt32ColumnValue";
                case EsentColtyp.SignedInt64:
                    return "Int64ColumnValue";
                case EsentColtyp.UnsignedInt64:
                    return "UInt64ColumnValue";
                case EsentColtyp.Float:
                    return "FloatColumnValue";
                case EsentColtyp.Double:
                    return "DoubleColumnValue";
                case EsentColtyp.DateTime:
                    return "DateTimeColumnValue";
                case EsentColtyp.Binary:
                case EsentColtyp.LongBinary:
                    return "BytesColumnValue";
                case EsentColtyp.Text:
                case EsentColtyp.LongText:
                    return "StringColumnValue";
                case EsentColtyp.Guid:
                    return "GuidColumnValue";
                default:
                    throw new ArgumentException($"Unknown coltyp {coltyp}");
            }
        }

        public static readonly HashSet<EsentColtyp> ReferenceTypes = new HashSet<EsentColtyp>() { EsentColtyp.Binary, EsentColtyp.LongBinary, EsentColtyp.Text, EsentColtyp.LongText };

        public static string MapToJetColumndefGrbit(EsentColumndefGrbit grbit)
        {
            switch (grbit)
            {
                case EsentColumndefGrbit.None:
                    return "ColumndefGrbit.None";
                case EsentColumndefGrbit.Fixed:
                    return "ColumndefGrbit.ColumnFixed";
                case EsentColumndefGrbit.Tagged:
                    return "ColumndefGrbit.ColumnTagged";
                case EsentColumndefGrbit.NotNULL:
                    return "ColumndefGrbit.ColumnNotNULL";
                case EsentColumndefGrbit.Version:
                    return "ColumndefGrbit.ColumnVersion";
                case EsentColumndefGrbit.Autoincrement:
                    return "ColumndefGrbit.ColumnAutoincrement";
                case EsentColumndefGrbit.MultiValued:
                    return "ColumndefGrbit.ColumnMultiValued";
                case EsentColumndefGrbit.EscrowUpdate:
                    return "ColumndefGrbit.ColumnEscrowUpdate";
                case EsentColumndefGrbit.Unversioned:
                    return "ColumndefGrbit.ColumnUnversioned";
                case EsentColumndefGrbit.MaybeNull:
                    return "ColumndefGrbit.ColumnMaybeNull";
                case EsentColumndefGrbit.UserDefinedDefault:
                    return "ColumndefGrbit.ColumnUserDefinedDefault";
                case EsentColumndefGrbit.TTKey:
                    return "ColumndefGrbit.TTKey";
                case EsentColumndefGrbit.TTDescending:
                    return "ColumndefGrbit.TTDescending";
                default:
                    throw new ArgumentException($"Unknown grbit {grbit}");
            }
        }

        public static string GetJetColumndefGrbitsString(EsentColumndefGrbit grbit)
        {
            if (grbit == EsentColumndefGrbit.None)
            {
                return MapToJetColumndefGrbit(EsentColumndefGrbit.None);
            }
            var sb = new StringBuilder();

            void AddFlag(EsentColumndefGrbit g)
            {
                if (grbit.HasFlag(g))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" | ");
                    }
                    sb.Append(MapToJetColumndefGrbit(g));
                }
            }

            AddFlag(EsentColumndefGrbit.Fixed);
            AddFlag(EsentColumndefGrbit.Tagged);
            AddFlag(EsentColumndefGrbit.NotNULL);
            AddFlag(EsentColumndefGrbit.Version);
            AddFlag(EsentColumndefGrbit.Autoincrement);
            AddFlag(EsentColumndefGrbit.MultiValued);
            AddFlag(EsentColumndefGrbit.EscrowUpdate);
            AddFlag(EsentColumndefGrbit.Unversioned);
            AddFlag(EsentColumndefGrbit.MaybeNull);
            AddFlag(EsentColumndefGrbit.UserDefinedDefault);
            AddFlag(EsentColumndefGrbit.TTKey);
            AddFlag(EsentColumndefGrbit.TTDescending);

            return sb.ToString();
        }

        public static string GetJetCodepage(EsentCodepage codepage)
        {
            switch (codepage)
            {
                case EsentCodepage.Ascii:
                    return "JET_CP.ASCII";
                case EsentCodepage.Unicode:
                    return "JET_CP.Unicode";
                case EsentCodepage.None:
                    return "JET_CP.None";
                default:
                    throw new ArgumentException($"Unknown codepage {codepage}");
            }
        }

        public IEnumerable<string> GenerateCreateColumnLines()
        {
            yield return $"coltyp = {GetJetColtyp()},";
            yield return $"grbit = {GetJetColumndefGrbitsString(Grbit)},";
            if (Codepage != EsentCodepage.None)
            {
                yield return $"cp = {GetJetCodepage(Codepage)},";
            }
            if (Size > 0)
            {
                yield return $"cbMax = {Size},";
            }
        }
    }
}