namespace CADence.Infrastructure.Command.Commands.Helpers;

public static class CommandUtils
{
    public static Dictionary<char, string> ParseRegularCommand(string cmd)
    {
        var parameters = new Dictionary<char, string>();
        var code = '\0';
        var start = 0;
        for (var i = 0; i <= cmd.Length; i++)
        {
            var c = (i < cmd.Length) ? cmd[i] : '\0';
            if (i != cmd.Length && !char.IsLetter(c)) continue;
            
            if (i > start)
            {
                parameters[code] = cmd.Substring(start, i - start);
            }
            code = c;
            start = i + 1;
        }
        return parameters;
    }
}