using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class ActionImport
    {
        public EntityContainer EntityContainer {get; private set;}
        public ActionImport(XElement xElement, EntityContainer container)
        {
            EntityContainer = container;
            Name = xElement.Attribute("Name")?.Value;
            EntitySet = xElement.Attribute("EntitySet")?.Value;
            Action = xElement.Attribute("Action")?.Value;
        }

        public string Name { get; private set; }
        public string Namespace => EntityContainer.Namespace; 
        public string NamespaceQualifiedName => $"{Namespace}.{Name}";
        public string Action { get; private set; }
        public string EntitySet { get; private set; }
    }
}
