using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Amethyst_game_engine.Core;

public static class JSONSerializer
{
    private static readonly CultureInfo _cultureInfo = new("en-US");

    public static Dictionary<string, object?> JsonToObj(string data) => JsonToObj(data.ToCharArray());

    public static Dictionary<string, object?> JsonToObj(byte[] data, int codepage = 0)
    {
        return JsonToObj(Encoding.GetEncoding(codepage).GetChars(data));
    }

    public static Dictionary<string, object?> JsonToObj(char[] data)
    {
        Dictionary<string, object?>? result = null;
        var symIndex = 0;

        try
        {
            while (symIndex < data.Length)
            {
                if (data[symIndex] == '{' && result is null)
                    result = ReadObject(data, ref symIndex);
                else if ((char.IsSeparator(data[symIndex]) || char.IsControl(data[symIndex])) == false)
                    throw new Exception();

                symIndex++;
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Syntax error. JSON file is invalid");
        }

        return result ?? [];
    }

    private static Dictionary<string, object?> ReadObject(char[] data, ref int symIndex)
    {
        Dictionary<string, object?> result = [];

        string currentKey = string.Empty;
        object? currentValue = null;

        byte flags = 0;

        do
        {
            symIndex++;

            if (data[symIndex] == '"' && (flags & 0b_110) < 6)
            {
                if ((flags & 0b_100) == 0)
                {
                    currentKey = ReadString(data, ref symIndex);
                    flags |= 0b_100;
                }
                else if ((flags & 0b_001) == 1)
                {
                    currentValue = ReadString(data, ref symIndex);
                    flags |= 0b_010;
                }
            }
            else if (data[symIndex] == ':' && (flags & 0b_101) == 4)
            {
                flags |= 0b_001;
            }
            else if (data[symIndex] == ',' && (flags & 0b_110) == 6)
            {
                result.Add(currentKey, currentValue);
                flags = 0;
            }
            else if (data[symIndex] == '{' && (flags & 0b_011) == 1)
            {
                currentValue = ReadObject(data, ref symIndex);
                flags |= 0b_010;
            }
            else if (data[symIndex] == '}' && (flags & 0b_110) is 0 or 6)
            {
                if ((flags & 0b_100) == 4)
                {
                    result.Add(currentKey, currentValue);
                    return result;
                }

                return result;

            }
            else if (data[symIndex] == '[' && (flags & 0b_011) == 1)
            {
                currentValue = ReadArray(data, ref symIndex);
                flags |= 0b_010;
            }
            else if (char.IsSeparator(data[symIndex]) || char.IsControl(data[symIndex]))
            {
                continue;
            }
            else if ((char.IsAsciiLetter(data[symIndex]) || char.IsDigit(data[symIndex]) || data[symIndex] == '-') && (flags & 0b_011) == 1)
            {
                currentValue = ReadLiteral(data, ref symIndex);
                flags |= 0b_010;
            }
            else
            {
                throw new Exception();
            }
        }
        while (true);
    }

    private static object?[] ReadArray(char[] data, ref int symIndex)
    {
        var isElementItitialized = false;
        List<object?> elements = [];

        do
        {
            symIndex++;

            if (char.IsSeparator(data[symIndex]) || char.IsControl(data[symIndex]))
            {
                continue;
            }
            else if (data[symIndex] == ',' && isElementItitialized)
            {
                isElementItitialized = false;
            }
            else if (data[symIndex] == '"' && isElementItitialized == false)
            {
                elements.Add(ReadString(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] == '{' && isElementItitialized == false)
            {
                elements.Add(ReadObject(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] == '[' && isElementItitialized == false)
            {
                elements.Add(ReadArray(data, ref symIndex));
                isElementItitialized = true;
            }
            else if ((char.IsAsciiLetter(data[symIndex]) || char.IsDigit(data[symIndex]) || data[symIndex] == '-')
                    && isElementItitialized == false)
            {
                elements.Add(ReadLiteral(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] == ']')
            {
                return [.. elements];
            }
            else
            {
                throw new Exception();
            }
        }
        while (true);
    }

    private static object? ReadLiteral(char[] data, ref int symIndex)
    {
        var startIndex = symIndex;

        while (char.IsAsciiLetter(data[symIndex]) || char.IsDigit(data[symIndex]) || data[symIndex] is '.' or '-' or '+')
        {
            symIndex++;
        }

        var length = symIndex-- - startIndex;
        var resultStr = new string(data[startIndex..(startIndex + length)]);

        if (resultStr.Length <= 5)
        {
            if (resultStr == "null")
                return null;
            else if (resultStr == "true")
                return true;
            else if (resultStr == "false")
                return false;
        }

        if (int.TryParse(resultStr, out int int_result))
            return int_result;
        else
            return double.Parse(resultStr, _cultureInfo);
    }

    private static string ReadString(char[] data, ref int symIndex)
    {
        var startIndex = symIndex + 1;

        do { symIndex++; }
        while (data[symIndex] != '"');

        var length = symIndex - startIndex;

        if (length != 0)
            return new string(data[startIndex..(startIndex + length)]);
        else
            return string.Empty;
    }
}
