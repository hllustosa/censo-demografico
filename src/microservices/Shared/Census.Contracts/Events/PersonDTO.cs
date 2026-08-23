namespace Census.Contracts.Events;

public class AddressDTO
{
    public string ZipCode { get; set; } = string.Empty;

    public string AddressDesc { get; set; } = string.Empty;

    public string Complement { get; set; } = string.Empty;

    public string Burrow { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
}

public class PersonDTO
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Sex { get; set; } = string.Empty;

    public string Race { get; set; } = string.Empty;

    public string Education { get; set; } = string.Empty;

    public AddressDTO Address { get; set; } = new();

    public string FatherId { get; set; } = string.Empty;

    public string MotherId { get; set; } = string.Empty;
}
