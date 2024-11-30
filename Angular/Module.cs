using ODataApiGen.Abstracts;

namespace ODataApiGen.Angular
{
    public class Module : AngularRenderable
    {
        public Package Package {get; private set;}
        public Module(Package package, ApiOptions options) : base(options)
        {
            Package = package;
        }
        public override string Name => Package.Name + "Module";
        public override string FileName => Package.Name.Dasherize() + ".module";
        public override string Directory => "";
        public IEnumerable<Service> Services => Package.Schemas.SelectMany(s => s.Containers.Select(c => c.Service))
        .Union(Package.Schemas.SelectMany(s => s.Containers.SelectMany(c => c.Services)));
        // Imports and Exports
        public override IEnumerable<string> ImportTypes => Package.Schemas.SelectMany(s => s.Containers.SelectMany(c => c.Services)).Select(a => a.ServiceType);
        public override IEnumerable<Import> Imports => GetImportRecords();
    }
}
