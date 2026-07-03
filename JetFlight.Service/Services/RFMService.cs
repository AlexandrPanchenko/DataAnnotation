using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.RFM;
using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services
{
    public interface IRFMService
    {
        Task<RfmDto> CreateAsync(BaseRfmDto model);
        Task<PagedListDTO<RfmDto>> GetAllAsync(PagingDTO pagingDTO, RangeDTO<int>? amount, RangeDTO<int>? count, RangeDTO<int>? period);
        Task<RfmDto> GetAsync(int id);
        Task UpdateAsync(RfmDto model);
        Task DeleteAsync(int id);
        Task<List<RfmDto>> GetRFMByIdsAsync(string ids);
    }

    public class RFMService : IRFMService
    {
        private readonly IDataUnitOfWork _unitOfWork;

        public RFMService(IDataUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RfmDto> GetAsync(int id)
        {
            var rfm = await GetQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (rfm == null)
            {
                throw new ArgumentException("RFM не знайден");
            }

            var result = ToDto(rfm);

            return result;
        }

        public async Task<List<RfmDto>> GetRFMByIdsAsync(string ids)
        {
            var idList = ids.Split(',').Select(int.Parse).ToList();
            var rfms = await GetQuery()
                .AsNoTracking()
                .Where(x => idList.Contains(x.Id))
                .Select(x => ToDto(x))
                .ToListAsync();

            return rfms;
        }

        public async Task<PagedListDTO<RfmDto>> GetAllAsync(PagingDTO pagingDTO, RangeDTO<int>? amount, RangeDTO<int>? count, RangeDTO<int>? period)
        {
            var query = GetQuery()
                .AsNoTracking();

            if (amount != null)
            {
                query = query.Where(x =>
                    x.Amount.From >= amount.From
                    && x.Amount.To <= amount.To);
            }

            if (count != null)
            {
                query = query.Where(x =>
                    x.Count.From >= count.From
                    && x.Count.To <= count.To);
            }

            if (period != null)
            {
                query = query.Where(x =>
                    x.Period.From >= period.From
                    && x.Period.To <= period.To);
            }

            var rfms = await query.GetPagedListAsync(pagingDTO, ToDto);
            return rfms;
        }

        public async Task UpdateAsync(RfmDto model)
        {
            var rfm = await GetQuery().FirstOrDefaultAsync(x => x.Id == model.Id);

            if (rfm == null)
            {
                throw new ArgumentException("RFM не знайден");
            }

            SetRFM(model, rfm);
            await _unitOfWork.Save(true);
        }

        public async Task<RfmDto> CreateAsync(BaseRfmDto model)
        {
            var entity = SetRFM(model);
            await _unitOfWork.RFMs.Add(entity);
            await _unitOfWork.Save(true);

            return ToDto(entity);
        }

        private static RFM SetRFM(BaseRfmDto model, RFM? entity = null)
        {
            entity ??= new RFM();

            if (!IsRangeValid(model.Period))
            {
                throw new ArgumentException($"Діапазон {nameof(model.Period)} не валідний");
            }
            
            if (!IsRangeValid(model.Count))
            {
                throw new ArgumentException($"Діапазон {nameof(model.Count)} не валідний");
            }
            
            if (!IsRangeValid(model.Amount))
            {
                throw new ArgumentException($"Діапазон {nameof(model.Amount)} не валідний");
            }

            entity.Period = model.Period.ToDbModel(entity.Period);
            entity.Count = model.Count.ToDbModel(entity.Count);
            entity.Amount = model.Amount.ToDbModel(entity.Amount);
            entity.Color = model.Color;
            entity.Name = model.Name;

            return entity;
        }

        private static bool IsRangeValid(RangeDTO<int> range)
        {
            return range.From <= range.To && range.To > 0;
        }

        private static RfmDto ToDto(RFM x)
            => new RfmDto
            {
                Amount = x.Amount.ToDto(),
                Count = x.Count.ToDto(),
                Period = x.Period.ToDto(),
                Color = x.Color,
                Name = x.Name,
                Id = x.Id,
            };

        private IQueryable<RFM> GetQuery()
            => _unitOfWork.RFMs.GetAll().Include(x => x.Period).Include(x => x.Amount).Include(x => x.Count);

        public async Task DeleteAsync(int id)
        {
            var rfm = await _unitOfWork.RFMs.GetAll().FirstOrDefaultAsync(x => x.Id == id);

            if (rfm == null)
            {
                throw new ArgumentException("RFM не знайден");
            }

            if (await _unitOfWork.Targets.Any(x => x.RFMs.Any(r => r.RFMId == id)))
            {
                throw new ArgumentException("RFM є в одному чи більше таргетах");
            }

            _unitOfWork.RFMs.Remove(rfm);
            await _unitOfWork.Save(true);
        }
    }

    public static class RangeExtensions
    {
        public static RangeDTO<T> ToDto<T>(this Range<T> range)
            => new RangeDTO<T>
            {
                From = range.From,
                To = range.To,
            };

        public static Range<T> ToDbModel<T>(this RangeDTO<T> model, Range<T> existing)
        {
            existing ??= new Range<T>();

            existing.From = model.From;
            existing.To = model.To;

            return existing;
        }
    }
}
