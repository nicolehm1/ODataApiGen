using ODataApiGen.Abstracts;

namespace ODataApiGen.Angular
{
    public class Index : AngularRenderable
    {
        public Package Package {get; private set;}
        public Index(Package package, ApiOptions options) : base(options)
        {
            Package = package;
        }
        // Imports
        public override IEnumerable<string> ImportTypes => [];
        // Exports
        public override IEnumerable<Import> Imports => GetImportRecords();
        public override string Name => Package.Name;
        public override string FileName => "index";
        public override string Directory => "";
        public IEnumerable<string> Exports =>GetImportRecords().Select(import => $"export * from './{import.From}'");
    }
}
