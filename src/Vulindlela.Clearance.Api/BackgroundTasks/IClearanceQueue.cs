
namespace Vulindlela.Clearance.Api.BackgroundTasks;

public interface IClearanceQueue
{
    ValueTask EnqueueAsync(
        int submissionId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<int> ReadAllAsync(
        CancellationToken cancellationToken);
}
