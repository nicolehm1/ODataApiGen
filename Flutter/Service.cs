using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public abstract class Service : FlutterRenderable, ILiquidizable
  {
    public EntityType EdmEntityType => HasModel ? Model.EdmEntityType :
        HasEntity ? Entity.EdmEntityType :
        null;
    public Service(ApiOptions options) : base(options) { }

    public Entity Entity { get; private set; }
    public void SetEntity(Entity entity)
    {
      Entity = entity;
    }
    public bool HasEntity => Entity != null;
    public string EntityName => HasEntity ? Entity.ImportedName : String.Empty;
    public Model Model { get; private set; }
    public void SetModel(Model model)
    {
      Model = model;
    }
    public bool HasModel => Model != null;
    public string ModelName => HasModel ? Model.ImportedName : String.Empty;
    public Collection Collection { get; private set; }
    public void SetCollection(Collection collection)
    {
      Collection = collection;
    }
    public bool HasCollection => Collection != null;
    public string CollectionName => HasCollection ? Collection.ImportedName : String.Empty;
    public abstract string EntitySetName { get; }
    public abstract string EntityType { get; }
    public abstract string EdmNamespace { get; }
    public abstract IEnumerable<Annotation> Annotations { get; }
    public override string Directory => EdmNamespace.Replace('.', Path.DirectorySeparatorChar);
    protected IEnumerable<string> RenderCallables(IEnumerable<Callable> allCallables)
    {
      var names = allCallables.GroupBy(c => c.Name).Select(c => c.Key);
      foreach (var name in names)
      {
        var callables = allCallables.Where(c => c.Name == name);
        var overload = callables.Count() > 1;
        var callable = callables.FirstOrDefault();
        var methodName = name[0].ToString().ToLower() + name.Substring(1);
        var callMethodName = "call" + name[0].ToString().ToUpper() + name.Substring(1);

        var callableNamespaceQualifiedName = $"{callable.Namespace}.{callable.Name}";

        var typescriptType = ToTypescriptType(callable.ReturnType);
        if ((callable.IsEdmReturnType || callable.IsEnumReturnType) && callable.ReturnsCollection) 
          typescriptType += "[]";

        var callableReturnType = String.IsNullOrEmpty(callable.ReturnType) ?
            "" :
        callable.IsEdmReturnType ?
            $" as Observable<ODataProperty<{typescriptType}>>" :
        callable.IsEnumReturnType ?
            $" as Observable<ODataProperty<{typescriptType}>>" :
        callable.ReturnsCollection ?
            $" as Observable<ODataEntities<{typescriptType}>>" :
            $" as Observable<ODataEntity<{typescriptType}>>";

        var parameters = new List<Parameter>();
        var optionals = new List<string>();
        foreach (var cal in callables)
        {
          foreach (var param in cal.Parameters)
          {
            if (parameters.All(p => p.Name != param.Name))
              parameters.Add(param);
            if (optionals.All(o => o != param.Name) && !callables.All(c => c.Parameters.Any(p => p.Name == param.Name)))
              optionals.Add(param.Name);
            if (param.HasAnnotation("Org.OData.Core.V1.OptionalParameter")) 
              optionals.Add(param.Name);
          }
        }
        parameters = parameters.Where(p => !p.IsBinding).GroupBy(p => p.Name).Select(g => g.First()).ToList();

        var baseMethodName = !callable.IsBound ?
            $"client.{callable.Type.ToLower()}" :
            callable.IsCollection
            ? $"entities().{callable.Type.ToLower()}"
            : $"entity(key).{callable.Type.ToLower()}";

        var boundArgs = new List<string>();
        if (callable.IsBound && !callable.IsCollection)
          boundArgs.Add($"key: EntityKey<{EntityName}>");

        var args = new List<string>(boundArgs);

        var arguments = parameters
          .Where(p => !optionals.Contains(p.Name))
          .Union(parameters.Where(p => optionals.Contains(p.Name)))
          .Select(p =>
            $"{p.Name}" +
            (optionals.Any(o => o == p.Name) ? "?" : "") +
            $": {ToTypescriptType(p.Type)}" +
            (p.IsCollection ? "[]" : ""));

        args.AddRange(arguments);
        if (callable.IsEdmReturnType || callable.IsEnumReturnType) {
          args.Add("options?: ODataOptions & {alias?: boolean}");
        } else if (callable.Type == "Function") {
          args.Add($"options?: ODataFunctionOptions<{typescriptType}>");
        } else {
          args.Add($"options?: ODataActionOptions<{typescriptType}>");
        }

        var type = "null";
        if (parameters.Count > 0)
        {
          type = $"{{{String.Join(", ", arguments)}}}";
        }

        var values = "null";
        if (parameters.Count > 0)
        {
          values = $"{{{String.Join(", ", parameters.Select(p => p.Name))}}}";
        }

        var responseType = String.IsNullOrEmpty(callable.ReturnType) ?
            "none" :
        callable.IsEdmReturnType ?
            "property" :
        callable.IsEnumReturnType ?
            "property" :
        callable.ReturnsCollection ?
            "entities" :
            "entity";

        // Function
        yield return $"public {methodName}({String.Join(", ", boundArgs)}): OData{callable.Type}Resource<{type}, {typescriptType}> {{ " +
            $"\n    return this.{baseMethodName}<{type}, {typescriptType}>('{callableNamespaceQualifiedName}'{(!callable.IsBound ? ", this.apiNameOrEntityType" : "")});" +
            "\n  }";

        // Call
        yield return $"public {callMethodName}({String.Join(", ", args)}) {{" +
            $"\n    return this.call{callable.Type}<{type}, {typescriptType}>(" +
            $"\n      {values}, " +
            $"\n      this.{methodName}({(callable.IsBound && !callable.IsCollection ? "key" : "")}), " +
            $"\n      '{responseType}', options){callableReturnType};" +
             "\n  }";
      }
    }

    protected IEnumerable<string> RenderNavigationPropertyBindings(IEnumerable<NavigationPropertyBinding> bindings)
    {
      var casts = new List<string>();
      foreach (var binding in bindings)
      {
        var isCollection = binding.NavigationProperty.IsCollection;
        var nav = binding.NavigationProperty;
        var navEntity = nav.EntityType;
        var bindingEntity = binding.EntityType;
        var propertyEntity = binding.PropertyType;

        var entity = (Program.Package as Package).FindEntity(navEntity.NamespaceQualifiedName);
        var returnType = isCollection ? $"ODataEntities<{entity.ImportedName}>" : $"ODataEntity<{entity.ImportedName}>";
        var responseType = isCollection ? "entities" : "entity";
        if (propertyEntity != null && bindingEntity.IsBaseOf(propertyEntity))
        {
          var castName = $"as{propertyEntity.Name}";
          if (!casts.Contains(propertyEntity.NamespaceQualifiedName))
          {
            // Cast
            entity = (Program.Package as Package).FindEntity(propertyEntity.NamespaceQualifiedName);
            yield return $@"public {castName}(): ODataEntitySetResource<{entity.ImportedName}> {{
    return this.entities().cast<{entity.ImportedName}>('{propertyEntity.NamespaceQualifiedName}');
  }}";
            casts.Add(propertyEntity.NamespaceQualifiedName);
          }
          entity = (Program.Package as Package).FindEntity(navEntity.NamespaceQualifiedName);

          var navMethodName = castName + nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1);
          var fetchMethodName = castName + "Fetch" + nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1);
          var createMethodName = isCollection ? 
            castName + $"Add{entity.Name}To{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}" : 
            castName + $"Set{entity.Name}As{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}";
          var deleteMethodName = isCollection ? 
            castName + $"Remove{entity.Name}From{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}" : 
            castName + $"Unset{entity.Name}As{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}";

          // Navigation
          yield return $"public {navMethodName}(key: EntityKey<{EntityName}>): ODataNavigationPropertyResource<{entity.ImportedName}> {{ " +
              $"\n    return this.{castName}().entity(key).navigationProperty<{entity.ImportedName}>('{binding.PropertyName}'); " +
              "\n  }";

          // Fetch
          yield return $"public {fetchMethodName}(key: EntityKey<{EntityName}>, options?: ODataQueryArgumentsOptions<{entity.ImportedName}>) {{" +
              $"\n    return this.fetchNavigationProperty<{entity.ImportedName}>(" +
              $"\n      this.{navMethodName}(key), " +
              $"\n      '{responseType}', options) as Observable<{returnType}>;" +
               "\n  }";

          // Link
          yield return $"public {createMethodName}(key: EntityKey<{EntityName}>, target: ODataEntityResource<{returnType}>, {{etag}}: {{etag?: string}} = {{}}): Observable<any> {{" +
              $"\n    return this.{navMethodName}(key).reference()" +
              $"\n      .{(isCollection ? "add" : "set")}(target{(isCollection ? "" : ", {etag}")});" +
               "\n  }";

          // Unlink
          yield return $@"public {deleteMethodName}(key: EntityKey<{EntityName}>, {{target, etag}}: {{target?: ODataEntityResource<{returnType}>, etag?: string}} = {{}}): Observable<any> {{" +
          $"\n    return this.{navMethodName}(key).reference()" +
          $"\n      .{(isCollection ? "remove(target)" : "unset({etag})")};" +
           "\n  }";
        }
        else
        {
          var navMethodName = nav.Name.Substring(0, 1).ToLower() + nav.Name.Substring(1);
          var fetchMethodName = "fetch" + nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1);
          var createMethodName = isCollection ? 
            $"add{entity.Name}To{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}" : 
            $"set{entity.Name}As{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}";
          var deleteMethodName = isCollection ? 
            $"remove{entity.Name}From{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}" : 
            $"unset{entity.Name}As{nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1)}";

          // Navigation
          yield return $"public {navMethodName}(key: EntityKey<{EntityName}>): ODataNavigationPropertyResource<{entity.ImportedName}> {{ " +
              $"\n    return this.entity(key).navigationProperty<{entity.ImportedName}>('{binding.PropertyName}'); " +
              "\n  }";

          // Fetch
          yield return $"public {fetchMethodName}(key: EntityKey<{EntityName}>, options?: ODataQueryArgumentsOptions<{entity.ImportedName}>) {{" +
              $"\n    return this.fetchNavigationProperty<{entity.ImportedName}>(" +
              $"\n      this.{navMethodName}(key), " +
              $"\n      '{responseType}', options) as Observable<{returnType}>;" +
               "\n  }";

          // Link
          yield return $"public {createMethodName}(key: EntityKey<{EntityName}>, target: ODataEntityResource<{returnType}>, {{etag}}: {{etag?: string}} = {{}}): Observable<any> {{" +
              $"\n    return this.{navMethodName}(key).reference()" +
              $"\n      .{(isCollection ? "add" : "set")}(target{(isCollection ? "" : ", {etag}")});" +
               "\n  }";

          // Unlink
          yield return $@"public {deleteMethodName}(key: EntityKey<{EntityName}>, {{target, etag}}: {{target?: ODataEntityResource<{returnType}>, etag?: string}} = {{}}): Observable<any> {{" +
          $"\n    return this.{navMethodName}(key).reference()" +
          $"\n      .{(isCollection ? "remove(target)" : "unset({etag})")};" +
           "\n  }";
        }
      }
    }
    public object ToLiquid()
    {
      return new
      {
        Name = ImportedName
      };
    }

  }
}