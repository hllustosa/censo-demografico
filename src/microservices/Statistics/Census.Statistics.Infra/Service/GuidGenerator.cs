using Census.Statistics.Domain.Interfaces;

namespace Census.Statistics.Infra.Service;

public class GuidGenerator : IGuidGenerator
{
    public string GenerateGuid() => Guid.NewGuid().ToString();
}
