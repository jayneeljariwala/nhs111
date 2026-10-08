namespace NHS111.Domain.Entities;

public class Patient(
    Guid id,
    string nhsNumber,
    string name,
    DateTime dateOfBirth,
    string? phoneNumber = null,
    string? email = null)
{
    public Guid Id { get; private set; } = id == Guid.Empty ? Guid.NewGuid() : id;
    public string NhsNumber { get; private set; } = nhsNumber;
    public string Name { get; private set; } = name;
    public DateTime DateOfBirth { get; private set; } = dateOfBirth;
    public string? PhoneNumber { get; private set; } = phoneNumber;
    public string? Email { get; private set; } = email;

    public void Update(
        string? nhsNumber = null,
        string? name = null,
        DateTime? dateOfBirth = null,
        string? phoneNumber = null,
        string? email = null)
    {
        if (nhsNumber != null) NhsNumber = nhsNumber;
        if (name != null) Name = name;
        if (dateOfBirth.HasValue) DateOfBirth = dateOfBirth.Value;
        if (phoneNumber != null) PhoneNumber = phoneNumber;
        if (email != null) Email = email;
    }
}
