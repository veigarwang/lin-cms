using AutoMapper;
using LinCms.Entities;

namespace LinCms.v1.Encyclopedias
{
    public class EncyclopediaProfile : Profile
    {
        public EncyclopediaProfile()
        {
            CreateMap<CreateUpdateEncyclopediaDto, Encyclopedia>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Encyclopedia, EncyclopediaDto>();
        }
    }
}
