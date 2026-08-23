using Census.People.Domain.Interfaces;

namespace Census.People.Infra.Service;

public class GuidGenerator : IGuidGenerator
{
    public string GenerateGuid() => Guid.NewGuid().ToString();
}
