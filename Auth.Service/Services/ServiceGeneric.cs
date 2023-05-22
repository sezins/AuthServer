using Auth.Core.Repository;
using Auth.Core.Service;
using Auth.Core.UnitofWork;
using Auth.Data.Repositpries;
using Auth.Data.UnitOfWork;
using SharedLibrary.Dtos;
using System.Collections.Immutable;
using System.Linq.Expressions;


namespace Auth.Service.Services
{
    public class ServiceGeneric<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IUnitofWork _unitOfWork;
        public ServiceGeneric(IGenericRepository<TEntity> genericRepository, IUnitofWork unitOfWork)
        {
            _genericRepository = genericRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<TDto>> AddAsync(TDto dto)
        {
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
            await _genericRepository.AddAsync(newEntity);
            await _unitOfWork.CommitAsync();
            var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
            return Response<TDto>.Success(newDto, 200);
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            var entityList = await _genericRepository.GetAllAsync();
            var dtoList = ObjectMapper.Mapper.Map<List<TDto>>(entityList);
            return Response<IEnumerable<TDto>>.Success(dtoList, 200);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return Response<TDto>.Fail("Id not found!", 404, true);
            }
            var dto = ObjectMapper.Mapper.Map<TDto>(entity);
            return Response<TDto>.Success(dto, 200);
        }

        public async Task<Response<NoDataDto>> RemoveAsync(int id)
        {
            var entity = await _genericRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return Response<NoDataDto>.Fail("Id not found!", 404, true);
            }
            _genericRepository.Remove(entity);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<NoDataDto>> UpdateAsync(TDto dto)
        {
            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
            _genericRepository.Update(updateEntity);
            await _unitOfWork.CommitAsync();
            //204 durum kodu => No Content => Response Body'sinde hiç bir data olmayacak.
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<IEnumerable<TDto>>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entityList = await _genericRepository.Where(predicate).ToListAsync();
            var dtoList = ObjectMapper.Mapper.Map<List<TDto>>(entityList);
            return Response<IEnumerable<TDto>>.Success(dtoList, 200);
        }
        }
    }
}
