using AutoMapper;
using LinCms.Entities;

namespace LinCms.v1.Books;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<CreateUpdateBookDto, Book>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); ;
        CreateMap<Book, BookDto>();
    }
}