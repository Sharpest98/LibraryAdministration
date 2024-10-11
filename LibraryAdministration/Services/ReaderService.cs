using AutoMapper;
using LibraryAdministration.Database;
using LibraryAdministration.Exceptions;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.Database;
using LibraryAdministration.Models.DTO.BookDTO;
using LibraryAdministration.Models.DTO.ReaderDTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAdministration.Services
{
    public class ReaderService : IReaderService
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public ReaderService(DataContext dataContext, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<ReaderDTO> CreateReader(CreateReaderDTO createReaderDTO)
        {
            if(createReaderDTO == null)
            {
                throw new ArgumentNullException(nameof(createReaderDTO));
            }
            var reader = _mapper.Map<Reader>(createReaderDTO);
            reader.CreatedBy = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name)!;
            reader.CreatedOn = DateTime.Now;
            _dataContext.Reader.Add(reader);
            await _dataContext.SaveChangesAsync();
            return _mapper.Map<ReaderDTO>(reader);
        }

        public async Task DeleteReader(int id)
        {
            var reader = await _dataContext.Reader.FirstOrDefaultAsync(r => r.Id == id);
            if (reader == null)
            {
                throw new ObjectNotFoundException(nameof(Reader), id);
            }
            var books = await _dataContext.Book.Where(r => r.ReaderId == id).ToListAsync();
            if(books.Count != 0)
            {
                throw new ImpossibleToDeleteReaderException();
            }
            _dataContext.Reader.Remove(reader);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<ReaderDTO>> GetAll()
        {
            var readers = await _dataContext.Reader.Select( x => _mapper.Map<ReaderDTO>(x)).ToListAsync();

            return readers;
        }

        public async Task<ReaderInfoDTO> GetInfoAboutReader(int id)
        {
            var reader = await _dataContext.Reader.FirstOrDefaultAsync(r => r.Id == id);
            if(reader == null)
            {
                throw new ObjectNotFoundException(nameof(Reader), id);
            }
            ReaderInfoDTO readerInfo = new ReaderInfoDTO
            { 
                Id = reader.Id,
                Name = reader.Name,
                LastName = reader.LastName,
                Address = reader.Address,
                PhoneNumber = reader.PhoneNumber

            };
            var books = await _dataContext.Book.Where(b => b.ReaderId == id).ToListAsync();
            if(books != null)
            {
                readerInfo.ListOfBooks = books.Select(x => _mapper.Map<BookDTO>(x)).ToList();
            }

            return readerInfo;
        }

        public async Task<List<ReaderDTO>> SearchReader(SearchReaderDTO searchReaderDTO)
        {
            if (searchReaderDTO?.SearchByText != null)
            {
                var readers = await _dataContext.Reader.Where(r =>
                    r.Name.ToLower().Contains(searchReaderDTO.SearchByText.ToLower()) ||
                    r.LastName.ToLower().Contains(searchReaderDTO.SearchByText.ToLower()) ||
                    r.Address.ToLower().Contains(searchReaderDTO.SearchByText.ToLower()) ||
                    r.PhoneNumber.ToLower().Contains(searchReaderDTO.SearchByText.ToLower())
                )
                .Select(r => _mapper.Map<ReaderDTO>(r)).ToListAsync();
               
                return readers;
            }
            return await _dataContext.Reader.Select(r => _mapper.Map<ReaderDTO>(r)).ToListAsync();

        }

        public async Task<List<ReaderDTO>> SortReader(SortReaderDTO sortDTO)
        {
            var reader =await  _dataContext.Reader.Select(x => _mapper.Map<ReaderDTO>(x)).ToListAsync();
            if(sortDTO.SortByNameAsc)
            {
                return reader.OrderBy(x => x.Name).ToList();
            }
            else if (sortDTO.SortByNameDesc)
            {
                return reader.OrderByDescending(x => x.Name).ToList();
            }
            else if(sortDTO.SortByLastNameAsc)
            {
                return reader.OrderBy(x => x.LastName).ToList();
            }
            else if(sortDTO.SortByLastNameDesc)
            {
                return reader.OrderByDescending(x => x.LastName).ToList();
            }

            return reader;
            
        }

        public async Task<ReaderDTO> UpdateReader(UpdateReaderDTO updateReaderDTO)
        {
            if(updateReaderDTO == null)
            {
                throw new ArgumentNullException(nameof(updateReaderDTO));
            }
            var reader = await _dataContext.Reader.FirstOrDefaultAsync(r => r.Id == updateReaderDTO.Id);
            if (reader == null)
            {
                throw new ObjectNotFoundException(nameof(Reader), updateReaderDTO.Id);
            }

            _mapper.Map(updateReaderDTO, reader);
            reader.LastModifiedBy = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name);
            reader.LastModifiedOn = DateTime.Now;
            _dataContext.Reader.Update(reader);
            await _dataContext.SaveChangesAsync();
            return _mapper.Map<ReaderDTO>(reader);
        }
    }
}
