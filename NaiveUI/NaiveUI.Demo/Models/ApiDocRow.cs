namespace NaiveUI.Demo.Models;

public sealed class ApiDocRow
{
    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string DefaultValue { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public bool HasType => !string.IsNullOrWhiteSpace(Type);

    public bool HasDefaultValue => !string.IsNullOrWhiteSpace(DefaultValue);

    public bool HasVersion => !string.IsNullOrWhiteSpace(Version);
}
