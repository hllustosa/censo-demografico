using Census.People.Domain.Values;

namespace Census.People.Application.Commands
{
    public abstract class BasePersonCommand
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public char Sex { get; set; }

        public string Race { get; set; }

        public string Education { get; set; }

        public Address Address { get; set; }

        public string FatherId { get; set; }

        public string MotherId { get; set; }

    }
}
