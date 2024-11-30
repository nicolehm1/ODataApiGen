using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
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
        return Utils.IsValidTypeScrtiptName(Value.Name) ? Value.Name : $"\"{Value.Name}\"";
        /*
        var navigation = Value is NavigationProperty;
        var name = Utils.IsValidTypeScrtiptName(Value.Name) ? Value.Name : $"\"{Value.Name}\"";
        return name + (navigation ? "?" : "");
        */
      }
    }

    public string Type
    {
      get
      {
        var pkg = Program.Package as Package;
        var nullable = Value.Nullable;
        var type = "dynamic";
        if (Value.IsEnumType)
        {
          var e = pkg.FindEnum(Value.Type);
          type = e.ImportedName;
        }
        else if (Value.IsEdmType)
        {
          type = Structured.ToTypescriptType(Value.Type);
          type = Value.IsCollection ? $"List<{type}>" : type;
        }
        else if (Value.IsEntityType || Value.IsComplexType)
        {
          var entity = pkg.FindEntity(Value.Type);
          type = Value.IsCollection ? $"List<{entity.ImportedName}>" : entity.ImportedName;
        }
        else if (Value is NavigationProperty nav)
        {
          var entity = pkg.FindEntity(nav.ToEntityType);
          type = nav.Many ? $"List<{entity.ImportedName}>" : entity.ImportedName;
        }
        var required = !(Value is NavigationProperty || Value.Nullable);
        type = type + (!required ? "?" : "");
        return type;
      }
    }
    public object ToLiquid()
    {
      return new
      {
        Name, Type
      };
    }
    public bool IsGeo => Value.Type.StartsWith("Edm.Geography") || Value.Type.StartsWith("Edm.Geometry");
  }
  public class Entity : StructuredType
  {
    public Entity(Models.StructuredType type, ApiOptions options) : base(type, options) { }

    public override string FileName => EdmStructuredType.Name.Dasherize() +
    (EdmStructuredType is ComplexType ? ".complex" : ".entity");
    public override string Name => Utils.ToDartName(EdmStructuredType.Name, DartElement.Class);
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