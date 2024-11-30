using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public class ModelField : ILiquidizable
  {
    protected Property Value { get; set; }
    protected StructuredType Structured { get; set; }
    public ModelField(Property prop, StructuredType structured)
    {
      Value = prop;
      Structured = structured;
    }
    public string Name
    {
      get
      {
        var required = !(Value is NavigationProperty || Value.Nullable);
        var name = Utils.IsValidTypescriptName(Value.Name) ? Value.Name : $"\"{Value.Name}\"";
        return name + (!required ? "?" : "!");
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
        else if (Value.Type != null)
        {
          if (Value.IsCollection)
          {
            var entity = pkg.FindEntity(Value.Type);
            var model = pkg.FindModel(Value.Type);
            var collection = pkg.FindCollection(Value.Type);
            type = $"{collection.ImportedName}<{entity.ImportedName}, {model.ImportedName}<{entity.ImportedName}>>";
          }
          else
          {
            var entity = pkg.FindEntity(Value.Type);
            var model = pkg.FindModel(Value.Type);
            type = $"{model.ImportedName}<{entity.ImportedName}>";
          }
        }
        else if (Value is NavigationProperty nav)
        {
          if (nav.Many)
          {
            var entity = pkg.FindEntity(nav.ToEntityType);
            var model = pkg.FindModel(nav.ToEntityType);
            var collection = pkg.FindCollection(nav.ToEntityType);
            type = $"{collection.ImportedName}<{entity.ImportedName}, {model.ImportedName}<{entity.ImportedName}>>";
          }
          else
          {
            var entity = pkg.FindEntity(nav.ToEntityType);
            var model = pkg.FindModel(nav.ToEntityType);
            type = $"{model.ImportedName}<{entity.ImportedName}>";
          }
        }
        return type;
      }
    }

    public string Resource()
    {
      var pkg = Program.Package as Package;
      var resourceName = $"$${Value.Name}";
      if (Value is NavigationProperty nav)
      {
        var entity = nav.Type != null ?
            pkg.FindEntity(nav.Type) :
            pkg.FindEntity(nav.ToEntityType);
        // resource
        return $@"public {resourceName}() {{
    return this.navigationProperty<{entity.ImportedName}>('{nav.Name}');
  }}";
      }

      // resource
      return $@"public {resourceName}() {{
    return this.property<{Type}>('{Value.Name}');
  }}";
    }

    public string GetAttribute()
    {
      var pkg = Program.Package as Package;
      var getterName = $"${Value.Name}";
      if (Value is NavigationProperty nav)
      {
        var entity = nav.Type != null ?
          pkg.FindEntity(nav.Type) :
            pkg.FindEntity(nav.ToEntityType);
        return $@"public {getterName}() {{
    return this.getAttribute<{entity.ImportedName}>('{nav.Name}') as {Type};
  }}";
      }

      return $@"public {getterName}() {{
    return this.getAttribute<{Type}>('{Value.Name}') as {Type};
  }}";
    }

    public string FetchAttribute()
    {
      var pkg = Program.Package as Package;
      var fetchName = $"{Value.Name}$";
      if (Value is NavigationProperty nav)
      {
        var entity = nav.Type != null ?
            pkg.FindEntity(nav.Type) :
            pkg.FindEntity(nav.ToEntityType);
        return $@"public {fetchName}(options?: ODataQueryArgumentsOptions<{entity.ImportedName}>) {{
      return this.fetchAttribute<{entity.ImportedName}>('{nav.Name}', options) as Observable<{Type}>;
    }}";
      }

      return $@"public {fetchName}(options?: ODataQueryArgumentsOptions<{Type}>) {{
    return this.fetchAttribute<{Type}>('{Value.Name}', options) as Observable<{Type}>;
  }}";
    }
    public string SetterReference()
    {
      var pkg = Program.Package as Package;
      var nav = Value as NavigationProperty;
      var setterName = $"{Value.Name}$$";
      var entity = Value.Type != null ?
          pkg.FindEntity(Value.Type) :
          pkg.FindEntity(nav.ToEntityType);
      // setter
      return $@"public {setterName}(model: {Type} | null, options?: ODataOptions) {{
    return this.setReference<{entity.ImportedName}>('{Value.Name}', model, options);
  }}";
    }
    public object ToLiquid()
    {
      return new
      {
        Name,
        Type,
        Resource = Resource(),
        Getter = GetAttribute(),
        Setter = NeedReference ? SetterReference() : "",
        Fetch = FetchAttribute()
      };
    }
    public bool IsGeo => Value.Type.StartsWith("Edm.Geography") || Value.Type.StartsWith("Edm.Geometry");
    public bool NeedReference => Value is NavigationProperty;
  }
  public class Model : StructuredType
  {
    public Entity Entity { get; private set; }

    public Model(Models.StructuredType type, Entity entity, ApiOptions options) : base(type, options)
    {
      Entity = entity;
    }
    public Collection Collection { get; private set; }

    public void SetCollection(Collection collection)
    {
      Collection = collection;
    }
    public override string FileName => EdmStructuredType.Name.Dasherize() + ".model";
    public override string Name => Utils.ToTypescriptName(EdmStructuredType.Name, TypeScriptElement.Class) + "Model";
    public override IEnumerable<string> ImportTypes
    {
      get
      {
        var list = new List<string> {
                    EdmStructuredType.NamespaceQualifiedName
                };
        list.AddRange(EdmStructuredType.Properties.Select(a => a.Type));
        if (EdmEntityType != null)
        {
          list.AddRange(EdmEntityType.Properties.Select(a => a.Type));
          list.AddRange(EdmEntityType.NavigationProperties.Select(a => a.Type));
          list.AddRange(EdmEntityType.NavigationProperties.Select(a => a.ToEntityType));
          list.AddRange(EdmEntityType.Actions.SelectMany(a => CallableNamespaces(a)));
          list.AddRange(EdmEntityType.Functions.SelectMany(a => CallableNamespaces(a)));
        }
        var service = Program.Metadata.EntitySets.FirstOrDefault(s => EdmStructuredType.IsTypeOf(s.EntityType));
        if (service != null)
        {
          list.AddRange(service.NavigationPropertyBindings.Select(b => b.NavigationProperty.Type));
          list.AddRange(service.NavigationPropertyBindings.Select(b => b.PropertyType).Where(t => t != null).Select(t => t.NamespaceQualifiedName));
        }

        return list.Where(t => !String.IsNullOrWhiteSpace(t) && !t.StartsWith("Edm.")).Distinct();
      }
    }
    public IEnumerable<ModelField> Fields
    {
      get
      {
        var props = EdmStructuredType.Properties.ToList();
        if (EdmStructuredType is EntityType type)
          props.AddRange(type.NavigationProperties);
        return props.Select(prop => new ModelField(prop, this));
      }
    }
    public IEnumerable<string> Actions
    {
      get
      {
        if (EdmEntityType != null)
        {
          var modelActions = EdmEntityType.Actions.Where(a => !a.IsCollection).ToArray();
          return modelActions.Count() > 0 ? RenderCallables(modelActions) : [];
        }
        return [];
      }
    }
    public IEnumerable<string> Functions
    {
      get
      {
        if (EdmEntityType != null)
        {
          var modelFunctions = EdmEntityType.Functions.Where(a => !a.IsCollection).ToArray();
          return modelFunctions.Count() > 0 ? RenderCallables(modelFunctions) : [];
        }
        return [];
      }
    }
    public IEnumerable<string> Navigations
    {
      get
      {
        var service = Program.Metadata.EntitySets.FirstOrDefault(s => EdmStructuredType.IsTypeOf(s.EntityType));
        if (service != null)
        {
          var properties = new List<NavigationProperty>();
          var entity = EdmEntityType;
          while (true)
          {
            properties.AddRange(entity.NavigationProperties);
            if (String.IsNullOrEmpty(entity.BaseType))
              break;
            entity = Program.Metadata.FindEntityType(entity.BaseType);
          }
          var bindings = service.NavigationPropertyBindings
              .Where(binding => properties.All(n => n.Name != binding.NavigationProperty.Name));
          return RenderNavigationPropertyBindings(bindings);
        }
        return [];
      }
    }
    public IEnumerable<ModelField> GeoFields => Fields.Where(p => p.IsGeo);
    public IEnumerable<ModelField> SetterFields => Fields.Where(p => p.NeedReference);
    public IEnumerable<ModelField> GetterFields => Fields.Where(p => p.NeedReference);
    public bool HasGeoFields => Options.GeoJson && GeoFields.Count() > 0;
    public override object ToLiquid()
    {
      return new
      {
        Name = ImportedName,
        Entity = new
        {
          Name = Entity.ImportedName
        }
      };
    }
  }
}