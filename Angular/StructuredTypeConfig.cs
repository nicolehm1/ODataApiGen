using System.Text.Json;
using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public class EntityKeyConfig : ILiquidizable
  {
    protected PropertyRef Value { get; set; }
    protected StructuredTypeConfig Config { get; set; }
    public EntityKeyConfig(PropertyRef property, StructuredTypeConfig config)
    {
      Value = property;
      Config = config;
    }

    public override string ToString()
    {
      var values = new Dictionary<string, string>();
      values.Add("name", $"'{Value.Name}'");
      if (!String.IsNullOrWhiteSpace(Value.Alias))
      {
        values.Add("alias", $"'{Value.Alias}'");
      }
      return $"{{{String.Join(", ", values.Select(p => $"{p.Key}: {p.Value}"))}}}";
    }
    public object ToLiquid()
    {
      return new
      {
        Value = ToString(),
      };
    }
  }
  public class EntityFieldConfig : ILiquidizable
  {
    protected Property Value { get; set; }
    protected StructuredTypeConfig Config { get; set; }
    public EntityFieldConfig(Property property, StructuredTypeConfig config)
    {
      Value = property;
      Config = config;
    }
    //public string Name => AngularRenderable.ToTypescriptName(Value.Name, TypeScriptElement.Method);
    public string Name => Utils.IsValidTypescriptName(Value.Name) ? Value.Name : $"\"{Value.Name}\"";

    public string Type
    {
      get
      {
        var values = new Dictionary<string, string>();
        if (Name != Value.Name)
          values.Add("name", $"'{Value.Name}'");
        if (Value.Type != null)
        {
          values.Add("type", $"'{Value.Type}'");
        }
        else if (Value is NavigationProperty nav)
        {
          values.Add("type", $"'{nav.ToEntityType}'");
        }
        if (!(Value is NavigationProperty) && !Value.Nullable)
          values.Add("nullable", "false");
        if (!String.IsNullOrEmpty(Value.MaxLength) && Value.MaxLength.ToLower() != "max")
          values.Add("maxLength", Value.MaxLength);
        if (!String.IsNullOrEmpty(Value.DefaultValue))
          values.Add("default", $"'{Value.DefaultValue}'");
        if (!String.IsNullOrEmpty(Value.SRID))
          values.Add("srid", $"'{Value.SRID}'");
        if (!String.IsNullOrEmpty(Value.Precision))
          values.Add("precition", Value.Precision);
        if (!String.IsNullOrEmpty(Value.Scale)) {
          var value = Value.Scale.ToLower();
          if (value == "variable")
            value = "'variable'";
          values.Add("scale", value);
        }
        if (Value.IsCollection || Value is NavigationProperty property && property.Many)
          values.Add("collection", "true");
        if (Value is NavigationProperty navigationProperty)
        {
          // Is Navigation
          values.Add("navigation", "true");
          if (navigationProperty.Referentials.Count() > 0)
          {
            values.Add("referentials", $"[{String.Join(", ", navigationProperty.Referentials.Select(p => $"{{property: '{p.Property}', referencedProperty: '{p.ReferencedProperty}'}}"))}]");
          }
        }
        var annots = Value.Annotations;
        if (annots.Count > 0)
        {
          var json = JsonSerializer.Serialize(annots.Select(annot => annot.ToDictionary()));
          values.Add("annotations", $"{json}");
        }
        return $"{{{String.Join(", ", values.Select(p => $"{p.Key}: {p.Value}"))}}}";
      }
    }
    public object ToLiquid()
    {
      return new
      {
        Name, Type
      };
    }
  }
  public class StructuredTypeConfig : AngularRenderable, ILiquidizable
  {
    public Entity Entity { get; private set; }
    public Model Model { get; private set; }
    public Collection Collection { get; private set; }
    public StructuredTypeConfig(Entity entity, ApiOptions options) : base(options)
    {
      Entity = entity;
      AddDependency(entity);
    }
    public StructuredTypeConfig(Entity entity, Model model, Collection collection, ApiOptions options) : this(entity, options)
    {
      Model = model;
      Collection = collection;
      AddDependency(model);
      AddDependency(collection);
    }
    public override string FileName => Entity.FileName + ".config";
    public override string Name => Entity.Name +
    (Entity.EdmStructuredType is ComplexType ? "ComplexConfig" : "EntityConfig");
    public string EntityType => Entity.EdmStructuredType.NamespaceQualifiedName;
    public string EdmEntityName => Entity.EdmStructuredType.Name;
    public string EntityName => Entity.Name;
    public bool OpenType => Entity.OpenType;

    public bool HasAnnotations => Entity.EdmStructuredType.Annotations.Count > 0;
    public string Annotations => JsonSerializer.Serialize(Entity.EdmStructuredType.Annotations.Select(annot => annot.ToDictionary()), new JsonSerializerOptions { WriteIndented = true });
    public bool HasKey => Entity.EdmStructuredType is EntityType type && type.Keys.Count > 0;
    public IEnumerable<EntityKeyConfig> Keys
    {
      get
      {
        var keys = Entity.EdmStructuredType is EntityType type ? type.Keys : new List<PropertyRef>();
        return keys.Select(prop => new EntityKeyConfig(prop, this));
      }
    }
    public IEnumerable<EntityFieldConfig> Properties
    {
      get
      {
        var props = Entity.EdmStructuredType.Properties.ToList();
        if (Entity.EdmStructuredType is EntityType type)
          props.AddRange(type.NavigationProperties);
        return props.Select(prop => new EntityFieldConfig(prop, this));
      }
    }

    // Imports
    public override IEnumerable<string> ImportTypes => new List<string> { EntityType };
    public override string Directory => Entity.EdmStructuredType.Namespace.Replace('.', Path.DirectorySeparatorChar);
    public override IEnumerable<Import> Imports => GetImportRecords();

    public StructuredType Base => Entity.Base;
    public object ToLiquid()
    {
      return new
      {
        EntityName,
        Name = ImportedName
      };
    }
  }
}