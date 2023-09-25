using Microsoft.Spark.Interop.Ipc;
using Microsoft.Spark.Network;

using System;
using System.Net;
using System.Threading.Tasks;

using Xunit;

namespace Microsoft.Spark.Worker.UnitTest
{
    [Collection("Spark Unit Tests")]
    public class SimpleWorkerTests
    {
        [Theory]
        [MemberData(nameof(TestData.VersionData), MemberType = typeof(TestData))]
        public async Task TestsSimpleWorkerTaskRunners(string version)
        {
            using ISocketWrapper serverListener = SocketFactory.CreateSocket();
            var ipEndpoint = (IPEndPoint)serverListener.LocalEndPoint;

            serverListener.Listen();

            var typedVersion = new Version(version);
            var simpleWorker = new SimpleWorker(typedVersion);

            Task clientTask = Task.Run(() => simpleWorker.Run(ipEndpoint.Port));

            PayloadWriter payloadWriter = new PayloadWriterFactory().Create(typedVersion);
            using (ISocketWrapper serverSocket = serverListener.Accept())
            {
                if ((typedVersion.Major == 3 && typedVersion.Minor >= 2) || typedVersion.Major > 3)
                {
                    int pid = SerDe.ReadInt32(serverSocket.InputStream);
                }

                TaskRunnerTests.TestTaskRunnerReadWrite(serverSocket, payloadWriter);
            }

            await clientTask;

            // Assert.True(clientTask.Wait(5000));
        }
    }
}
