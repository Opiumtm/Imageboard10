using System.ComponentModel;
using System.Xml.Serialization;

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

        [XmlArray("Searches")]
        [XmlArrayItem(typeof(EsentSearchSeek), ElementName = "Seek")]
        [XmlArrayItem(typeof(EsentSearchRange), ElementName = "Range")]
        public EsentSearch[] Searches { get; set; }
    }
}