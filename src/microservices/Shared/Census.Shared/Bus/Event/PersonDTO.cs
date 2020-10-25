namespace Census.Shared.Bus.Event
{
    public class AddressDTO
    {
        public string ZipCode { get; set; }

        public string AddressDesc { get; set; }

        public string Complement { get; set; }

        public string Burrow { get; set; }

        public string City { get; set; }

        public string State { get; set; }
    }

    public class PersonDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Sex { get; set; }

        public string Race { get; set; }

        public string Education { get; set; }

        public AddressDTO Address { get; set; }

        public string FatherId { get; set; }

        public string MotherId { get; set; }
    }
}
