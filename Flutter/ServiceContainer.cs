using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Flutter
{
    public class ServiceContainer : Service 
    {
        public Container Container {get; private set;}
        public ServiceContainer(Container container, ApiOptions options) : base(options)
        {
            Container = container;
        }
        public override IEnumerable<string> ImportTypes
        {
            get
            {
                var parameters = new List<Parameter>();
                foreach (var cal in Container.EdmEntityContainer.UnboundActions)
                    parameters.AddRange(cal.Parameters);
                foreach (var cal in Container.EdmEntityContainer.UnboundFunctions)
                    parameters.AddRange(cal.Parameters);

                var list = new List<string>();
                list.AddRange(parameters.Select(p => p.Type));
                list.AddRange(Container.EdmEntityContainer.UnboundActions.SelectMany(a => CallableNamespaces(a)));
                list.AddRange(Container.EdmEntityContainer.UnboundFunctions.SelectMany(f => CallableNamespaces(f)));
                return list.Where(t => !String.IsNullOrWhiteSpace(t) && !t.StartsWith("Edm.")).Distinct();
            }
        }

        public override IEnumerable<Import> Imports => GetImportRecords();

        public override string Name => Utils.ToDartName(Container.EdmEntityContainer.Name, DartElement.Class) + "Service";
        public override string EdmNamespace => Container.EdmEntityContainer.Namespace;
        public override string FileName => Container.EdmEntityContainer.Name.Dasherize() + ".service";
        public IEnumerable<string> Actions =>  RenderCallables(Container.EdmEntityContainer.UnboundActions);
        public IEnumerable<string> Functions => RenderCallables(Container.EdmEntityContainer.UnboundFunctions);
        public override string Directory => EdmNamespace.Replace('.', Path.DirectorySeparatorChar);
        public override IEnumerable<Annotation> Annotations => []; 
        public override string EntitySetName => Container.Name;
        public override string EntityType => "";
        public string ApiName => Options.Name;
    }
}