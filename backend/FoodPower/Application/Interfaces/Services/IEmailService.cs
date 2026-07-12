using System.Threading;
using System.Threading.Tasks;

namespace FoodPower.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
