using ODataApiGen.Abstracts;

namespace ODataApiGen.Flutter
{
    public class ApiConfig : FlutterRenderable {
        public Package Package {get; private set;}
        public ApiConfig(Package package, ApiOptions options) : base(options){
            Package = package;
        }
        // Imports
        public override IEnumerable<string> ImportTypes => Package.Schemas.SelectMany(m => m.ImportTypes);
        // Exports
        public override IEnumerable<Import> Imports => GetImportRecords();
        public override string Name => Package.Name + "Config";
        // About File
        public override string FileName => Package.Name.Dasherize() + ".config";
        public override string Directory => "";
    }
}