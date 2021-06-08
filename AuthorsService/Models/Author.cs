using System;

namespace AuthorsService.Models
{
    public class Author
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public uint Age { get; set; }
        public string Biography { get; set; }
        public uint NumberOfBooks { get; set; }
    }
}
