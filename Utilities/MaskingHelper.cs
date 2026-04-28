using System.Text.RegularExpressions;

namespace PAN.API.Infrastructure.Logging;

public static class MaskingHelper
{
    public static string MaskSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // PAN (ABCDE1234F)
        input = Regex.Replace(input, @"\b[A-Z]{5}[0-9]{4}[A-Z]\b", m =>
            $"{m.Value.Substring(0, 5)}****{m.Value[^1]}");

        // Mobile (10 digits)
        input = Regex.Replace(input, @"\b\d{10}\b", m =>
            $"******{m.Value.Substring(6, 4)}");

        return input;
    }
}