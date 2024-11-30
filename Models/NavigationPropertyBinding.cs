using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class NavigationPropertyBinding
    {
        public EntitySet? EntitySet {get; private set;}
        public Singleton? Singleton {get; private set;}
        public NavigationPropertyBinding(XElement xElement, EntitySet entitySet)
        {
            EntitySet = entitySet;
            Path = xElement.Attribute("Path")!.Value;
            Target = xElement.Attribute("Target")!.Value;
        }
        public NavigationPropertyBinding(XElement xElement, Singleton singleton)
        {
            Singleton = singleton;
            Path = xElement.Attribute("Path")!.Value;
            Target = xElement.Attribute("Target")!.Value;
        }
        public string Path { get; set; }
        public string Target { get; set; }
        public EntityType EntityType => Program.Metadata.FindEntityType(EntitySet?.EntityType);

        public EntityType PropertyType { 
            get {
                var parts = Path.Split('/');
                var entity = Program.Metadata.FindEntityType(EntitySet?.EntityType);
                if (parts.Length > 1) {
                    foreach (var nameOrType in parts.Take(parts.Length - 1))
                    {
                        var baseEntity = Program.Metadata.FindEntityType(nameOrType);
                        entity = baseEntity ?? entity.FindNavigationProperty(nameOrType).EntityType;
                    }
                }
                return entity;
            }
        }
        public string PropertyName => Path.Split('/').Last();
        public NavigationProperty NavigationProperty => PropertyType.FindNavigationProperty(PropertyName);
    }
}