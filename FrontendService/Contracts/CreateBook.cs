using System;

namespace FrontendService.Contracts
{
    public class CreateBook
    {
        public Guid AuthorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}