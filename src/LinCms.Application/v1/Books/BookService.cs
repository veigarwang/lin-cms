using IGeekFan.FreeKit.Extras.Dto;
using IGeekFan.FreeKit.Extras.FreeSql;
using LinCms.Data;
using LinCms.Entities;
using LinCms.Entities.Base;
using LinCms.Exceptions;
using LinCms.Extensions;
using LinCms.IRepositories;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            if (book.Cover != updateBook.Cover && _fileRepository.GetFileUrl(book.Cover) != updateBook.Cover)
            {
                DeletePicFile(book);
            }

            SetUnchangedFieldsToNull(updateBook, book);

            if (updateBook.ISBN != null && updateBook.ISBN.Length != 13)
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

            //book.Image = updateBook.Image;
            //book.Title = updateBook.Title;
            //book.Author = updateBook.Author;
            //book.Summary = updateBook.Summary;
            //book.Summary = updateBook.Summary;

            //使用AutoMapper方法简化类与类之间的转换过程
            Mapper.Map(updateBook, book);

            if (updateBook.GetType().GetProperties().All(p => p.GetValue(updateBook) == null))
            {
                throw new LinCmsException("未发现任何改动");
            }

            if (book.ShelfLocation == "[]")
            {
                book.ShelfLocation = null;
            }

            await _bookRepository.UpdateAsync(book);
        }

        private static void SetUnchangedFieldsToNull(CreateUpdateBookDto updateBook, Book book)
        {
            Type dtoType = typeof(CreateUpdateBookDto);
            Type entityType = typeof(Book);

            foreach (var property in dtoType.GetProperties())
            {
                try
                {
                    var originalProperty = entityType.GetProperty(property.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (originalProperty == null) continue; // 如果原始对象中没有该属性，则跳过

                    var newValue = property.GetValue(updateBook);
                    var oldValue = originalProperty.GetValue(book);
                    bool isModified = false;
                    if (newValue != null)
                    {
                        if (newValue is DateTime newDate && oldValue is DateTime oldDate)
                        {
                            isModified = !(newDate.Year == oldDate.Year &&
                                           newDate.Month == oldDate.Month &&
                                           newDate.Day == oldDate.Day &&
                                           newDate.Hour == oldDate.Hour &&
                                           newDate.Minute == oldDate.Minute &&
                                           newDate.Second == oldDate.Second &&
                                           newDate.Millisecond == oldDate.Millisecond);
                        }
                        else
                        {
                            isModified = !newValue.Equals(oldValue);
                        }
                    }
                    else
                    {
                        // 如果 newValue 为空，可能是前端未传该字段，或者用户想清空该字段
                        if (oldValue != null)
                        {
                            isModified = true;
                        }
                    }

                    if (!isModified)
                    {
                        property.SetValue(updateBook, null);
                    }

                }
                catch (Exception ex)
                {
                    throw new LinCmsException("更新失败：" + ex.Message);
                }
            }
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
            List<BookDto> items = null;
            long count = 0;
            pageDto.Keyword = pageDto.Keyword?.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            items = pageDto.ExactMatch ? (await _bookRepository
                .WhereIf(!string.IsNullOrEmpty(pageDto.ItemType), p => Convert.ToInt16(pageDto.ItemType) == p.BookType)
                .WhereIf(pageDto.Keyword != "{\"isTrusted\":true}" && !string.IsNullOrEmpty(pageDto.Keyword),
                    p => p.Isbn == pageDto.Keyword.Replace("-", string.Empty)
                    || p.Title == pageDto.Keyword
                    || p.Subtitle == pageDto.Keyword
                    || p.Author1 == pageDto.Keyword
                    || p.Author1.StartsWith(pageDto.Keyword + ",")
                    || p.Author1.EndsWith("," + pageDto.Keyword)
                    || p.Author1.Contains("," + pageDto.Keyword + ",")
                    || p.Author2 == pageDto.Keyword
                    || p.Author2.StartsWith(pageDto.Keyword + ",")
                    || p.Author2.EndsWith("," + pageDto.Keyword)
                    || p.Author2.Contains("," + pageDto.Keyword + ",")
                    || p.Author3 == pageDto.Keyword
                    || p.Author3.StartsWith(pageDto.Keyword + ",")
                    || p.Author3.EndsWith("," + pageDto.Keyword)
                    || p.Author3.Contains("," + pageDto.Keyword + ","))
                .OrderByDescending(r => r.DatePurchased)
                .OrderByDescending(r => r.Isbn)
                .ToPagerListAsync(pageDto, out count))
                .Select(r => Mapper.Map<BookDto>(r)).ToList()

                : (await _bookRepository
                .WhereIf(!string.IsNullOrEmpty(pageDto.ItemType), p => Convert.ToInt16(pageDto.ItemType) == p.BookType)
                .WhereIf(pageDto.Keyword != "{\"isTrusted\":true}" && !string.IsNullOrEmpty(pageDto.Keyword),
                    p => p.Isbn.Contains(pageDto.Keyword.Replace("-", string.Empty))
                    || p.Title.Contains(pageDto.Keyword)
                    || p.Subtitle.Contains(pageDto.Keyword)
                    || p.Author1.Contains(pageDto.Keyword)
                    || p.Author2.Contains(pageDto.Keyword)
                    || p.Author3.Contains(pageDto.Keyword))
                .OrderByDescending(r => r.DatePurchased)
                .OrderByDescending(r => r.Isbn)
                .ToPagerListAsync(pageDto, out count))
                .Select(r => Mapper.Map<BookDto>(r)).ToList();

            foreach (var book in items)
            {
                if (book.Isbn.Length > 3)
                    book.Isbn = book.Isbn.Insert(3, "-").Insert(5, "-").Insert(10, "-").Insert(15, "-");
                book.BookTypeName = _baseItemRepository.Where(p => p.BaseTypeId == 1 && p.ItemCode == book.BookType.ToString()).ToOne().ItemName;
                book.AuthorTypeName1 = _baseItemRepository.Where(p => p.BaseTypeId == 2 && p.ItemCode == book.AuthorType1.ToString()).ToOne().ItemName;
                if (book.AuthorType2 != null)
                    book.AuthorTypeName2 = _baseItemRepository.Where(p => p.BaseTypeId == 2 && p.ItemCode == book.AuthorType2.ToString()).ToOne().ItemName;
                if (book.AuthorType3 != null)
                    book.AuthorTypeName3 = book.AuthorTypeName3 ?? _baseItemRepository.Where(p => p.BaseTypeId == 2 && p.ItemCode == book.AuthorType3.ToString()).ToOne().ItemName;
                book.ShelfLocation = string.IsNullOrEmpty(book.ShelfLocation) || book.ShelfLocation == "[]" ? null : GetShelfLocation(book.ShelfLocation);
            }
            return new PagedResultDto<BookDto>(items, count);
        }

        private string GetShelfLocation(string shelfLocation)
        {
            List<string> locationList = JsonConvert.DeserializeObject<List<string>>(shelfLocation);
            string city = _baseItemRepository.Where(p => p.BaseTypeId == 10 && p.ItemCode == locationList[0]).ToOne().ItemName;
            string shelf = _baseItemRepository.Where(p => p.BaseTypeId == 11 && p.ItemCode == locationList[1]).ToOne().ItemName.Split('|')[1];
            return city + " " + shelf;
        }
    }


}