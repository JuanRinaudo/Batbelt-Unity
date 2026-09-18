using System.Collections.Generic;

public static class JsonValidator
{
    public static bool IsValidJsonStructure(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;

        json = json.Trim();

        // Must start and end with {} or []
        if (!((json.StartsWith("{") && json.EndsWith("}")) ||
              (json.StartsWith("[") && json.EndsWith("]"))))
        {
            return false;
        }

        var stack = new Stack<char>();
        bool inString = false;
        bool isEscaped = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (isEscaped)
            {
                isEscaped = false;
                continue;
            }

            if (c == '\\' && inString)
            {
                isEscaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString) continue;

            if (c == '{' || c == '[')
            {
                stack.Push(c);
            }
            else if (c == '}')
            {
                if (stack.Count == 0 || stack.Pop() != '{') return false;
            }
            else if (c == ']')
            {
                if (stack.Count == 0 || stack.Pop() != '[') return false;
            }
        }

        return stack.Count == 0 && !inString;
    }
}