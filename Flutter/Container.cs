using System.Text.Json;
using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class Container : FlutterRenderable, ILiquidizable
  {
    public EntityContainer EdmEntityContainer { get; private set; }
    public ServiceContainer Service { get; private set; }
    public ICollection<Service> Services { get; } = new List<Service>();
    public ICollection<EntitySetConfig> EntitySetConfigs { get; } = new List<EntitySetConfig>();
    public Container(EntityContainer container, ApiOptions options) : base(options)
    {
      EdmEntityContainer = container;
      Service = new ServiceContainer(this, options);
      foreach (var eset in container.EntitySets)
      {
        Service service = new ServiceEntitySet(eset, options);
        Services.Add(service);
        var config = new EntitySetConfig(service, options);
        EntitySetConfigs.Add(config);
      }
      foreach (var s in container.Singletons)
      {
        var service = new ServiceSingleton(s, options);
        Services.Add(service);
        var config = new EntitySetConfig(service, options);
        EntitySetConfigs.Add(config);
      }
    }
    public bool HasAnnotations => EdmEntityContainer.Annotations.Count > 0;
    public string Annotations => JsonSerializer.Serialize(EdmEntityContainer.Annotations.Select(annot => annot.ToDictionary()), new JsonSerializerOptions { WriteIndented = true });
    public override string FileName => EdmEntityContainer.Name.Dasherize() + ".container";
    public override string Name => Utils.ToDartName(EdmEntityContainer.Name, DartElement.Class) + "Container";
    public string ContainerType => EdmEntityContainer.NamespaceQualifiedName;
    public string ContainerName => EdmEntityContainer.Name;
    public string ApiName => Options.Name;
    // Imports
    public override IEnumerable<string> ImportTypes => new List<string> { ContainerType };
    public override IEnumerable<Import> Imports => GetImportRecords();
    public override string Directory => EdmEntityContainer.Namespace.Replace('.', Path.DirectorySeparatorChar);
    public void ResolveDependencies(IEnumerable<Enum> enums, IEnumerable<Entity> entities, IEnumerable<Model> models, IEnumerable<Collection> collections)
    {
      // Services
      foreach (var service in Services)
      {
        var inter = entities.FirstOrDefault(m => m.EdmStructuredType.IsTypeOf(service.EntityType));
        if (inter != null)
        {
          service.SetEntity(inter);
          //service.AddDependency(inter);
        }
        var model = models.FirstOrDefault(m => m.EdmStructuredType.NamespaceQualifiedName == service.EntityType);
        if (model != null)
        {
          service.SetModel(model);
          //service.AddDependency(model);
        }
        var collection = collections.FirstOrDefault(m => m.EdmStructuredType.NamespaceQualifiedName == service.EntityType);
        if (collection != null)
        {
          service.SetCollection(collection);
          //service.AddDependency(collection);
        }
      }

      AddDependencies(EntitySetConfigs);
    }
    public IEnumerable<string> GetAllDirectories()
    {
      return new[] { Service.Directory }
          .Union(Services.Select(s => s.Directory))
          .Union(EntitySetConfigs.Select(s => s.Directory));
    }
    public IEnumerable<Renderable> Renderables
    {
      get
      {
        var renderables = new List<Renderable>();
        renderables.Add(Service);
        renderables.AddRange(Services);
        renderables.AddRange(EntitySetConfigs);
        return renderables;
      }
    }
    public object ToLiquid()
    {
      return new
      {
        Name = ImportedName,
        ContainerName,
        ContainerType
      };
    }
  }
}