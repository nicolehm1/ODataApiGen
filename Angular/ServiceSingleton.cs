using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public class ServiceSingleton : Service
  {
    public Singleton EdmSingleton { get; private set; }

    public ServiceSingleton(Singleton type, ApiOptions options) : base(options)
    {
      EdmSingleton = type;
    }
    public override string Name => Utils.ToTypescriptName(EdmSingleton.Name, TypeScriptElement.Class) + "Service";
    public override string EdmNamespace => EdmSingleton.Namespace;
    public override string FileName => EdmSingleton.Name.Dasherize() + ".service";
    // Imports
    public override IEnumerable<Import> Imports => GetImportRecords();
    public override IEnumerable<string> ImportTypes
    {
      get
      {
        var parameters = new List<Parameter>();
        foreach (var cal in EdmSingleton.Actions)
          parameters.AddRange(cal.Parameters);
        foreach (var cal in EdmSingleton.Functions)
          parameters.AddRange(cal.Parameters);

        var list = new List<string> {
                    EdmSingleton.Type
                };
        list.AddRange(parameters.Select(p => p.Type));
        list.AddRange(EdmSingleton.Actions.SelectMany(a => CallableNamespaces(a)));
        list.AddRange(EdmSingleton.Functions.SelectMany(a => CallableNamespaces(a)));
        if (Entity != null)
        {
          list.AddRange(Entity.EdmStructuredType.Properties.Select(a => a.Type));
          if (Entity.EdmStructuredType is EntityType type)
            list.AddRange(type.NavigationProperties.Select(a => a.Type));
        }
        if (Model != null)
        {
          list.AddRange(Model.EdmStructuredType.Properties.Select(a => a.Type));
          if (Model.EdmStructuredType is EntityType type)
            list.AddRange(type.NavigationProperties.Select(a => a.Type));
        }
        return list;
      }
    }

    public string SingletonName => EdmSingleton.Name;
    public string SingletonType => EdmSingleton.Type;
    public override string EntityType => EdmSingleton.Type;
    public override IEnumerable<Annotation> Annotations => EdmSingleton.Annotations;
    public override string ServiceType => EdmSingleton.NamespaceQualifiedName;
    public IEnumerable<string> Actions => RenderCallables(EdmSingleton.Actions);
    public IEnumerable<string> Functions => RenderCallables(EdmSingleton.Functions);
  }
}