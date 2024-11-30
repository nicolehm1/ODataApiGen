using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public class EntityProperty : ILiquidizable
  {
    protected Property Value { get; set; }
    protected StructuredType Structured { get; set; }
    public EntityProperty(Property prop, StructuredType structured)
    {
      Structured = structured;
      Value = prop;
    }

    public string Name
    {
      get
      {
        var required = !(Value is NavigationProperty || Value.Nullable);
        var name = Utils.IsValidTypescriptName(Value.Name) ? Value.Name : $"\"{Value.Name}\"";
        return name + (!required ? "?" : "");
      }
    }

    public string Type
    {
      get
      {
        var pkg = (Package) Program.Package;
        var type = "any";
        if (Value.IsEnumType)
        {
          var e = pkg.FindEnum(Value.Type);
          type = e.ImportedName;
          type = type + (Value.IsCollection ? "[]" : "");
          //if (Value.Nullable) { type = type + " | null"; }
        }
        else if (Value.IsEdmType)
        {
          type = Structured.ToTypescriptType(Value.Type);
          type = type + (Value.IsCollection ? "[]" : "");
          //if (type != "string" && Value.Nullable && !Value.IsCollection) { type = type + " | null"; }
        }
        else if (Value.IsEntityType || Value.IsComplexType)
        {
          var entity = pkg.FindEntity(Value.Type);
          type = $"{entity.ImportedName}" + (Value.IsCollection ? "[]" : "");
          //if (Value.Nullable && !Value.IsCollection) { type = type + " | null"; }
        }
        else if (Value is NavigationProperty nav)
        {
          var entity = pkg.FindEntity(nav.ToEntityType);
          type = $"{entity.ImportedName}" + (nav.Many ? "[]" : "");
        }
        return type;
      }
    }
    public object ToLiquid()
    {
      return new
      {
          Name,
          Type
      };
    }
    public bool IsGeo => Value.Type.StartsWith("Edm.Geography") || Value.Type.StartsWith("Edm.Geometry");
  }
  public class Entity : StructuredType
  {
    public Entity(Models.StructuredType type, ApiOptions options) : base(type, options) { }

    public override string FileName => EdmStructuredType.Name.Dasherize() +
    (EdmStructuredType is ComplexType ? ".complex" : ".entity");
    public override string Name => Utils.ToTypescriptName(EdmStructuredType.Name, TypeScriptElement.Class);
    // Exports

    public IEnumerable<EntityProperty> Properties
    {
      get
      {
        var props = EdmStructuredType.Properties.ToList();
        if (EdmStructuredType is EntityType type)
          props.AddRange(type.NavigationProperties);
        return props.Select(prop => new EntityProperty(prop, this));
      }
    }
    public IEnumerable<EntityProperty> GeoProperties => Properties.Where(p => p.IsGeo);
    public bool HasGeoFields => Options.GeoJson && GeoProperties.Count() > 0;
    public string TypeName => Name + (EdmStructuredType is ComplexType ? "ComplexType" : "EntityType");
    public override object ToLiquid()
    {
      return new
      {
        Name = ImportedName,
        EntityType = EdmStructuredType.NamespaceQualifiedName
      };
    }
  }
}