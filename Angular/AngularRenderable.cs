using ODataApiGen.Abstracts;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public abstract class AngularRenderable : Renderable
    {
        public AngularRenderable(ApiOptions options) : base(options) {}

        public override string FileExtension => ".ts";
        public string _ToTypescriptName(string name, TypeScriptElement e) {
            return Utils.ToTypescriptName(name, e);
        }
        public string ToTypescriptType(string type)
        {

            return Utils.ToTypescriptType(type, Options.GeoJson);
        }
        public IEnumerable<string> CallableNamespaces(Callable callable)
        {
            var uriList = new List<string>();
            if (!string.IsNullOrWhiteSpace(callable.ReturnType) && !callable.IsEdmReturnType)
            {
                uriList.Add(callable.ReturnType);
            }
            if (!string.IsNullOrWhiteSpace(callable.BindingParameter.Type))
            {
                uriList.Add(callable.BindingParameter.Type);
            }
            foreach (var param in callable.Parameters)
            {
                uriList.Add(param.Type);
            }
            return uriList;
        }
        public IEnumerable<string> RenderImports()
        {
            return GetImportRecords().Select(import =>
            {
                var path = import.From.ToString();
                if (!path.StartsWith("../"))
                    path = $"./{path}";
                return $"import {{ {String.Join(", ", import.Names)} }} from '{path}';";
            });
        }
        public abstract IEnumerable<Import> Imports { get; }
        protected IEnumerable<Import> GetImportRecords()
        {
            var records = Dependencies
                .Where(a => a.Item2.Uri != Uri)
                .GroupBy(i => i.Item2.Uri).Select(group =>
            {
                var uri = Uri.MakeRelativeUri(group.First().Item2.Uri);
                var names = group.Select(d =>
                {
                    if (d.Item1 != d.Item2.Name) {
                        d.Item2.ImportedName = d.Item1;
                        return $"{d.Item2.Name} as {d.Item1}";
                    }

                    d.Item2.ImportedName = d.Item2.Name;
                    return d.Item1;
                }).Distinct();
                return new Import(names, uri);
            });
            return records;
        }
    }
}