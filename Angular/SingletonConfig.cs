using System.Text.Json;
using DotLiquid;
using ODataApiGen.Abstracts;

namespace ODataApiGen.Angular
{
    public class SingletonConfig : AngularRenderable, ILiquidizable
  {
    public ServiceSingleton Service { get; private set; }
    public SingletonConfig(ServiceSingleton service, ApiOptions options) : base(options)
    {
      Service = service;
      AddDependency(service);
    }
    public override string FileName => Service.FileName + ".config";
    public override string Name => Service.Name + "SingletonConfig";
    public bool HasAnnotations => Service.Annotations.Count() > 0;
    public string Annotations => JsonSerializer.Serialize(Service.Annotations.Select(annot => annot.ToDictionary()), new JsonSerializerOptions { WriteIndented = true });
    public string SingletonName => Service.SingletonName;
    public string SingletonType => Service.SingletonType;
    // Imports
    public override IEnumerable<string> ImportTypes => new List<string>();
    public override IEnumerable<Import> Imports => GetImportRecords();
    public override string Directory => Service.EdmNamespace.Replace('.', Path.DirectorySeparatorChar);
    public object ToLiquid()
    {
      return new
      {
        SingletonName,
        SingletonType,
        Name = ImportedName,
        Service = new
        {
            Service.Name,
        }
      };
    }
  }
}