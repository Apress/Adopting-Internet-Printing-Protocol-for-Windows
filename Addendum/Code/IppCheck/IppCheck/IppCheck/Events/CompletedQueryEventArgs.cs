using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IppCheck
{
    public class CompletedQueryEventArgs
    {
        private bool bResult;

        public CompletedQueryEventArgs(bool result) 
        { 
            Result = result;
        }

        public bool Result 
        { 
            get => bResult; 
            set => bResult = value; 
        }
    }
}
