using PostmanCloneLibrary.Models;

namespace PostmanCloneLibrary;

public interface IApiAccess
{
    bool IsValidUrl(string url);
    Task<ResponseData> SendAsync(RequestTab request, CancellationToken ct = default);
}
