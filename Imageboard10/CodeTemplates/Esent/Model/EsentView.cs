using System.ComponentModel;
using System.Xml.Serialization;

namespace CodeTemplates.Esent.Model
{
    public class EsentView
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Role")]
        [DefaultValue(EsentViewRole.None)]
        public EsentViewRole Role { get; set; }

        [XmlElement("Field")]
        public EsentViewField[] Fields { get; set; }

        [XmlArray("AssignableTo")]
        [XmlArrayItem("ViewName")]
        public string[] AssignableTo { get; set; }
    }
}