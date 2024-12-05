using IGeekFan.FreeKit.Extras.Dto;
using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Data;
using LinCms.Entities;
using LinCms.Entities.Base;
using LinCms.Exceptions;
using LinCms.Extensions;
using LinCms.IRepositories;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LinCms.v1.Books
{
    public class BookService : ApplicationService, IBookService
    {
        private readonly IAuditBaseRepository<Book> _bookRepository;
        private readonly IAuditBaseRepository<BaseItem> _baseItemRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BookService(IAuditBaseRepository<Book> bookRepository, IAuditBaseRepository<BaseItem> baseItemRepository, IFileRepository fileRepository, IWebHostEnvironment hostingEnvironment)
        {
            _bookRepository = bookRepository;
            _baseItemRepository = baseItemRepository;
            _fileRepository = fileRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task CreateAsync(CreateUpdateBookDto createBook)
        {
            if (!long.TryParse(createBook.ISBN, out _))
            {
                throw new LinCmsException("ISBN 格式不正确");
            }

            if (createBook.ISBN.Length != 13)
            {
                throw new LinCmsException("ISBN 长度不正确，当前长度：" + createBook.ISBN.Length);
            }

            bool exist = _bookRepository.Select.Any(r => r.Isbn == createBook.ISBN);
            if (exist)
            {
                throw new LinCmsException("书籍《" + createBook.Title + "》" + createBook.Subtitle + "已存在");
            }

            Book book = Mapper.Map<Book>(createBook);
            await _bookRepository.InsertAsync(book);
        }

        public Task DeleteAsync(long id)
        {
            Book book = _bookRepository.Where(a => a.Id == id).ToOne();
            DeletePicFile(book);
            return _bookRepository.DeleteAsync(new Book { Id = id });
        }

        private void DeletePicFile(Book book)
        {
            string url = _hostingEnvironment.WebRootPath + "/" + book.Cover;
            if (File.Exists(@url))
                File.Delete(@url);
        }

        public async Task UpdateAsync(long id, CreateUpdateBookDto updateBook)
        {
            Book book = await _bookRepository.Where(r => r.Id == id).ToOneAsync();
            if (book == null)
            {
                throw new LinCmsException("更新失败，指定书籍不存在");
            }

            if (updateBook.ISBN.Length != 13)
            {
                throw new LinCmsException("ISBN格式不正确");
            }

            bool exist = await _bookRepository.Select.AnyAsync(r => r.Isbn == updateBook.ISBN && r.Id != id);
            if (exist)
            {
                throw new LinCmsException("书籍已存在");
            }

            if (updateBook.DatePurchased == DateTime.MinValue)
            {
                throw new LinCmsException("购买日期有误");
            }            

            if (book.Cover != updateBook.Cover && _fileRepository.GetFileUrl(book.Cover) != updateBook.Cover)
            {
                DeletePicFile(book);
            }
            //book.Image = updateBook.Image;
            //book.Title = updateBook.Title;
            //book.Author = updateBook.Author;
            //book.Summary = updateBook.Summary;
            //book.Summary = updateBook.Summary;

            //使用AutoMapper方法简化类与类之间的转换过程
            Mapper.Map(updateBook, book);
            await _bookRepository.UpdateAsync(book);
        }

        public async Task<BookDto> GetAsync(long id)
        {
            Book book = await _bookRepository.Where(a => a.Id == id).ToOneAsync();
            if (book == null)
            {
                throw new LinCmsException("指定书籍不存在，ID: " + id);
            }
            book.Cover = _fileRepository.GetFileUrl(book.Cover);
            return Mapper.Map<BookDto>(book);
        }

        public async Task<long> GetTotalAsync(bool isRead)
        {
            return await _bookRepository.WhereIf(isRead, p => p.IsRead == isRead).CountAsync();
        }

        public async Task<List<BookDto>> GetListAsync()
        {
            List<BookDto> items = (await _bookRepository.Select.OrderByDescending(r => r.Id).ToListAsync()).Select(r => Mapper.Map<BookDto>(r)).ToList();
            return items;
        }

        public async Task<PagedResultDto<BookDto>> GetPageListAsync(PageDto pageDto)
        {
            List<BookDto> items = (await _bookRepository.WhereIf(!string.IsNullOrEmpty(pageDto.ItemType), p => Convert.ToInt16(pageDto.ItemType) == p.BookType).WhereIf(pageDto.Keyword != "{\"isTrusted\":true}" && !string.IsNullOrEmpty(pageDto.Keyword), p => p.Isbn.Contains(pageDto.Keyword.Replace("-", string.Empty)) || p.Title.Contains(pageDto.Keyword) || p.Subtitle.Contains(pageDto.Keyword) || p.Author1.Contains(pageDto.Keyword) || p.Author2.Contains(pageDto.Keyword) || p.Author3.Contains(pageDto.Keyword)).OrderByDescending(r => r.DatePurchased).OrderByDescending(r => r.Isbn).ToPagerListAsync(pageDto, out long count)).Select(r => Mapper.Map<BookDto>(r)).ToList();

            foreach (var book in items)
            {
                if (book.Isbn.Length > 3)
                    book.Isbn = book.Isbn.Insert(3, "-").Insert(5, "-").Insert(10, "-").Insert(15, "-");
                book.BookTypeName = _baseItemRepository.Where(p => p.BaseTypeId == 1 && p.ItemCode == book.BookType.ToString()).ToOne().ItemName;
                book.AuthorTypeName1 = _baseItemRepository.Where(p => p.BaseTypeId == 2 && p.ItemCode == book.AuthorType1.ToString()).ToOne().ItemName;
                book.AuthorTypeName2 = _baseItemRepository.Where(p => p.BaseTypeId == 2 && p.ItemCode == book.AuthorType2.ToString()).ToOne().ItemName;
                book.AuthorTypeName3 = _baseItemRepository.Where(p => p.BaseTypeId == 2 && p.ItemCode == book.AuthorType3.ToString()).ToOne().ItemName;
            }
            return new PagedResultDto<BookDto>(items, count);
        }
    }
}