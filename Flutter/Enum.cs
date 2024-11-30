using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class Enum : FlutterRenderable, ILiquidizable {
        public EnumType EdmEnumType {get; private set;}
        public Enum(EnumType type, ApiOptions options) : base(options) {
            EdmEnumType = type;
        }
        // Imports
        public override IEnumerable<string> ImportTypes => [];
        // Exports
        public override IEnumerable<Import> Imports => GetImportRecords();
        public override string Name => Utils.ToDartName(EdmEnumType.Name, DartElement.Enum);
        public string EnumType => EdmEnumType.NamespaceQualifiedName;
        public override string FileName => EdmEnumType.Name.Dasherize() + ".enum";
        public override string Directory => EdmEnumType.Namespace.Replace('.', Path.DirectorySeparatorChar);
        public IEnumerable<string> Members => EdmEnumType.Members.Select(m => $"{m.Name} = {m.Value}");
        public bool Flags => EdmEnumType.Flags;
        public object ToLiquid()
        {
            return new { 
                Name = ImportedName, EnumType
            };
        }
    }
}