﻿using System;

namespace FrontendService.Contracts
{
    public class Book
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AuthorFirstname { get; set; }
        public string AuthorLastName { get; set; }
    }
}