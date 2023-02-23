using System;
using System.Threading;
using System.Threading.Tasks;
using ServerApi;

internal class Server: IServerApi
{
    public int Add(int a, int b)
    {
        // Log to STDERR so as to not corrupt the STDOUT pipe that we may be using for JSON-RPC.
        Console.Error.WriteLine($"Received request: {a} + {b}");

        return a + b;
    }

    public void Dispose()
    {
    }

    public Task<INestedApi> GetNestedApiAsync(string argument, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        return Task.FromResult(new NestedServer(argument) as INestedApi);
    }

    
    // I found that passing a marshaled proxy back to the server is not allowed: `NotSupportedException: Marshaling a proxy back to its owner`
    // Leaving it here but commented out as an example of something you can't do.
    /*
    public Task NestedApiMutatorAsync(INestedApi nest, CancellationToken cancellation)
    {
        // Cast and modify a member of the marshalled object, something that can't be done via the interface
        var nestConcrete = (NestedServer)nest;
        nestConcrete.baseString = nestConcrete.baseString.ToUpper();
        return Task.CompletedTask;
    }
    */

    public Task<string> TopLevelApiThatJustUppercasesAStringAsync(string argument, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        return Task.FromResult(argument.ToUpper());
    }

    internal class NestedServer : INestedApi
    {
        public string baseString;

        public NestedServer(string baseString)
        {
            this.baseString = baseString;
        }

        public void Dispose()
        {
        }

        public Task<string> ConcatenateWithConstructedArgumentAsync(string argument, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"{baseString} {argument}");
        }
    }
}
