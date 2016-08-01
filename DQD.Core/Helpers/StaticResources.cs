using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQD.Core.Helpers {
    static public class StaticResources {
        static public Dictionary<string,Uri> UriMaps = new Dictionary<string,Uri> {
            {"MatchPItem",new Uri("MatchPItem.uri")},
        };

        static public Uri GetUri(string str) { return UriMaps.ContainsKey(str) ? UriMaps[str] : null; }
    }
}
