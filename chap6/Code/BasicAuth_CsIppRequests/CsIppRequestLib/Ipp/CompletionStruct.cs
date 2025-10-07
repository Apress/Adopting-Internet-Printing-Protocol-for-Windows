using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CsIppRequestLib
{
    public struct CompletionStruct
    {
        public CompletionStruct(int _status, int _jobId)
        {
            status  = _status;
            jobId = _jobId;
        }

        public int status { get; set; }
        public int jobId { get; set; }
    }

}
