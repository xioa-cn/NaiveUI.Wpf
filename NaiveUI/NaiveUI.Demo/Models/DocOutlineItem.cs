using System.Collections.Generic;
using System.Linq;

namespace NaiveUI.Demo.Models;

public sealed class DocOutlineItem
{
    public DocOutlineItem(string label, string targetName)
    {
        Label = label;
        TargetName = targetName;
    }

    public string Label { get; }

    public string TargetName { get; }

    public static IReadOnlyList<DocOutlineItem> Create(params (string Label, string TargetName)[] items)
    {
        return [.. items.Select(static item => new DocOutlineItem(item.Label, item.TargetName))];
    }
}
