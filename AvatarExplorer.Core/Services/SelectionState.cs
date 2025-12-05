namespace AvatarExplorer.Core.Services;

public record SelectionNode(string Type, string Key);

internal class SelectionState
{
    private readonly Stack<SelectionNode> _stack = new();

    public void Push(string type, string key)
    {
        _stack.Push(new SelectionNode(type, key));
    }

    public SelectionNode? Pop()
    {
        if (_stack.Count == 0) return null;
        return _stack.Pop();
    }

    public SelectionNode? Current => _stack.Count > 0 ? _stack.Peek() : null;

    public SelectionNode? Root => _stack.Count > 0 ? _stack.Last() : null;

    public void Clear() => _stack.Clear();

    public SelectionNode? Search(string type) => _stack.FirstOrDefault(i => i.Type == type);
}
