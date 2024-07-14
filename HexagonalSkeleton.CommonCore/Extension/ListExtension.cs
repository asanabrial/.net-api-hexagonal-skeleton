using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexagonalSkeleton.CommonCore.Extension
{
    public static class ListExtension
    {
        public static bool HasElements<T>(this List<T>? list) => list != null && list.Count != 0;

    }
}
