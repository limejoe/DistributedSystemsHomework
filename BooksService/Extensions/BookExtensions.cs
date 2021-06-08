﻿using BooksService.Models;
using SharedTypes.Api;

namespace BooksService.Extensions
{
    public static class BookExtensions
    {
        public static BookDto ToDto(this Book book) =>
            new()
            {
                AuthorId = book.AuthorId,
                Title = book.Title,
                Description = book.Description,
                Id = book.Id
            };

        public static BooksService.Grpc.Book ToGrpcDto(this Book book) =>
            new()
            {
                AuthorId = book.AuthorId.ToString(),
                Title = book.Title,
                Description = book.Description,
                Id = book.Id.ToString()
            };
    }
}