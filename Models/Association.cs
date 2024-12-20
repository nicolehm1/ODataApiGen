using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class Association : Annotable {
        public Schema Schema {get; private set;}
        public Association(XElement element, Schema schema) : base(element)
        {
            Schema = schema;
            Name = element.Attribute("Name")?.Value;
            Ends = element.Descendants().Where(a => a.Name.LocalName == "End")
                .Select(end => new AssociationEnd(end, this)).ToList();
        }

        public List<AssociationEnd> Ends { get; private set; }
        public string Namespace => Schema.Namespace;
        public string Name { get; private set; }
        public string NamespaceQualifiedName { get { return $"{Namespace}.{Name}"; } }
    }
}