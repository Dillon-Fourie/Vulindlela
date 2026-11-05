
using System.Threading.Channels;

namespace Vulindlela.Clearance.Api.BackgroundTasks;

public class ClearanceQueue : IClearanceQueue
{
    private readonly Channel<int> _channel;

    public ClearanceQueue()
    {
        _channel = Channel.CreateUnbounded<int>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });
    }

    public ValueTask EnqueueAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(
            submissionId,
            cancellationToken);
    }

    public IAsyncEnumerable<int> ReadAllAsync(
        CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }
}
