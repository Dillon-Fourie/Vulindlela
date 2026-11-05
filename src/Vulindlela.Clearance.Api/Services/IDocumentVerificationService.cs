
namespace Vulindlela.Clearance.Api.Services;

public interface IDocumentVerificationService
{
    List<string> GetRequiredDocuments(string serviceType);

    List<string> GetMissingDocuments(
        string serviceType,
        IEnumerable<string> submittedDocumentTypes);
}
