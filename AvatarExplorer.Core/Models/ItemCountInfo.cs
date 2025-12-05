using AvatarExplorer.Core.Interfaces;

namespace AvatarExplorer.Core.Models;

// TODO: メモリリークしてるかも
public record ItemCountInfo(ISelectableItem Item, int Count);
