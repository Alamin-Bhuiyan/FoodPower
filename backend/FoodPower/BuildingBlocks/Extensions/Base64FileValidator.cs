using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FoodPower.BuildingBlocks.Extensions;

public static class Base64FileValidator
{
    private static readonly Regex Base64Regex = new(@"^[a-zA-Z0-9\+/]*={0,2}$", RegexOptions.Compiled);

    public static bool IsValidBase64File(string? base64String, IEnumerable<string> allowedTypes)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        var parts = base64String.Split(',');
        var base64Data = parts.Length > 1 ? parts[1] : parts[0];

        if (!IsBase64(base64Data))
            return false;

        var bytes = Convert.FromBase64String(base64Data);

        var fileType = GetFileType(bytes);
        return fileType is not null && allowedTypes.Contains(fileType, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsWithinSizeLimit(string? base64String, long maxBytes)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        var parts = base64String.Split(',');
        var base64Data = parts.Length > 1 ? parts[1] : parts[0];

        var padding = base64Data.EndsWith("==") ? 2 : base64Data.EndsWith("=") ? 1 : 0;
        var sizeInBytes = (base64Data.Length * 3L) / 4 - padding;

        return sizeInBytes <= maxBytes;
    }

    private static bool IsBase64(string base64)
        => base64.Length % 4 == 0 && Base64Regex.IsMatch(base64);

    public static string? GetFileType(byte[] bytes)
    {
        if (bytes.Length < 4)
            return null;

        if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
            return "pdf";

        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "png";

        // JPEG/JPG signature: FF D8 FF
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "jpeg";

        // WEBP: RIFF....WEBP
        if (bytes.Length >= 12
            && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return "webp";

        return null;
    }
}
