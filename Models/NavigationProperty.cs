using System.Xml.Linq;

namespace ODataApiGen.Models
{
    public class NavigationProperty : Property
  {
    public NavigationProperty(XElement xElement, StructuredType structured) : base(xElement, structured)
    {
      Name = xElement.Attribute("Name")?.Value.Split(".").Last();
      NamespaceQualifiedName = xElement.Attribute("Name")?.Value;
      MaxLength = null;
      ContainsTarget = xElement.Attribute("ContainsTarget")?.Value == "true";
      Partner = xElement.Attribute("Partner")?.Value;
      IsCollection = xElement.Attribute("Type")?.Value.StartsWith("Collection(") ?? false;
      Type = xElement.Attribute("Type")?.Value;
      if (!string.IsNullOrWhiteSpace(Type) && Type.StartsWith("Collection("))
          Type = Type.Substring(11, Type.Length - 12);

      Referentials = xElement.Descendants()
              .Where(a => a.Name.LocalName == "ReferentialConstraint")
              .Select(key => new ReferentialConstraint {
                  Property = key.Attribute("Property")?.Value,
                  ReferencedProperty = key.Attribute("ReferencedProperty")?.Value
              })
              .ToList();
              
      // Version 2 and 3
      Relationship = xElement.Attribute("Relationship")?.Value;
      ToRole = xElement.Attribute("ToRole")?.Value;
      FromRole = xElement.Attribute("FromRole")?.Value;
    }
    public string? NamespaceQualifiedName { get; set; }
    public string? Partner { get; set; }
    public bool ContainsTarget { get; set; }
    public IEnumerable<ReferentialConstraint> Referentials { get; set; }
    public string? Relationship { get; set; }
    public string? ToRole { get; set; }
    public string? FromRole { get; set; }
    public Association? Association { get; set; }
    public string FromEntityType
    {
      get
      {
        return Association != null ?
            Association.Ends.First(e => e.Role == FromRole).Type : 
            "";
      }
    }
    public string ToEntityType 
    {
      get
      {
        return Association != null ?
            Association.Ends.First(e => e.Role == ToRole).Type : 
            "";
      }
    }
    public bool Many 
    {
      get
      {
        return Association != null && Association.Ends.First(e => e.Role == ToRole).Multiplicity == "*";
      }
    }
  }
    public class ReferentialConstraint
    {
        public string? Property { get; set; }
        public string? ReferencedProperty { get; set; }
    }
}