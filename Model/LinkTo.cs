using System.Linq;

namespace K.DB.Model
{
    public sealed class LinkTo : K.Core.Base.BaseModel
    {
        public int Id => K.Core.Security.Hash.Id(Display);
        public C.LinkTo.Type Type { get; set; }
        public string Value { get; set; }
        public K.Core.Model.Select[] Parameters { get; set; }
        public string Display { get; set; }
        public C.LinkTo.OpenAs OpenAs { get; set; }

        public LinkTo() { }

        public LinkTo(string tag)
        {
            var foo = Scripts.MyTags.LinkTo(tag);

            Type = foo.Type;
            Value = foo.Value;
            Parameters = foo.Parameters;
            Display = foo.Display;
            OpenAs = foo.OpenAs;
        }

        public dynamic[] GetParametersValues()
        {
            return Parameters.Select(t => t.Value).ToArray();
        }
    }
}

