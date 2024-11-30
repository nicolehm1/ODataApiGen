using System.Xml.Linq;
using DotLiquid;

namespace ODataApiGen.Models
{
    public abstract class StructuredType : Annotable, ILiquidizable
    {
        public Schema Schema {get; private set;}
        public StructuredType(XElement element, Schema schema) : base(element)
        {
            Schema = schema;
            Name = element.Attribute("Name")?.Value;
            BaseType = element.Attribute("BaseType")?.Value;
            OpenType = element.Attribute("OpenType")?.Value == "true";

            Properties = element.Descendants().Where(a => a.Name.LocalName == "Property")
                .Select(prop => new Property(prop, this)).ToList();
        }
        public bool IsBaseOf(StructuredType structured)
        {
            if (IsTypeOf(structured.BaseType)) return true;
            var baseType = Program.Metadata.FindEntityType(structured.BaseType);
            if (baseType != default)
                return IsBaseOf(baseType);
            return false;
        }
        public bool IsTypeOf(string type) {
            var names = new List<string> {$"{Schema.Namespace}.{Name}"};
            if (!String.IsNullOrEmpty(Schema.Alias))
                names.Add($"{Schema.Alias}.{Name}");
            return names.Contains(type);
        }
        public int HierarchyLevelOf(StructuredType structured, int level = 0)
        {
            if (IsTypeOf(structured.BaseType)) return level;
            var baseType = Program.Metadata.FindEntityType(structured.BaseType);
            if (baseType != default)
                return HierarchyLevelOf(baseType, level + 1);
            return level;
        }
        public string Namespace => Schema.Namespace;
        public string Alias => Schema.Alias;
        public string Name { get; private set; }
        public string BaseType { get; private set; }
        public bool OpenType { get; private set; }
        public string NamespaceQualifiedName => $"{Namespace}.{Name}";
        public string AliasQualifiedName => $"{Alias}.{Name}";
        public List<Property> Properties { get; private set; }
        public object ToLiquid()
        {
            return new
            {
                Name,
                NamespaceQualifiedName,
                AliasQualifiedName,
            };
        }
    }
}
