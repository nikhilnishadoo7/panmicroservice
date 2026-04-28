// Utilities/ValidationHelper.cs
using System.Text.RegularExpressions;

namespace PAN.API.Utilities;

public static class ValidationHelper
{
    public static bool IsValidPan(string pan)
    {
        return Regex.IsMatch(pan, "^[A-Z]{5}[0-9]{4}[A-Z]$");
    }
}