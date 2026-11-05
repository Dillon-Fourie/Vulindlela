
namespace Vulindlela.Clearance.Api.Services;

public class DocumentVerificationService
    : IDocumentVerificationService
{
    private static readonly Dictionary<string, List<string>>
        RequiredDocuments = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ID Application"] = new()
            {
                "Birth Certificate",
                "Proof of Address"
            },

            ["Passport Application"] = new()
            {
                "Identity Document",
                "Proof of Address",
                "Passport Photo"
            },

            ["Driver's Licence Renewal"] = new()
            {
                "Identity Document",
                "Current Driver's Licence",
                "Proof of Address"
            }
        };

    public List<string> GetRequiredDocuments(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType) ||
            !RequiredDocuments.TryGetValue(
                serviceType.Trim(),
                out var documents))
        {
            throw new ArgumentException(
                $"Unknown service type: {serviceType}");
        }

        return new List<string>(documents);
    }

    public List<string> GetMissingDocuments(
        string serviceType,
        IEnumerable<string> submittedDocumentTypes)
    {
        var required = GetRequiredDocuments(serviceType);

        var submitted = new HashSet<string>(
            submittedDocumentTypes
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim()),
            StringComparer.OrdinalIgnoreCase);

        return required
            .Where(document => !submitted.Contains(document))
            .ToList();
    }
}
