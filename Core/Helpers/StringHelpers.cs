using Core.Services;
using System.Security.Cryptography;
using System.Text;

namespace Core.Helpers;

public static class StringHelpers
{
    public static string GetExtensionForIcon(string fileExt)
    {
        string icon = "";
        switch (fileExt)
        {
            case "application/msword":
            case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                icon = "<i class=\"ms-Icon ms-Icon--WordDocument\"></i>";
                break;
            case "application/vnd.ms-excel":
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                icon = "<i class=\"ms-Icon ms-Icon--ExcelDocument\"></i>";
                break;
            case "application/pdf":
                icon = "<i class=\"ms-Icon ms-Icon--PDF\"></i>";
                break;
            case "application/vnd.ms-powerpoint":
            case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                icon = "<i class=\"ms-Icon ms-Icon--PowerPointDocument\"></i>";
                break;
            case "image/png":
            case "image/jpg":
            case "image/jpeg":
                icon = "<i class=\"icon icon--image-folder\"></i>";
                break;
            case "application/octet-stream":
            case "multipart/x-zip":
            case "application/zip":
            case "application/x-rar-compressed":
            case "application/x-zip-compressed":
                icon = "<i class=\"ms-Icon ms-Icon--ZipFolder\"></i>";
                break;
            default:
                break;
        }
        return icon;
    }
    public static string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }
    public static string? TextLimit(string text, int length)
    {
        int start = 0;
        if (text != null && text.Length >= length)
        {
            return text.Substring(start, length);
        }
        return text;
    }
    public static string? Encrypt(string input)
    {
        string result = "";
        if (!string.IsNullOrEmpty(input))
        {
            result = AES.Encrypt(input, "6H5JhPhlJyELikcub7NOmpxA8jSTX5LZ5izhhwbjHkDKOH19DU1vwcEE4Wg/L6ju");
        }
        return result;
    }

    public static string? Decrypt(string input)
    {
        string result = "";
        if (!string.IsNullOrEmpty(input))
        {
            result = AES.Decrypt(input, "6H5JhPhlJyELikcub7NOmpxA8jSTX5LZ5izhhwbjHkDKOH19DU1vwcEE4Wg/L6ju");
        }
        return result;
    }
    public static string BytesToString(long byteCount)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB", "PB", "EB"]; //Longs run out around EB
        if (byteCount == 0)
        {
            return "0" + suf[0];
        }
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return $"{(Math.Sign(byteCount) * num)} {suf[place]}";
    }
    public static string? FirstCharToLowerCase(this string? str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }

    public static string? FirstCharToUpperCase(this string? str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsLower(str[0]))
            return str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str[1..];

        return str;
    }

    public static string GetStringValue(string connectionString, string key)
    {
        var startIndex = connectionString.IndexOf($"{key}=", StringComparison.Ordinal) + $"{key}=".Length;
        var endIndex = connectionString.IndexOf(';', startIndex);

        endIndex = endIndex != -1 ? endIndex : connectionString.Length;

        var stringValue = connectionString.Substring(startIndex, endIndex - startIndex);

        return stringValue;
    }
    public static string GetSecureRandomString(int byteLength = 64)
    {
        Span<byte> buffer = byteLength > 4096
            ? new byte[byteLength]
            : stackalloc byte[byteLength];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer);
    }

    public static string EncodeBase64(string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(valueBytes);
    }

    public static string DecodeBase64(string value)
    {
        var valueBytes = System.Convert.FromBase64String(value);
        return Encoding.UTF8.GetString(valueBytes);
    }
}
