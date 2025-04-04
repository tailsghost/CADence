namespace CADence.Abstractions.Global;

/// <summary>
/// Provides functionality to enable or disable the execution of accuracy calculations.
/// </summary>
public static class ExecuteAccuracy
{
    private static bool _isExecute = false;

    /// <summary>
    /// Sets the execution flag for accuracy calculations.
    /// </summary>
    /// <param name="value">True to enable, false to disable.</param>
    public static void SetExecute(bool value)
        => _isExecute = value;

    /// <summary>
    /// Retrieves the current state of the execution flag.
    /// </summary>
    /// <returns>True if execution is enabled; otherwise, false.</returns>
    public static bool GetExecute()
        => _isExecute;
}

