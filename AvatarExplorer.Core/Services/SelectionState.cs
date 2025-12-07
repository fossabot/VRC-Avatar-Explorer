using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public record SelectionNode(ItemTagState State, string Key);

internal class SelectionState
{
    private readonly Stack<SelectionNode> _stack = new();

    public void Push(ItemTagState state, string key)
    {
        if (state == ItemTagState.SearchItem && Search(ItemTagState.SearchItem) != null)
        {
            foreach (var itemTagState in _stack.Select(i => i.State).ToArray())
            {
                Pop();
                if (itemTagState == ItemTagState.SearchItem) break;
            }
        }

        _stack.Push(new SelectionNode(state, key));
    }

    public SelectionNode? Pop()
    {
        if (_stack.Count == 0) return null;
        return _stack.Pop();
    }

    public SelectionNode? Current => _stack.Count > 0 ? _stack.Peek() : null;

    public SelectionNode? Root => _stack.Count > 0 ? _stack.Last() : null;

    public void Clear() => _stack.Clear();

    public SelectionNode? Search(ItemTagState state) => _stack.FirstOrDefault(i => i.State == state);
    
    public IEnumerable<SelectionNode> GetCurrentPath() => _stack.Reverse();
}
