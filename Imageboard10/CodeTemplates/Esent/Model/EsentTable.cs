using System.ComponentModel;
using System.Xml.Serialization;

namespace CodeTemplates.Esent.Model
{
    [XmlRoot(ElementName = "Table")]
    public class EsentTable
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Namespace")]
        public string Namespace { get; set; }

        [XmlAttribute("Visibility")]
        [DefaultValue("internal")]
        public string Visibility { get; set; }

        [XmlArray("Columns")]
        [XmlArrayItem("Column")]
        public EsentColumndef[] Columns { get; set; }

        [XmlArray("Indexes")]
        [XmlArrayItem("Index")]
        public EsentIndex[] Indexes { get; set; }

        [XmlArray("Views")]
        [XmlArrayItem("View")]
        public EsentView[] Views { get; set; }
    }
}