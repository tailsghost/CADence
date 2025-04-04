namespace CADence.Abstractions.Global;

public static class ExecuteAccuracy
{
    private static bool _isExecute = false;

    public static void SetExecute(bool value)
        => _isExecute = value;

    public static bool GetExecute()
        => _isExecute;
}

