using System.ComponentModel;
using System.Xml.Serialization;

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
    }
}