﻿using System.Threading.Tasks;
using IGeekFan.FreeKit.Extras.Dto;
using LinCms.Data;
using LinCms.v1.Books;
using Xunit;

namespace LinCms.Test.Service.v1
{
    public class BookServiceTest : BaseLinCmsTest
    {
        private readonly IBookService _bookService;

        public BookServiceTest() : base()
        {
            _bookService = GetRequiredService<IBookService>();
        }

        [Fact]
        public async Task GetListAsyncTest()
        {
            PagedResultDto<BookDto> books = await _bookService.GetPageListAsync(new PageDto { });
            var bookslist = await _bookService.GetListAsync();

            Assert.True(books.Count > 0);
        }
    }
}
