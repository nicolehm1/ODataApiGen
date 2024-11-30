using System.Text.Json;
using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class EnumMemberConfig : ILiquidizable
  {
    protected EnumMember Value { get; set; }
    protected EnumTypeConfig Config { get; set; }
    public EnumMemberConfig(EnumMember member, EnumTypeConfig config)
    {
      Value = member;
      Config = config;
    }
    public string Name => Utils.IsValidTypeScrtiptName(Value.Name) ? Value.Name : $"\"{Value.Name}\"";

    public string Type
    {
      get
      {
        var values = new Dictionary<string, string>();
        values.Add("value", $"{Value.Value}");
        if (Name != Value.Name)
          values.Add("name", $"'{Value.Name}'");
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
          Name,
          Type
      };
    }
  }
  public class EnumTypeConfig : FlutterRenderable, ILiquidizable
  {
    public Enum Enum { get; private set; }
    public EnumTypeConfig(Enum enu, ApiOptions options) : base(options)
    {
      Enum = enu;
    }
    public override string FileName => Enum.FileName + ".config";
    public override string Name => Enum.Name + "Config";
    public string EnumType => Enum.EdmEnumType.NamespaceQualifiedName;
    public string EdmEnumName => Enum.EdmEnumType.Name;
    public string EnumName => Enum.Name;

    public bool HasAnnotations => Enum.EdmEnumType.Annotations.Count > 0;
    public string Annotations => JsonSerializer.Serialize(Enum.EdmEnumType.Annotations.Select(annot => annot.ToDictionary()), new JsonSerializerOptions { WriteIndented = true });
    public IEnumerable<EnumMemberConfig> Members
    {
      get
      {
        return Enum.EdmEnumType.Members.Select(member => new EnumMemberConfig(member, this));
      }
    }

    // Imports
    public override IEnumerable<string> ImportTypes => new List<string> { EnumType };
    public override IEnumerable<Import> Imports => GetImportRecords();
    public override string Directory => Enum.EdmEnumType.Namespace.Replace('.', Path.DirectorySeparatorChar);
    public bool Flags => Enum.EdmEnumType.Flags;

    public object ToLiquid()
    {
      return new
      {
        Name = ImportedName,
        EnumName
      };
    }
  }
}