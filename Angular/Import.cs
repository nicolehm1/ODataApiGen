using DotLiquid;

namespace ODataApiGen.Angular
{
    public class Import : ILiquidizable 
    {
        public IEnumerable<string> Names { get; set; }
        public Uri From { get; set; }
        public Import(IEnumerable<string> names, Uri from)
        {
            Names = names;
            From = from;
        }

        public string Path {
            get {
                var path = From.ToString();
                if (!path.StartsWith("../"))
                    path = $"./{path}";
                return path;
            }
        }

        public object ToLiquid()
        {
            return new {Path, Names};
        }
    }
}