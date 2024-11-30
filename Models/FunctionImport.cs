using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class FunctionImport
    {
        public EntityContainer EntityContainer {get; private set;}
        public FunctionImport(XElement xElement, EntityContainer container)
        {
            EntityContainer = container;
            EntitySet = xElement.Attribute("EntitySet")?.Value;
            Name = xElement.Attribute("Name")?.Value;
            IncludeInServiceDocument = xElement.Attribute("IncludeInServiceDocument")?.Value == "true";
            Function = xElement.Attribute("Function")?.Value;
        }

        public string Name { get; private set; }
        public string Namespace => EntityContainer.Namespace; 
        public string NamespaceQualifiedName => $"{Namespace}.{Name}";
        public string Function { get; private set; }
        public string EntitySet { get; private set; }
        public bool IncludeInServiceDocument { get; private set; }
    }
}
