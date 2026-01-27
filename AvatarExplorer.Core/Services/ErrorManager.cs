using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public class ErrorManager
{
    private readonly List<ErrorContext> _errorContexts = new();
    public static ErrorManager Instance { get; } = new();
    public IReadOnlyList<ErrorContext> ErrorContexts => _errorContexts;

    public event Action<string, Exception?>? OnErrorOccured;
    public event Action<string, Exception?>? OnInternalErrorOccured;

    private ErrorManager()
    {
    }

    public void PostInternalError(string message, Exception? exception = null)
    {
        _errorContexts.Add(new(true, message, exception));
        OnInternalErrorOccured?.Invoke(message, exception);
    }
    public void PostError(string message, Exception? exception = null)
    {
        _errorContexts.Add(new(false, message, exception));
        OnErrorOccured?.Invoke(message, exception);
    }
}
