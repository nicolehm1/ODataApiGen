using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class AssociationSet : Annotable
    {
        public EntityContainer EntityContainer {get; private set;}
        public AssociationSet(XElement element, EntityContainer container) : base(element)
        {
            EntityContainer = container;
            Name = element.Attribute("Name")?.Value;
            Association = element.Attribute("Association")?.Value;

            AssociationSetEnds = element.Descendants().ToList().Where(a => a.Name.LocalName == "End")
                .Select(end => new AssociationSetEnd(end, this));
        }

        public string Name { get; private set; }
        public string Namespace => EntityContainer.Namespace; 
        public string NamespaceQualifiedName => $"{Namespace}.{Name}";
        public string Association { get; private set; }
        public IEnumerable<AssociationSetEnd> AssociationSetEnds { get; set; }
    }
}
