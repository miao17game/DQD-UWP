using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace DQD.Core. Tools {
    public static class DataProcess {
        public static Uri ConvertToUri ( string str ) {
            if ( !string . IsNullOrEmpty ( str ) )
                return new Uri ( str );
            else
                return null;
        }
    }
}
