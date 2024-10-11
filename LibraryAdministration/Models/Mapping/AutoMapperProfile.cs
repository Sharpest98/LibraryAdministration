using AutoMapper;
using LibraryAdministration.Models.Database;
using LibraryAdministration.Models.DTO.BookDTO;
using LibraryAdministration.Models.DTO.ReaderDTO;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryAdministration.Models.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Book, Book>();

            CreateMap<UpdateBookDTO, Book>()
                .ForMember(dest => dest.Author, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.Author));
                    opt.MapFrom(src => src.Author);
                })
                .ForMember(dest => dest.Title, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.Title));
                    opt.MapFrom(src => src.Title);
                })
                .ForMember(dest => dest.ReleaseYear, opt =>
                {
                    opt.PreCondition(scr => scr.ReleaseYear is not null);
                    opt.MapFrom(scr => scr.ReleaseYear);
                })
                .ForMember(dest => dest.Genre, opt =>
                {
                    opt.PreCondition(scr => !string.IsNullOrEmpty(scr.Genre));
                    opt.MapFrom(opt => opt.Genre);
                })
                .ForMember(dest => dest.Id, opt =>
                {
                    opt.MapFrom(opt => opt.Id);
                })
                .ForMember(dest => dest.ReaderId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore());
                

            CreateMap<CreateBookDTO, Book>();
            CreateMap<Book, BookDTO>();
            
            CreateMap<CreateReaderDTO, Reader>();
            CreateMap<Reader, ReaderDTO>();
            CreateMap<UpdateReaderDTO, Reader>()
                .ForMember(dest => dest.Name, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.Name));
                    opt.MapFrom(opt => opt.Name);
                })
                .ForMember(dest => dest.LastName, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.LastName));
                    opt.MapFrom(opt => opt.LastName);
                })
                .ForMember(dest => dest.Address, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.Address));
                    opt.MapFrom(opt => opt.Address);
                })
                .ForMember(dest => dest.PhoneNumber, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrEmpty(src.PhoneNumber));
                    opt.MapFrom(opt => opt.PhoneNumber);
                })
                .ForMember(dest => dest.Id, opt => opt.Ignore());



        }
    }
}
