using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    /// <summary>
    /// CompletionStruct
    /// </summary>
    public sealed class CompletionStruct
    {
        public int status { get; set; }
        public int jobId { get; set; } = -1;
        public int? JobStateEnum { get; set; }
        public string JobStateText { get; set; }
        public List<string> JobStateReasons { get; } = new List<string>();
    }
}
