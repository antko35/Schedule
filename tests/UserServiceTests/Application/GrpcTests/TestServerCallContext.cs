namespace UserServiceTests.Application.GrpcTests
{
    using Grpc.Core;

    public static class TestServerCallContext
    {
        public static ServerCallContext Create(
        string method = "TestMethod",
        string host = "localhost",
        DateTime deadline = default,
        Metadata requestHeaders = null,
        CancellationToken cancellationToken = default,
        string peer = null,
        AuthContext authContext = null,
        ContextPropagationToken contextPropagationToken = null,
        Func<Metadata, Task> writeHeadersFunc = null,
        Func<WriteOptions> writeOptionsGetter = null,
        Action<WriteOptions> writeOptionsSetter = null)
        {
            deadline = deadline == default ? DateTime.UtcNow.AddMinutes(1) : deadline;
            requestHeaders ??= new Metadata();

            return Grpc.Core.Testing.TestServerCallContext.Create(
                method,
                host,
                deadline,
                requestHeaders,
                cancellationToken,
                peer,
                authContext,
                contextPropagationToken,
                writeHeadersFunc,
                writeOptionsGetter,
                writeOptionsSetter);
        }
    }
}
