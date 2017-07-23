using System;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Isam.Esent.Interop;

namespace CodeTemplates.Esent.Model
{
    public class EsentViewField
    {
        [XmlText]
        public string Name { get; set; }

        [XmlAttribute("FetchFlags")]
        [DefaultValue(EsentViewFieldFetchFlags.Default)]
        public EsentViewFieldFetchFlags FetchFlags { get; set; }

        public string GetRetrieveFlagsString()
        {
            if (FetchFlags == EsentViewFieldFetchFlags.Default)
            {
                return "RetrieveColumnGrbit.None";
            }
            StringBuilder sb = new StringBuilder();

            void AddFlag(EsentViewFieldFetchFlags f, string jetFlag)
            {
                if (FetchFlags.HasFlag(f))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" | ");
                    }
                    sb.Append(jetFlag);
                }
            }

            AddFlag(EsentViewFieldFetchFlags.FromIndex, "RetrieveColumnGrbit.RetrieveFromIndex");
            AddFlag(EsentViewFieldFetchFlags.FromPrimaryBookmark, "RetrieveColumnGrbit.RetrieveFromPrimaryBookmark");
            return sb.ToString();
        }
    }

    [Flags]
    public enum EsentViewFieldFetchFlags
    {
        Default = 0,
        FromIndex = 0x0001,
        FromPrimaryBookmark = 0x0002,
    }
}