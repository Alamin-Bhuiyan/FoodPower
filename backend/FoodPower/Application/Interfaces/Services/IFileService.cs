using System.Threading;
using System.Threading.Tasks;

namespace FoodPower.Application.Interfaces.Services;

public interface IFileService
{
    Task<string> SaveBase64FileAsync(
        string base64,
        string subfolder,
        CancellationToken cancellationToken = default);
}
