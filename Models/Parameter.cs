using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class Parameter : Annotable
    {
        public Parameter(XElement xElement) : base(xElement)
        {
            Name = xElement.Attribute("Name")?.Value;
            Nullable = xElement.Attribute("Nullable")?.Value != "false";
            IsCollection = xElement.Attribute("Type")?.Value.StartsWith("Collection(") ?? false;
            Type = xElement.Attribute("Type")!.Value;
            if (Type.StartsWith("Collection("))
                Type = Type.Substring(11, Type.Length - 12);
        }
        public string? Name { get; set; }
        public bool IsEdmType => !String.IsNullOrWhiteSpace(Type) && Type.StartsWith("Edm.");
        public bool IsBinding => Name == "bindingParameter";
        public string Type { get; set; }
        public bool Nullable { get; set; }
        public bool IsCollection { get; set; }
    }
}
