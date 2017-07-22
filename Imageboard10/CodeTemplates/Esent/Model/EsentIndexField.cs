using System.ComponentModel;
using System.Xml.Serialization;

namespace CodeTemplates.Esent.Model
{
    public class EsentIndexField
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Sort")]
        [DefaultValue(EsentIndexSort.Asc)]
        public EsentIndexSort Sort { get; set; }
    }
}