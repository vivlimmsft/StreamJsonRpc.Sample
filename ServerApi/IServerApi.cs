using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApi
{
    internal interface IServerApi: IDisposable
    {
        public Task<INestedApi> GetNestedApiAsync(string argument, CancellationToken cancellation);

        public Task<string> TopLevelApiThatJustUppercasesAStringAsync(string argument, CancellationToken cancellation);

        // I found that passing a marshaled proxy back to the server is not allowed: `NotSupportedException: Marshaling a proxy back to its owner`
        // Leaving it here but commented out as an example of something you can't do.
        //public Task NestedApiMutatorAsync(INestedApi nest, CancellationToken cancellation);
    }
}
