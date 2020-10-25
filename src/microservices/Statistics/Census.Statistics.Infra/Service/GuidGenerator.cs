using System;

public class GuidGenerator : IGuidGenerator
{
    public string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }
}
