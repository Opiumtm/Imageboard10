using System.ComponentModel;
using System.Xml.Serialization;

namespace CodeTemplates.Esent.Model
{
    public abstract class EsentSearch
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("View")]
        [DefaultValue("")]
        public string View { get; set; }
    }

    public class EsentSearchSeek : EsentSearch
    {
        [XmlElement("SeekKey")]
        public EsentKey SeekKey { get; set; }

        [XmlAttribute("Unique")]
        [DefaultValue(false)]
        public bool Unique { get; set; }
    }

    public class EsentSearchRange : EsentSearchSeek
    {
        [XmlElement("RangeKey")]
        public EsentKey RangeKey { get; set; }

        [XmlAttribute("RangeType")]
        public EsentSetIndexRangeGrbit RangeType { get; set; }
    }

    public class EsentKey
    {
        [XmlElement("KeyPart")]
        public EsentMakeKeyGrbit[] KeyParts { get; set; }
    }
}