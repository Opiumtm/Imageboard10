using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace CodeTemplates.Esent.Model
{
    public class EsentViewField
    {
        [XmlText]
        public string Name { get; set; }

        [XmlAttribute("FetchFlags")]
        [DefaultValue(EsentViewFieldFetchFlags.Default)]
        public EsentViewFieldFetchFlags FetchFlags { get; set; }
    }

    [Flags]
    public enum EsentViewFieldFetchFlags
    {
        Default = 0,
        FromIndex = 0x0001,
        FromPrimaryBookmark = 0x0002,
    }
}