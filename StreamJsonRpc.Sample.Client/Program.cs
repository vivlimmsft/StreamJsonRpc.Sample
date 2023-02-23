using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nerdbank.Streams;
using ServerApi;
using StreamJsonRpc;

class Program
{
    static bool useStdIo = true;

    static async Task Main()
    {
        if (useStdIo)
        {
            var psi = new ProcessStartInfo(FindPathToServer(), "stdio");
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            var process = Process.Start(psi);
            var stdioStream = FullDuplexStream.Splice(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            await ActAsRpcClientAsync(stdioStream);
        }
        else
        {
            Console.WriteLine("Connecting to server...");
            using (var stream = new NamedPipeClientStream(".", "StreamJsonRpcSamplePipe", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                await ActAsRpcClientAsync(stream);
                Console.WriteLine("Terminating stream...");
            }
        }
    }

    private static async Task ActAsRpcClientAsync(Stream stream)
    {
        using (var service = JsonRpc.Attach<IServerApi>(stream))
        {
            // If you have anything that could cancel this operation, you should use a real cancellationtoken here
            var cancellationToken = CancellationToken.None;

            Console.WriteLine("Connected. Sending request...");
            Console.WriteLine($"Using service to convert to uppercase: {await service.TopLevelApiThatJustUppercasesAStringAsync("hello world", cancellationToken)}");

            // Call method that returns a marshalled object implementing INestedApi. It must be disposed!
            using (var nestedApi = await service.GetNestedApiAsync("16 ounce", cancellationToken))
            {
                // Use it; it just concatenates the string it was constructed with to the argument
                var nestedApiResult = await nestedApi.ConcatenateWithConstructedArgumentAsync("oat milk latte", cancellationToken);
                Console.WriteLine($"nested api call result: {nestedApiResult}"); // Should read "16 ounce oat milk latte"

                // I found that passing a marshaled proxy back to the server is not allowed: `NotSupportedException: Marshaling a proxy back to its owner`
                // Leaving the code here but commented to demonstrate the sort of thing you *can't* do
                // Call a method on the service that casts the marshalled object back into the concrete type, and modifies a member to make the string it was passed ALL UPPERCASE
                //await service.NestedApiMutatorAsync(nestedApi, cancellationToken); // It doesn't return anything, the modification happens only on the server

                // Call the same method on the marshalled object which should be modified on the server side.
                //var nestedApiResult2 = await nestedApi.DoAThingGetAStringAsync("oat milk latte", cancellationToken);
                //Console.WriteLine($"nested api call result: {nestedApiResult2}"); // Should read "16 OUNCE oat milk latte"
            }


        }
    }

    private static string FindPathToServer()
    {
#if DEBUG
        const string Config = "Debug";
#else
        const string Config = "Release";
#endif
        return Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            $@"..\..\..\..\StreamJsonRpc.Sample.Server\bin\{Config}\netcoreapp3.1\StreamJsonRpc.Sample.Server.exe");
    }
}
