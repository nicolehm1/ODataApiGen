using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class ServiceEntitySet : Service
  {
    public EntitySet EdmEntitySet { get; private set; }
    public ServiceEntitySet(EntitySet type, ApiOptions options) : base(options)
    {
      EdmEntitySet = type;
    }
    public override IEnumerable<string> ImportTypes
    {
      get
      {
        var parameters = new List<Parameter>();
        foreach (var cal in EdmEntitySet.Actions)
          parameters.AddRange(cal.Parameters);
        foreach (var cal in EdmEntitySet.Functions)
          parameters.AddRange(cal.Parameters);

        var list = new List<string> {
                    EdmEntitySet.EntityType
                };
        list.AddRange(parameters.Select(p => p.Type));
        list.AddRange(EdmEntitySet.Actions.SelectMany(a => CallableNamespaces(a)));
        list.AddRange(EdmEntitySet.Functions.SelectMany(a => CallableNamespaces(a)));
        list.AddRange(EdmEntitySet.NavigationPropertyBindings.Select(b => b.NavigationProperty.Type));
        list.AddRange(EdmEntitySet.NavigationPropertyBindings.Select(b => b.PropertyType).Where(t => t != null).Select(t => t.NamespaceQualifiedName));
        if (EdmEntityType != null)
        {
          list.AddRange(EdmEntityType.Actions.SelectMany(a => CallableNamespaces(a)));
          list.AddRange(EdmEntityType.Functions.SelectMany(a => CallableNamespaces(a)));
        }
        if (HasEntity)
        {
          list.AddRange(Entity.EdmStructuredType.Properties.Select(a => a.Type));
        }
        if (HasModel)
        {
          list.AddRange(Model.EdmStructuredType.Properties.Select(a => a.Type));
        }
        return list.Where(t => !String.IsNullOrWhiteSpace(t) && !t.StartsWith("Edm.")).Distinct();
      }
    }

    public override IEnumerable<Import> Imports => GetImportRecords();
    public override string EntitySetName => EdmEntitySet.Name;
    public override string EntityType => EdmEntitySet.EntityType;
    public string ServiceType => EdmEntitySet.NamespaceQualifiedName;
    public override string Name => Utils.ToDartName(EdmEntitySet.Name, DartElement.Class) + "Service";
    public override string EdmNamespace => EdmEntitySet.Namespace;
    public override string FileName => EdmEntitySet.Name.Dasherize() + ".service";
    public IEnumerable<string> Actions => RenderCallables(EdmEntitySet.Actions.Union(EdmEntityType.Actions));
    public IEnumerable<string> Functions => RenderCallables(EdmEntitySet.Functions.Union(EdmEntityType.Functions));
    public IEnumerable<string> Navigations => RenderNavigationPropertyBindings(EdmEntitySet.NavigationPropertyBindings);
    public override IEnumerable<Annotation> Annotations => EdmEntitySet.Annotations;
  }
}
