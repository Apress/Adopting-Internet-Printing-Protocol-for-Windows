using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    public static class IppHelpers
    {
        public static bool IsRequestSuccessful(int status)
        {
            return status < (int)CsIppRequestLib.Status.IPP_STATUS.IPP_STATUS_OK_EVENTS_COMPLETE;
        }
    }
}
