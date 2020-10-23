using Census.People.Domain.Values;

namespace Census.People.Domain.Interfaces
{
    public interface IAddressRepository
    {
        Address GetAddressByZipCode(string zipCode);
    }
}
