using System.Xml.Linq;
using DotLiquid;

namespace ODataApiGen.Models
{
    public class EnumType : Annotable, ILiquidizable
    {
        public Schema Schema {get; private set;}
        public string Namespace => Schema.Namespace;
        public string Alias => Schema.Alias;
        public string Name { get; private set; }
        public string NamespaceQualifiedName => $"{Namespace}.{Name}";
        public string AliasQualifiedName => $"{Alias}.{Name}";
        public bool Flags { get; private set; }
        public IEnumerable<EnumMember> Members { get; private set; }
        public EnumType(XElement element, Schema schema) : base(element)
        {
            Schema = schema;
            Name = element.Attribute("Name")?.Value;
            Flags = element.Attribute("IsFlags")?.Value == "true";
            Members = element.Descendants().Where(a => a.Name.LocalName == "Member")
                .Select(member => new EnumMember(member, this)).ToList();
        }
        public bool IsTypeOf(string type) {
            var names = new List<string> {$"{Schema.Namespace}.{Name}"};
            if (!String.IsNullOrEmpty(Schema.Alias))
                names.Add($"{Schema.Alias}.{Name}");
            return names.Contains(type);
        }

        public object ToLiquid()
        {
            return new
            {
                Name,
                NamespaceQualifiedName
            };
        }
    }
}
