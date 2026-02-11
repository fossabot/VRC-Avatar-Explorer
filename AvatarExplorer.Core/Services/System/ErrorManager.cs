using AvatarExplorer.Core.Models.Common;

namespace AvatarExplorer.Core.Services.System;

public class ErrorManager
{
    private readonly List<ErrorContext> _errorContexts = new();
    public static ErrorManager Instance { get; } = new();
    public IReadOnlyList<ErrorContext> ErrorContexts => _errorContexts;

    public event Action<string, Exception?, string?>? OnErrorOccured;
    public event Action<string, Exception?, string?>? OnInternalErrorOccured;

    private ErrorManager()
    {
    }

    public void PostInternalError(string message, Exception? exception = null, string? tag = null)
    {
        _errorContexts.Add(new(true, message, exception, tag));
        OnInternalErrorOccured?.Invoke(message, exception, tag);
    }
    public void PostError(string message, Exception? exception = null, string? tag = null)
    {
        _errorContexts.Add(new(false, message, exception, tag));
        OnErrorOccured?.Invoke(message, exception, tag);
    }
}
