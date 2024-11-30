using DotLiquid;
using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public class Enum : AngularRenderable, ILiquidizable
    {
        public EnumType EdmEnumType { get; private set; }
        public Enum(EnumType type, ApiOptions options) : base(options)
        {
            EdmEnumType = type;
        }
        // Imports
        public override IEnumerable<string> ImportTypes => [];
        public override IEnumerable<Import> Imports => GetImportRecords();
        public override string Name => Utils.ToTypescriptName(EdmEnumType.Name, TypeScriptElement.Enum);
        public override string FileName => EdmEnumType.Name.Dasherize() + ".enum";
        public override string Directory => EdmEnumType.Namespace.Replace('.', Path.DirectorySeparatorChar);
        public string FullName => EdmEnumType.NamespaceQualifiedName;
        public string TypeName => Name + "EnumType";
        public IEnumerable<string> Members => EdmEnumType.Members.Select(m => $"{m.Name} = {m.Value}");
        public bool Flags => EdmEnumType.Flags;
        public object ToLiquid()
        {
            return new
            {
                Name = ImportedName,
            };
        }
    }
}