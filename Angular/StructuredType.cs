using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public abstract class StructuredType : AngularRenderable, ILiquidizable
  {
    public EntityType? EdmEntityType => EdmStructuredType as EntityType;
    public Models.StructuredType EdmStructuredType { get; private set; }
    public StructuredType(Models.StructuredType type, ApiOptions options) : base(options)
    {
      EdmStructuredType = type;
    }

    public StructuredType? Base { get; private set; }
    public void SetBase(StructuredType b)
    {
      Base = b;
    }

    // Imports
    public override IEnumerable<string> ImportTypes
    {
      get
      {
        var list = new List<string>();
        if (EdmEntityType != null)
        {
          list.AddRange(EdmEntityType.Properties.Select(a => a.Type));
          list.AddRange(EdmEntityType.NavigationProperties.Select(a => a.Type));
          list.AddRange(EdmEntityType.NavigationProperties.Select(a => a.ToEntityType));
          list.AddRange(EdmEntityType.Actions.Select(a => a.Type));
          list.AddRange(EdmEntityType.Functions.Select(a => a.Type));
        }
        /*For Not-EDM types (e.g. enums with namespaces, complex types*/
        list.AddRange(EdmStructuredType.Properties
            .Where(a => !a.IsEdmType)
            .Select(a => a.Type));
        if (Base != null)
          list.Add(Base.EdmStructuredType.NamespaceQualifiedName);
        return list.Where(t => !String.IsNullOrWhiteSpace(t) && !t.StartsWith("Edm.")).Distinct();
      }
    }
    public override string Directory => EdmStructuredType.Namespace.Replace('.', Path.DirectorySeparatorChar);
    public bool OpenType => EdmStructuredType.OpenType;
    public override IEnumerable<Import> Imports => GetImportRecords();

    protected IEnumerable<string> RenderCallables(Callable[] allCallables)
    {
      var names = allCallables.GroupBy(c => c.Name).Select(c => c.Key);
      foreach (var name in names)
      {
        var callables = allCallables.Where(c => c.Name == name).ToList();
        var callable = callables.First();
        var methodName = name.Substring(0, 1).ToLower() + name.Substring(1);

        var callableNamespaceQualifiedName = callable.IsBound ? $"{callable.Namespace}.{callable.Name}" : callable.Name;

        var typescriptType = ToTypescriptType(callable.ReturnType);
        var callableReturnType = String.IsNullOrEmpty(callable.ReturnType) ?
            "" :
        callable.IsEdmReturnType ?
            $" as Observable<{typescriptType}>" :
        callable.IsEnumReturnType ?
            $" as Observable<{typescriptType}>" :
        callable.ReturnsCollection ?
            $" as Observable<{typescriptType}Collection<{typescriptType}, {typescriptType}Model<{typescriptType}>>>" :
            $" as Observable<{typescriptType}Model<{typescriptType}>>";

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

        var arguments = parameters
          .Where(p => !optionals.Contains(p.Name))
          .Union(parameters.Where(p => optionals.Contains(p.Name)))
          .Select(p =>
            $"{p.Name}" +
            (optionals.Any(o => o == p.Name) ? "?" : "") +
            $": {ToTypescriptType(p.Type)}" +
            (p.IsCollection ? "[]" : "")).ToList();

        var args = new List<string>(arguments);
        if (callable.IsEdmReturnType || callable.IsEnumReturnType) {
          args.Add("options?: ODataOptions & {alias?: boolean}");
        } else if (callable.Type == "Function") {
          args.Add($"options?: ODataFunctionOptions<{typescriptType}>");
        } else {
          args.Add($"options?: ODataActionOptions<{typescriptType}>");
        }

        var types = "null";
        if (parameters.Count > 0)
        {
          types = $"{{{String.Join(", ", arguments)}}}";
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
        callable.ReturnsCollection ?
            "collection" :
            "model";
        yield return $"public {methodName}({String.Join(", ", args)}) {{" +
            $"\n    return this.call{callable.Type}<{types}, {typescriptType}>('{callableNamespaceQualifiedName}', {values}, '{responseType}', options){callableReturnType};" +
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

        var entity = ((Package) Program.Package).FindEntity(navEntity.NamespaceQualifiedName);
        if (propertyEntity != null && bindingEntity.IsBaseOf(propertyEntity) && bindingEntity.HierarchyLevelOf(propertyEntity) == 1)
        {
          var castName = $"as{propertyEntity.Name}";
          if (!casts.Contains(propertyEntity.NamespaceQualifiedName))
          {
            // Cast
            entity = (Program.Package as Package).FindEntity(propertyEntity.NamespaceQualifiedName);
            yield return $@"public {castName}() {{
    return this.cast<{entity.ImportedName}, {entity.Name}Model<{entity.ImportedName}>>('{propertyEntity.NamespaceQualifiedName}');
  }}";
            casts.Add(propertyEntity.NamespaceQualifiedName);
          }
        }
        else
        {
          //TODO collection and model name
          var returnType = isCollection ? $"ODataCollection<{entity.ImportedName}, ODataModel<{entity.ImportedName}>>" : $"ODataModel<{entity.ImportedName}>";
          var responseType = isCollection ? "collection" : "model";
          var methodName = $"as{propertyEntity.Name}" + nav.Name.Substring(0, 1).ToUpper() + nav.Name.Substring(1);
          var castEntity = (Program.Package as Package).FindEntity(propertyEntity.NamespaceQualifiedName);

          // Navigation
          yield return $@"public {methodName}(options?: ODataQueryArgumentsOptions<{entity.ImportedName}>) {{
    return this.fetchNavigationProperty<{entity.ImportedName}>('{binding.Path}', '{responseType}', options) as Observable<{returnType}>;
  }}";
        }
      }
    }
    public abstract object ToLiquid();
  }
}