namespace Bimss.Domain.Membership.ReferenceData;

public abstract class ReferenceDataItem
{
    protected ReferenceDataItem(Guid id, string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Code = code;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void SetActive(bool isActive) => IsActive = isActive;
}
