using System;

namespace SharedTypes.Api
{
    public class UpdateBooksCount
    {
        public Guid AuthorId { get; set; }
        public uint Delta { get; set; }
        public UpdateBooksCountType UpdateType { get; set; }
    }
}