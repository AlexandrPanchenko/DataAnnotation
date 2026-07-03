using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;

namespace JetFlight.Service.Services;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Shared.Models.LogHistory;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Exceptions;

public interface ISeoMetaService
{
    Task<SeoMetaDTO> Get(string entityType, int entityId);
    Task<SeoMetaDTO> GetById(int seoMetaId);
    Task<SeoMetaDTO> UpdateSeoMeta(SeoMetaDTO seoMetaDTO);
    Task<SeoMetaDTO> GetByEnum(string entityType, RootPage rootPage, int branchId);
}

public class SeoMetaService : ISeoMetaService
{
    private readonly IDataUnitOfWork _unitOfWork;
    // TODO: temp solution for messages. Refactor required
    private readonly string _pageNotFoundMessage = "Сторінка не знайдена";
    private readonly string _mainPageNotFoundMessage = "Головна сторінка не знайдена";
    public SeoMetaService(
        IDataUnitOfWork unitOfWork)
    {

        _unitOfWork = unitOfWork;

    }
    public async Task<SeoMetaDTO> GetByEnum(string entityType, RootPage rootPage, int branchId)
    {
        Page? page = null;
        if (entityType == EntityTypesConstant.Page)
        {
            var mainPage = await _unitOfWork.Pages.GetAllPages().Where(x => x.RootPage == rootPage && x.BranchId == branchId).FirstOrDefaultAsync()
     ?? throw new NotFoundException(_mainPageNotFoundMessage);
            page = mainPage.RootPage == RootPage.KeyValues
                ? mainPage.Origin
                : await _unitOfWork.Pages.GetAllPages().Where(x => x.ParentId == mainPage.Id && x.Published == true).FirstOrDefaultAsync()
                    ?? throw new NotFoundException(_pageNotFoundMessage);
        }

        var seoMeta = await _unitOfWork.SeoMeta.GetAll()
                .Where(x => x.EntityType == entityType && page != null && x.EntityId == page.Id)
                .FirstOrDefaultAsync();

        if (seoMeta == null) return new SeoMetaDTO();

        var seoMetaDto = new SeoMetaDTO
        {
            CanonicalUrl = seoMeta.CanonicalUrl,
            EntityId = seoMeta.EntityId != null ? seoMeta.EntityId.Value : 0,
            EntityType = seoMeta.EntityType,
            Description = seoMeta.Description,
            Keywords = seoMeta.Keywords,
            Title = seoMeta.Title,
        };

        return seoMetaDto;
    }

    public async Task<SeoMetaDTO> Get(string entityType, int entityId)
    {
        bool parentId = false;
        Page? existingPage = null;
        if (entityType == EntityTypesConstant.Page)
        {
            existingPage = await _unitOfWork.Pages.GetAll().FirstOrDefaultAsync(s => s.ParentId == entityId && s.Published == true);
            if (existingPage != null)
            {
                parentId = true;
            }
        }

        var seoMeta = parentId == true
            ? await _unitOfWork.SeoMeta.GetAll()
                .Where(x => x.EntityType == entityType && existingPage != null && x.EntityId == existingPage.Id)
                .FirstOrDefaultAsync()
            : await _unitOfWork.SeoMeta.GetAll()
                .Where(x => x.EntityType == entityType && x.EntityId == entityId)
                .FirstOrDefaultAsync();

        if (seoMeta == null) return new SeoMetaDTO();

        var seoMetaDto = new SeoMetaDTO
        {
            CanonicalUrl = seoMeta.CanonicalUrl,
            EntityId = seoMeta.EntityId != null ? seoMeta.EntityId.Value : 0,
            EntityType = seoMeta.EntityType,
            Description = seoMeta.Description,
            Keywords = seoMeta.Keywords,
            Title = seoMeta.Title,
        };

        return seoMetaDto;
    }

    public async Task<SeoMetaDTO> GetById(int seoMetaId)
    {
        var seoMeta = await _unitOfWork.SeoMeta.GetAll().Where(x => x.Id == seoMetaId).ToListAsync();
        if (seoMeta == null)
        {
            return new SeoMetaDTO();
        }

        return seoMeta
             .Select(seoMeta => new SeoMetaDTO
             {
                 CanonicalUrl = seoMeta.CanonicalUrl,
                 EntityId = seoMeta.EntityId != null ? seoMeta.EntityId.Value : 0,
                 EntityType = seoMeta.EntityType,
                 Description = seoMeta.Description,
                 Keywords = seoMeta.Keywords,
                 Title = seoMeta.Title,
                 UpdatedAt = seoMeta.UpdatedAt

             }).First();
    }

    public async Task<SeoMetaDTO> UpdateSeoMeta(SeoMetaDTO seoMetaDTO)
    {
        var existingSeo = await _unitOfWork.SeoMeta.GetAll().Where(x => x.EntityId == seoMetaDTO.EntityId).FirstOrDefaultAsync();

        if (existingSeo == null)
        {
            throw new Exception("SeoMeta not found");
        }

        if (!string.IsNullOrEmpty(seoMetaDTO.CanonicalUrl)) existingSeo.CanonicalUrl = seoMetaDTO.CanonicalUrl;
        if (!string.IsNullOrEmpty(seoMetaDTO.Description)) existingSeo.Description = seoMetaDTO.Description;
        if (!string.IsNullOrEmpty(seoMetaDTO.Title)) existingSeo.Title = seoMetaDTO.Title;
        if (!string.IsNullOrEmpty(seoMetaDTO.Keywords)) existingSeo.Keywords = seoMetaDTO.Keywords;
        existingSeo.UpdatedAt = DateTime.UtcNow.SetKindUtc();
        if (seoMetaDTO.EntityType == EntityTypesConstant.Page)
        {
            var existingPage = await _unitOfWork.Pages.GetAll().FirstOrDefaultAsync(s => s.Id == seoMetaDTO.EntityId);
            if (existingPage != null)
            {
                existingPage.UpdatedAt = DateTime.UtcNow.SetKindUtc();
                await _unitOfWork.Save();
            }
        }

        await _unitOfWork.Save();

        return new SeoMetaDTO
        {
            CanonicalUrl = existingSeo.CanonicalUrl,
            EntityId = existingSeo.EntityId != null ? existingSeo.EntityId.Value : 0,
            EntityType = existingSeo.EntityType,
            Description = existingSeo.Description,
            Keywords = existingSeo.Keywords,
            Title = existingSeo.Title,
            UpdatedAt = existingSeo?.UpdatedAt,
        };
    }

}

