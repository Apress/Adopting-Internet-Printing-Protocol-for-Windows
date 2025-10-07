using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public interface ICredentialsProvider
    {
        Task<(string Username, string Password, bool bCancel)> GetCredentialsAsync(bool bIsAuthenticated);
    }
}
