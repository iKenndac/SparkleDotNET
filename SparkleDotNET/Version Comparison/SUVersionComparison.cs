using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkleDotNET {
    public interface SUVersionComparison {
         int CompareVersionToVersion(string version1, string version2);
    }
}
