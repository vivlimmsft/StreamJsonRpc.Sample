using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace ServerApi
{
    [RpcMarshalable]
    internal interface INestedApi : IDisposable
    {
        public Task<string> ConcatenateWithConstructedArgumentAsync(string argument, CancellationToken cancellationToken);
    }
}
