using System.Text.Json;
using DotLiquid;
using ODataApiGen.Abstracts;

namespace ODataApiGen.Flutter
{
    public class EntitySetConfig : FlutterRenderable, ILiquidizable
  {
    public Service Service { get; private set; }
    public EntitySetConfig(Service service, ApiOptions options) : base(options)
    {
      Service = service;
      AddDependency(service);
    }
    public override string FileName => Service.FileName + ".config";
    public override string Name => Service.Name + "EntitySetConfig";
    public bool HasAnnotations => Service.Annotations.Count() > 0;
    public string Annotations => JsonSerializer.Serialize(Service.Annotations.Select(annot => annot.ToDictionary()), new JsonSerializerOptions { WriteIndented = true });
    public string EntitySetName => Service.EntitySetName;
    public string EntityType => Service.EntityType;
    // Imports
    public override IEnumerable<string> ImportTypes => new List<string>();
    public override IEnumerable<Import> Imports => GetImportRecords();
    public override string Directory => Service.EdmNamespace.Replace('.', Path.DirectorySeparatorChar);
    public object ToLiquid()
    {
      return new
      {
        Name = ImportedName,
        EntitySetName,
        EntityType,
        Service = new
        {
            Service.Name,
        }
      };
    }
  }
}