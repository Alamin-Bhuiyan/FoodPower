using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Extensions;

namespace FoodPower.Features.Auth.Services;

public class FileService : IFileService
{
    public async Task<string> SaveBase64FileAsync(
        string base64,
        string subfolder,
        CancellationToken cancellationToken = default)
    {
        var parts = base64.Split(',');
        var base64Data = parts.Length > 1 ? parts[1] : parts[0];

        var bytes = Convert.FromBase64String(base64Data);

        var extension = Base64FileValidator.GetFileType(bytes) ?? "png";
        var fileName = $"{Guid.NewGuid():N}.{extension}";

        var directory = Path.Combine(Directory.GetCurrentDirectory(), "resources", subfolder);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fullPath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);

        return $"resources/{subfolder}/{fileName}";
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        try
        {
            var normalized = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), normalized);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // best-effort delete; ignore failures.
        }
    }
}
