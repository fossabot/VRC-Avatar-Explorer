namespace AvatarExplorer.Core.Models;

public class ContextMenuAction(string name, ActionKey actionKey = ActionKey.None, ActionLayer actionLayer = ActionLayer.None, string tag = "", bool reloadRequired = false)
{
    public string DisplayName { get; } = name;
    public ActionKey ActionKey { get; } = actionKey;
    public ActionLayer ActionLayer { get; } = actionLayer;
    public string Tag { get; } = tag;
    public bool ReloadRequired { get; } = reloadRequired;
}
