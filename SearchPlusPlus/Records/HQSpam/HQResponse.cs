using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSearch.Records
{
    public class HQResponse
    {
        public List<HQChartInfo> charts { get; set; } = null!;
        public int total { get; set; }
        public int page { get; set; }
        public int totalPages { get; set; }
    }
}
