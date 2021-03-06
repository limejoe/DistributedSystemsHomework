using System;

namespace FrontendService.Contracts
{
    public class Author
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte Age { get; set; }
        public string Biography { get; set; }
        public uint NumberOfBooks { get; set; }
    }
}