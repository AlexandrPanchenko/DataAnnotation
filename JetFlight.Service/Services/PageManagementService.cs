using JetFlight.Service.Extensions;
using JetFlight.Shared.Models.LogHistory;

namespace JetFlight.Service.Services;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using Microsoft.AspNetCore.Http;
using JetFlight.Shared.Models.PageManagement;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using JetFlight.Shared.Models.Posts;
using System.Data;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Exceptions;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models;
using JetFlight.Shared.UserContext;
using Newtonsoft.Json;

public interface IPageManagementService
{
    Task<PageDTO> CreatePageWithSections(SubPageCreateRequest page);
    Task<GetPageResponse> GetAllSubPages(RootPage rootPage, int branchId);
    Task<List<GetSectionsResponse>> GetSectionsForPage(int pageId, bool isPublished = false, bool? parentPage = false);
    Task<SectionUpdateResponseDTO> UpdateSection(SectionUpdateRequest sectionDto);
    Task<List<PageDTO>> GetRootPages(int? branchId = null);
    Task<List<GetSectionsResponse>> GetPublishSectionsByPageEnum(RootPage rootPage, int branchId);

    Task<PageStatusUpdateResponseDTO> UpdateAllPageStatus();

    Task<PageStatusUpdateResponseDTO> UpdatePageStatus(PageStatusUpdateRequest pageStatusDto);
    Task<PageStatusUpdateResponseDTO> UpdatePublishPageStatus(PagePublishStatusUpdateRequest pageStatusDto);

    Task<PageUpdateResponseDTO> UpdatePageDetails(PageUpdateDTO pageDto);

    Task DeletePage(int pageId);

    Task<PageDTO> CopyPage(int pageId);

    Task<PageDTO> GetPage(int pageId);
    Task<PageDTO> GetPageByEnum(RootPage rootPage, int branchId);
    Task<List<GetSectionsResponse>> GetDraftHeaderFooterSectionsByPageEnum(RootPage rootPage, int branchId);

    Task<List<string>> GetPublishedPageLinks(byte branchId);
}

public class PageManagementService : IPageManagementService
{
    private readonly IDataUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IMediaService _mediaService;

    // TODO: temp solution for messages. Do refactor
    private readonly string _pageNotFoundMessage = "Сторінка не знайдена";
    private readonly string _mainPageNotFoundMessage = "Головна сторінка не знайдена";

    public PageManagementService(
        IDataUnitOfWork unitOfWork,
        IUserContext userContext,
        IMediaService mediaService)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;

        _mediaService = mediaService;
    }
    public async Task<PageDTO> CreatePageWithSections(SubPageCreateRequest page)
    {
        PageDTO newPage;

        switch (page.RootPage)
        {
            case RootPage.Home:
                newPage = await CreateHomePage(page);
                break;

            case RootPage.Policies:
                newPage = await CreatePolicySubPage(page);
                break;

            case RootPage.Offers:
                newPage = await CreateHolidayPromotionsPage(page);
                break;

            case RootPage.Bonuses:
                newPage = await CreateBonusSubPage(page);
                break;

            case RootPage.Vouchers:
                newPage = await CreateVaucherSubPage(page);
                break;

            case RootPage.Accumulation:
                newPage = await CreateAccamulationsSubPage(page);
                break;

            case RootPage.Cookies:
                newPage = await CreateCookiesSubPage(page);
                break;

            case RootPage.Charity:
                newPage = await CreateCharitySubPage(page);
                break;

            case RootPage.Contacts:
                newPage = await CreateContactSubPage(page);
                break;

            case RootPage.KeyValues:
                newPage = await CreateHeaderFooterPage(page);
                break;

            default:
                throw new NotFoundException(_pageNotFoundMessage);
        }

        return newPage;
    }

    public async Task<List<GetSectionsResponse>> GetDraftHeaderFooterSectionsByPageEnum(RootPage rootPage, int branchId)
    {
        var page = await _unitOfWork.Pages.GetAllPages().Where(x => x.RootPage == rootPage && x.BranchId == branchId).FirstOrDefaultAsync()
            ?? throw new NotFoundException(_pageNotFoundMessage);

        var sections = await _unitOfWork.Sections.GetAll().Where(x => x.PageId == page.Id).ToListAsync();
        var sectionPublishedDto = sections.AsEnumerable()
        .Select(section => new GetSectionsResponse
        {
            PageId = section.PageId,
            Position = section.Position,
            CreatedAt = section.CreatedAt,
            Id = section.Id,
            IsActive = section.IsActive,
            Name = section.Name,
            Title = section.Title,
            UpdatedAt = section.UpdatedAt,

            SectionsFields = _unitOfWork.SectionFields.GetAll().Where(x => x.SectionId == section.Id)
                .Select(sf => new GetSectionsFieldResponseDTO
                {
                    Id = sf.Id,
                    Key = sf.Key,
                    Value = sf.Value,
                    Placeholder = sf.Placeholder,
                    Title = sf.Title,
                    Type = sf.Type,
                    Dimensions = sf.Dimensions,
                    Extendable = sf.Extendable,
                    Position = sf.Position,
                    UpdatedAt = sf.UpdatedAt,
                    SectionId = section.Id,
                    SubSectionTitle = sf.SubSectionTitle ?? string.Empty,
                    RelatedTitle = sf.RelatedTitle,
                    CreatedAt = sf.CreatedAt,
                }).ToList()
        }).ToList();
        return sectionPublishedDto.OrderBy(x => x.Id).ToList();
    }

    public async Task<List<GetSectionsResponse>> GetPublishSectionsByPageEnum(RootPage rootPage, int branchId)
    {
        var mainPage = await _unitOfWork.Pages.GetAllPages().Where(x => x.RootPage == rootPage && x.BranchId == branchId).FirstOrDefaultAsync()
            ?? throw new NotFoundException(_mainPageNotFoundMessage);
        var page = mainPage.RootPage == RootPage.KeyValues
            ? mainPage.Origin
            : await _unitOfWork.Pages.GetAllPages().Where(x => x.ParentId == mainPage.Id && x.Published == true).FirstOrDefaultAsync()
                ?? throw new NotFoundException(_pageNotFoundMessage);

        var sections = await _unitOfWork.Sections.GetAllWithPage().Where(x => x.PageId == page.Id).ToListAsync();
        var sectionPublishedDto = sections.AsEnumerable()
            .Select(section => new GetSectionsResponse
            {
                PageId = section.PageId,
                Position = section.Position,
                CreatedAt = section.CreatedAt,
                Id = section.Id,
                IsActive = section.IsActive,
                Name = section.Name,
                Title = section.Title,
                UpdatedAt = section.UpdatedAt,
                Page = new PageDTO
                {
                    Id = section.Page.Id,
                    Name = section.Page.Name,
                    Title = section.Page.Title,
                    IsActive = section.Page.IsActive,
                    Published = section.Page.Published,
                    PublishedAt = section.Page.PublishedAt,
                    ScheduledPublishDate = section.Page.ScheduledPublishDate,
                    Link = section.Page.Link
                },

                SectionsFields = _unitOfWork.SectionFields.GetAll().Where(x => x.SectionId == section.Id)
                    .Select(sf => new GetSectionsFieldResponseDTO
                    {
                        Id = sf.Id,
                        Key = sf.Key,
                        Value = sf.Value,
                        Placeholder = sf.Placeholder,
                        Title = sf.Title,
                        Type = sf.Type,
                        Dimensions = sf.Dimensions,
                        Extendable = sf.Extendable,
                        Position = sf.Position,
                        UpdatedAt = sf.UpdatedAt,
                        SectionId = section.Id,
                        SubSectionTitle = sf.SubSectionTitle ?? string.Empty,
                        RelatedTitle = sf.RelatedTitle,
                        CreatedAt = sf.CreatedAt,
                    }).ToList()
            }).ToList();
        return sectionPublishedDto.OrderBy(x => x.Id).ToList();
    }

    public async Task<List<GetSectionsResponse>> GetSectionsForPage(int pageId, bool isPublished = false, bool? parentPage = false)
    {
        if (isPublished == true)
        {
            var draftpage = await _unitOfWork.Pages.GetAllPages().Where(x => x.Id == pageId && x.Published == false).FirstOrDefaultAsync()
                ?? throw new NotFoundException(_mainPageNotFoundMessage);
            var page = parentPage == false ? draftpage.Origin : await _unitOfWork.Pages.GetAllPages().Where(x => x.ParentId == pageId && x.Published == true).FirstOrDefaultAsync()
                ?? throw new NotFoundException(_pageNotFoundMessage);
            var sections = await _unitOfWork.Sections.GetAll().Where(x => x.PageId == page.Id).ToListAsync();
            var sectionPublishedDto = sections.AsEnumerable()
            .Select(section => new GetSectionsResponse
            {
                PageId = section.PageId,
                Position = section.Position,
                CreatedAt = section.CreatedAt,
                Id = section.Id,
                IsActive = section.IsActive,
                Name = section.Name,
                Title = section.Title,
                UpdatedAt = section.UpdatedAt,

                SectionsFields = _unitOfWork.SectionFields.GetAll().Where(x => x.SectionId == section.Id)
                    .Select(sf => new GetSectionsFieldResponseDTO
                    {
                        Id = sf.Id,
                        Key = sf.Key,
                        Value = sf.Value,
                        Placeholder = sf.Placeholder,
                        Title = sf.Title,
                        Type = sf.Type,
                        Dimensions = sf.Dimensions,
                        Extendable = sf.Extendable,
                        Position = sf.Position,
                        UpdatedAt = sf.UpdatedAt,
                        SectionId = section.Id,
                        SubSectionTitle = sf.SubSectionTitle ?? string.Empty,
                        RelatedTitle = sf.RelatedTitle,
                        CreatedAt = sf.CreatedAt,
                    }).ToList()
            }).ToList();
            return sectionPublishedDto.OrderBy(x => x.Id).ToList();
        }
        else
        {
            var sections = await _unitOfWork.Sections.GetAll().Where(x => x.PageId == pageId && (x.Page.Published == isPublished || x.Page.Published == null)).ToListAsync();
            var sectionDto = sections.AsEnumerable()
            .Select(section => new GetSectionsResponse
            {
                PageId = section.PageId,
                Position = section.Position,
                CreatedAt = section.CreatedAt,
                Id = section.Id,
                IsActive = section.IsActive,
                Name = section.Name,
                Title = section.Title,
                UpdatedAt = section.UpdatedAt,

                SectionsFields = _unitOfWork.SectionFields.GetAll().Where(x => x.SectionId == section.Id)
                .Select(sf => new GetSectionsFieldResponseDTO
                {
                    Id = sf.Id,
                    Key = sf.Key,
                    Value = sf.Value,
                    Placeholder = sf.Placeholder,
                    Title = sf.Title,
                    Type = sf.Type,
                    Dimensions = sf.Dimensions,
                    UpdatedAt = sf.UpdatedAt,
                    SectionId = section.Id,
                    Extendable = sf.Extendable,
                    Position = sf.Position,
                    SubSectionTitle = sf.SubSectionTitle ?? string.Empty,
                    CreatedAt = sf.CreatedAt,
                    RelatedTitle = sf.RelatedTitle,
                }).ToList()
            }).ToList();
            return sectionDto.OrderBy(x => x.Id).ToList(); ;
        }
    }

    public async Task<List<PageDTO>> GetRootPages(int? branchId = null)
    {
        var query = _unitOfWork.Pages.GetRootPages();

        if (branchId.HasValue)
        {
            query = query.Where(x => x.BranchId == branchId.Value);
        }

        var pages = await query.OrderBy(x=>x.Order).ToListAsync();

        var pagesDto = pages
        .Select(page => new PageDTO
        {
            Id = page.Id,
            IsActive = page.IsActive,
            Name = page.Name,
            RootPage = page.RootPage,
            Title = page.Title,
            NumberOfPages = _unitOfWork.Pages.GetAllSubPages(page.Id).Where(x => x.ParentId == page.Id && x.Published != true).Count()
        }).ToList();


        return pagesDto;
    }

    public async Task<GetPageResponse> GetAllSubPages(RootPage rootPage, int branchId)
    {
        var parentPage = await _unitOfWork.Pages.GetAllPages().Where(x => x.RootPage == rootPage && x.BranchId == branchId).FirstOrDefaultAsync()
            ?? throw new NotFoundException(_pageNotFoundMessage);
        var pages = await _unitOfWork.Pages.GetAllSubPages(parentPage.Id).Where(x => x.Published != true).ToListAsync();
        var pagesDto = pages.ToList()
        .Select(page => new PageDTO
        {
            Id = page.Id,
            Published = page.Origin != null ? page.Origin.Published : page.Published,
            Name = page.Name,
            NumberOfSections = page.Sections.Count,
            Title = page.Title,
            UpdatedAt = page.UpdatedAt,
            PublishedAt = page.PublishedAt,
            ScheduledPublishDate = page.ScheduledPublishDate,
            Link = page.Link,
            RootPage = page.RootPage,

        }).ToList();

        return new GetPageResponse
        {
            Page = pagesDto,
            Total = pages.Count,
            Title = parentPage.Name
        };
    }

    public async Task DeletePage(int pageId)
    {
        Page? page = await _unitOfWork.Pages.GetById(pageId);

        if (page is null)
        {
            throw new NotFoundException("Сторінку не знайдено");
        }

        if (page.Published == true || page.PublishedAt != null)
        {
            throw new BadRequestException("Неможливо видалити опубліковану сторінку");
        }

        if (page.RootPage is not null)
        {
            throw new BadRequestException("Неможливо видалити root сторінку");
        }

        _unitOfWork.Pages.Remove(page);
        await _unitOfWork.Save();
    }

    public async Task<PageDTO> CopyPage(int pageId)
    {
        Page? page = await _unitOfWork.Pages.GetAllPages()
            .Where(x => x.Id == pageId)
            .Include(x => x.Sections)
            .ThenInclude(x => x.SectionFields)
            .ThenInclude(x => x.MediaFiles)
            .FirstOrDefaultAsync();

        if (page is null)
        {
            throw new NotFoundException("Сторінку не знайдено");
        }

        if (page.RootPage is not null)
        {
            throw new BadRequestException("Неможливо копіювати root сторінку");
        }

        Page newPage = new Page
        {
            BranchId = page.BranchId,
            Published = false,
            OriginId = null,
            ParentId = page.ParentId,
            Name = page.Name + " (копія)",
            Title = page.Title,
            Link = page.Link,
            RootPage = null,
            IsActive = page.IsActive,
            Order = page.Order,
            PublishedAt = null,
            ScheduledPublishDate = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
        };

        newPage.Sections ??= new List<Section>();
        foreach (var section in page.Sections)
        {
            var newSection = new Section()
            {
                Name = section.Name,
                Title = section.Title,
                Position = section.Position,
                IsActive = section.IsActive,
                IsHtml = section.IsHtml,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
            };
            newSection.SectionFields ??= new List<SectionField>();

            foreach (var sectionField in section.SectionFields)
            {
                var newSectionField = new SectionField
                {
                    MediaFiles = await CopyFileForSectionField(sectionField.MediaFiles),
                    Key = sectionField.Key,
                    Value = sectionField.Value,
                    Type = sectionField.Type,
                    Title = sectionField.Title,
                    Position = sectionField.Position,
                    Extendable = sectionField.Extendable,
                    SubSectionTitle = sectionField.SubSectionTitle,
                    Placeholder = sectionField.Placeholder,
                    Dimensions = sectionField.Dimensions,
                    RelatedTitle = sectionField.RelatedTitle,
                };

                newSection.SectionFields.Add(newSectionField);
            }

            newPage.Sections.Add(newSection);
        }

        await _unitOfWork.Pages.Add(newPage);
        await _unitOfWork.Save(skipLogHistory: true);

        var seoMeta = await _unitOfWork.SeoMeta.GetAll().Where(x => x.EntityId == page.Id && x.EntityType == "page").FirstOrDefaultAsync();

        if (seoMeta is not null)
        {
            var newSeoMeta = new SeoMeta
            {
                EntityType = seoMeta.EntityType,
                EntityId = newPage.Id,
                Title = seoMeta.Title,
                Description = seoMeta.Description,
                Keywords = seoMeta.Keywords,
                CanonicalUrl = seoMeta.CanonicalUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.SeoMeta.Add(newSeoMeta);
        }

        var newPageLog = new LogHistory
        {
            Action = ActionConstant.Inserted,
            AdminId = _userContext.AdminId,
            Date = DateTime.UtcNow,
            EntityType = EntityTypesConstant.Log.Page,
            EntityId = newPage.Id,
            UpdatedTo = JsonConvert.SerializeObject(new PageLogHistoryDTO()
            {
                Id = newPage.Id,
                Name = newPage.Name,
            })
        };

        await _unitOfWork.LogsHistory.Add(newPageLog);
        await _unitOfWork.Save(true);

        return new PageDTO
        {
            Id = newPage.Id,
            Published = newPage.Published,
            Title = newPage.Title,
            Name = newPage.Name,
            Link = newPage.Link,
            NumberOfSections = newPage.Sections.Count,
            RootPage = newPage.RootPage,
            IsActive = newPage.IsActive,
            PublishedAt = newPage.PublishedAt,
            ScheduledPublishDate = newPage.ScheduledPublishDate,
            UpdatedAt = newPage.UpdatedAt
        };
    }

    private async Task<MediaFiles?> CopyFileForSectionField(MediaFiles? file)
    {
        if (file is null)
        {
            return null;
        }

        var newFilePath = await _mediaService.CopyAsync(file.Name);
        var newFileName = Path.GetFileName(newFilePath.ToString());
        var copy = new MediaFiles
        {
            MimeType = file.MimeType,
            Name = newFileName,
            Size = file.Size,
            Width = file.Width,
            Height = file.Height,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        return copy;
    }

    public async Task<PageDTO> GetPage(int pageId)
    {
        var page = await _unitOfWork.Pages.GetById(pageId);
        var pageDto = new PageDTO
        {
            Id = page.Id,
            Published = page.Origin != null ? page.Origin.Published : page.Published,
            IsActive = page.IsActive,
            Name = page.Name,
            NumberOfSections = page.Sections.Count,
            Title = page.Title,
            UpdatedAt = page.UpdatedAt,
            PublishedAt = page.PublishedAt,
            ScheduledPublishDate = page.ScheduledPublishDate,
            Link = page.Link
        };

        return pageDto;
    }

    public async Task<PageDTO> GetPageByEnum(RootPage rootPage, int branchId)
    {
        var page = await _unitOfWork.Pages.GetAllPages().Where(x => x.RootPage == rootPage && x.BranchId == branchId).FirstOrDefaultAsync()
            ?? throw new NotFoundException(_pageNotFoundMessage);
        var pageDto = new PageDTO
        {
            Id = page.Id,
            Published = page.Origin != null ? page.Origin.Published : page.Published,
            IsActive = page.IsActive,
            Name = page.Name,
            NumberOfSections = page.Sections.Count,
            Title = page.Title,
            UpdatedAt = page.UpdatedAt,
            PublishedAt = page.PublishedAt,
            ScheduledPublishDate = page.ScheduledPublishDate,
            Link = page.Link
        };

        return pageDto;
    }

    public async Task<PageStatusUpdateResponseDTO> UpdatePageStatus(PageStatusUpdateRequest pageStatusDto)
    {
        var existingPage = await _unitOfWork.Pages.GetAll().FirstOrDefaultAsync(s => s.Id == pageStatusDto.PageId) ?? throw new Exception("Page not found");
        existingPage.IsActive = pageStatusDto.IsActive;
        if (existingPage.Origin != null)
        {
            existingPage.Origin.IsActive = pageStatusDto.IsActive;
        }

        await _unitOfWork.Save();

        return new PageStatusUpdateResponseDTO
        {
            Item = new PageDTO { IsActive = pageStatusDto.IsActive }
        };
    }

    public async Task<PageStatusUpdateResponseDTO> UpdateAllPageStatus()
    {
        var pagesToPublish = await _unitOfWork.Pages.GetAll().Where(x => x.Published == false && x.ScheduledPublishDate.HasValue && x.ScheduledPublishDate < DateTime.UtcNow).ToListAsync();
        foreach (var item in pagesToPublish)
        {
            var publishRequest = new PagePublishStatusUpdateRequest { PageId = item.Id, Published = true };
            await UpdatePublishPageStatus(publishRequest);
        }


        return new PageStatusUpdateResponseDTO
        {
            Item = new PageDTO { Published = true }
        };
    }

    public async Task<PageUpdateResponseDTO> UpdatePageDetails(PageUpdateDTO pageDto)
    {
        var datetimenow = DateTime.UtcNow.SetKindUtc();
        var response = new PageUpdateResponseDTO();
        var existingPage = await _unitOfWork.Pages.GetAll().FirstOrDefaultAsync(s => s.Id == pageDto.Id);
        if (existingPage != null)
        {
            if (!string.IsNullOrEmpty(pageDto.Name)) existingPage.Name = pageDto.Name;

            if (!string.IsNullOrEmpty(pageDto.Title)) existingPage.Title = pageDto.Title;
            existingPage.ScheduledPublishDate = pageDto.ScheduledPublishDate;
            existingPage.UpdatedAt = datetimenow;

            await _unitOfWork.Save();

            response.Item = new PageUpdateDTO
            {
                Id = existingPage.Id,
                Name = existingPage.Name,
                Title = existingPage.Title
            };
        }
        else
        {
            response.Errors.Add("Запис не знайдено");
        }


        return response;
    }


    public async Task<PageStatusUpdateResponseDTO> UpdatePublishPageStatus(PagePublishStatusUpdateRequest pageStatusDto)
    {
        var existingPage = await GetExistingPage(pageStatusDto.PageId);
        var dateTimeNow = DateTime.UtcNow.SetKindUtc();

        if (pageStatusDto.Published)
        {
            await HandlePublishing(existingPage, dateTimeNow);
        }
        else
        {
            await HandleUnpublishing(existingPage);
        }

        await _unitOfWork.Save();

        return new PageStatusUpdateResponseDTO
        {
            Item = new PageDTO { Id = existingPage.Id }
        };
    }

    private async Task<Page> GetExistingPage(int pageId)
    {
        return await _unitOfWork.Pages.GetAllPages().FirstOrDefaultAsync(s => s.Id == pageId)
               ?? throw new NotFoundException(_pageNotFoundMessage);
    }

    private async Task HandlePublishing(Page existingPage, DateTime dateTimeNow)
    {
        await UnpublishDraftSubPages(existingPage);

        if (existingPage.OriginId != null)
        {
            await UpdateOriginPage(existingPage, dateTimeNow);
        }
        else
        {
            await CreateNewPublishedPage(existingPage, dateTimeNow);
        }

        await CopySeoMeta(existingPage, dateTimeNow);
    }

    private async Task UnpublishDraftSubPages(Page existingPage)
    {
        var draftSubPages = await _unitOfWork.Pages.GetAllPages()
            .Where(x => x.ParentId == existingPage.ParentId && x.OriginId != null && x.Id != existingPage.Id && x.BranchId == existingPage.BranchId)
            .ToListAsync();

        foreach (var page in draftSubPages)
        {
            if (page.Origin != null)
            {
                var sectionsToRemove = await _unitOfWork.Sections.GetAll().Where(x => x.PageId == page.Origin.Id).ToListAsync();
                foreach (var section in sectionsToRemove)
                {
                    var sectionFieldsToRemove = await _unitOfWork.SectionFields.GetAll().Where(x => x.SectionId == section.Id).ToListAsync();
                    sectionFieldsToRemove.ForEach(sf => _unitOfWork.SectionFields.Remove(sf));
                }
                sectionsToRemove.ForEach(s => _unitOfWork.Sections.Remove(s));

                _unitOfWork.Pages.Remove(page.Origin);
                page.Origin = null;
                page.PublishedAt = null;
            }
        }

        await _unitOfWork.Save();
    }

    private async Task UpdateOriginPage(Page existingPage, DateTime dateTimeNow)
    {
        var originPage = await GetOriginPage(existingPage);

        if (originPage != null)
        {
            RemoveUnmatchedSectionFields(originPage, existingPage);
            await _unitOfWork.Save();
            var oldPublishedTime = existingPage.PublishedAt;
            UpdatePageDetails(originPage, existingPage, dateTimeNow);
            await _unitOfWork.Save(skipLogHistory: true);
            await _unitOfWork.LogsHistory.Add(new LogHistory
            {
                AdminId = _userContext.AdminId,
                EntityType = EntityTypesConstant.Log.Page,
                UpdatedFrom = JsonConvert.SerializeObject(new PageLogHistoryDTO()
                {
                    Id = existingPage.Id,
                    PublishedAt = oldPublishedTime,
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }),
                UpdatedTo = JsonConvert.SerializeObject(new PageLogHistoryDTO()
                {
                    Id = existingPage.Id,
                    PublishedAt = existingPage.PublishedAt
                }, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                }),
                EntityId = existingPage.Id,
                Action = ActionConstant.Updated,
                Date = DateTime.UtcNow,
            });

            await UpdateSectionsAndFields(existingPage, originPage, dateTimeNow);
        }
    }

    private async Task<Page?> GetOriginPage(Page existingPage)
    {
        return await _unitOfWork.Pages.GetAllPages()
            .FirstOrDefaultAsync(x => x.Id == existingPage.OriginId && x.BranchId == existingPage.BranchId);
    }

    private void RemoveUnmatchedSectionFields(Page originPage, Page existingPage)
    {
        foreach (var publishedSection in originPage.Sections)
        {
            var draftSection = existingPage.Sections.FirstOrDefault(ds => ds.Name == publishedSection.Name);
            if (draftSection != null)
            {
                var publishedSectionFields = publishedSection.SectionFields;
                var draftSectionFieldIds = draftSection.SectionFields.Select(sf => sf.Id).ToList();

                var sectionFieldsToRemove = publishedSectionFields.Where(sf => !draftSectionFieldIds.Contains(sf.Id)).ToList();
                foreach (var sectionFieldToRemove in sectionFieldsToRemove)
                {
                    _unitOfWork.SectionFields.Remove(sectionFieldToRemove);
                }
            }
        }
    }

    private async Task UpdateSectionsAndFields(Page existingPage, Page originPage, DateTime dateTimeNow)
    {
        foreach (var section in existingPage.Sections)
        {
            var existingPublishedSection = originPage.Sections.FirstOrDefault(s => s.PageId == originPage.Id && section.Name == s.Name);
            if (existingPublishedSection != null)
            {
                UpdateSectionDetails(existingPublishedSection, section, dateTimeNow);
                await _unitOfWork.Save();

                foreach (var field in section.SectionFields)
                {
                    var existingPublishedSectionField = existingPublishedSection.SectionFields.FirstOrDefault(s => s.Key == field.Key && s.Id != field.Id);
                    if (existingPublishedSectionField != null)
                    {
                        UpdateSectionFieldDetails(existingPublishedSectionField, field, dateTimeNow);
                        await _unitOfWork.Save(true);
                    }
                    else
                    {
                        await AddNewSectionField(existingPublishedSection.Id, field, dateTimeNow);
                    }
                }
            }
            await _unitOfWork.Save();
        }
    }

    private async Task CreateSectionUpdateLogHistory(Section section, Section oldSection, List<SectionFieldLogHistoryDTO> oldSectionFields, List<SectionFieldLogHistoryDTO> newSectionFields)
    {
        List<LogHistoryItem<SectionFieldLogHistoryDTO, int>> updates = oldSectionFields
            .FullOuterJoin(newSectionFields, x => x.Id ?? 0)
            .Where(x =>! x.Left.IsEqualTo(x.Right))
            .ToList();

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        var logHistoryItem = new LogHistory
        {
            Action = ActionConstant.Updated,
            AdminId = _userContext.AdminId,
            Date = DateTime.UtcNow,
            EntityType = EntityTypesConstant.Log.Sections,
            EntityId = section.PageId,
            UpdatedFrom = JsonConvert.SerializeObject(new SectionsLogHistoryDTO
            {
                Id = section.Id,
                PageId = section.PageId,
                Name = section.Name,
                Title = section.Title,
                SectionFields = updates.Where(x => x.Left != null).Select(x => x.Left!).ToList(),
            }, settings),
            UpdatedTo = JsonConvert.SerializeObject(new SectionsLogHistoryDTO
            {
                Id = oldSection.Id,
                PageId = oldSection.PageId,
                Name = oldSection.Name,
                Title = oldSection.Title,
                SectionFields = updates.Where(x => x.Right != null).Select(x => x.Right!).ToList()
            }, settings)
        };

        await _unitOfWork.LogsHistory.Add(logHistoryItem);
        await _unitOfWork.Save(true);
    }

    private void UpdatePageDetails(Page originPage, Page existingPage, DateTime dateTimeNow)
    {
        originPage.Name = existingPage.Name;
        originPage.Title = existingPage.Title;
        originPage.Link = existingPage.Link;
        originPage.BranchId = existingPage.BranchId;
        originPage.UpdatedAt = dateTimeNow;
        originPage.PublishedAt = dateTimeNow;
        originPage.ScheduledPublishDate = null;
        existingPage.UpdatedAt = dateTimeNow;
        existingPage.PublishedAt = dateTimeNow;
        existingPage.ScheduledPublishDate = null;
        originPage.Published = true;
    }

    private void UpdateSectionDetails(Section existingPublishedSection, Section section, DateTime dateTimeNow)
    {
        existingPublishedSection.Name = section.Name;
        existingPublishedSection.Title = section.Title;
        existingPublishedSection.Position = section.Position;
        existingPublishedSection.IsHtml = section.IsHtml;
        existingPublishedSection.UpdatedAt = dateTimeNow;
    }

    private void UpdateSectionFieldDetails(SectionField existingPublishedSectionField, SectionField field, DateTime dateTimeNow)
    {
        existingPublishedSectionField.Value = field.Value;
        existingPublishedSectionField.Title = field.Title;
        existingPublishedSectionField.Position = field.Position;
        existingPublishedSectionField.Placeholder = field.Placeholder;
        existingPublishedSectionField.Dimensions = field.Dimensions;
        existingPublishedSectionField.Type = field.Type;
        existingPublishedSectionField.Extendable = field.Extendable;
        existingPublishedSectionField.SubSectionTitle = field.SubSectionTitle;
        existingPublishedSectionField.UpdatedAt = dateTimeNow;
        existingPublishedSectionField.RelatedTitle = field.RelatedTitle;
    }

    private async Task AddNewSectionField(int sectionId, SectionField field, DateTime dateTimeNow)
    {
        var newSectionField = new SectionField
        {
            Value = field.Value,
            Title = field.Title,
            Position = field.Position,
            Placeholder = field.Placeholder,
            Extendable = field.Extendable,
            SubSectionTitle = field.SubSectionTitle,
            Dimensions = field.Dimensions,
            CreatedAt = dateTimeNow,
            UpdatedAt = dateTimeNow,
            Key = field.Key,
            SectionId = sectionId,
            Type = field.Type,
            RelatedTitle = field.RelatedTitle,
        };

        await _unitOfWork.SectionFields.Add(newSectionField);
        await _unitOfWork.Save(true);
    }

    private async Task CreateNewPublishedPage(Page existingPage, DateTime dateTimeNow)
    {
        var newPage = new Page
        {
            Published = true,
            PublishedAt = dateTimeNow,
            Name = existingPage.Name,
            IsActive = existingPage.IsActive,
            Title = existingPage.Title,
            BranchId = existingPage.BranchId,
            CreatedAt = dateTimeNow,
            Link = existingPage.Link,
            ParentId = existingPage.ParentId,
            ScheduledPublishDate = null,
            RootPage = null
        };

        var res = await _unitOfWork.Pages.Add(newPage);
        await _unitOfWork.Save(skipLogHistory: true);
        existingPage.OriginId = res.Id;
        existingPage.PublishedAt = dateTimeNow;
        existingPage.ScheduledPublishDate = null;

        foreach (var section in existingPage.Sections)
        {
            var newSection = new Section
            {
                PageId = newPage.Id,
                Name = section.Name,
                Title = section.Title,
                Position = section.Position,
                IsHtml = section.IsHtml,
                SectionFields = new List<SectionField>()
            };

            foreach (var field in section.SectionFields)
            {
                newSection.SectionFields.Add(new SectionField
                {
                    Title = field.Title,
                    Key = field.Key,
                    MediaFilesId = field.MediaFilesId,
                    Type = field.Type,
                    Placeholder = field.Placeholder,
                    SubSectionTitle = field.SubSectionTitle,
                    Dimensions = field.Dimensions,
                    Value = field.Value,
                    Extendable = field.Extendable,
                    Position = field.Position,
                    RelatedTitle = field.RelatedTitle,
                });
            }
            newPage.Sections.Add(newSection);
        }
        await _unitOfWork.Save();
    }

    private async Task CopySeoMeta(Page existingPage, DateTime dateTimeNow)
    {
        var seoMeta = await _unitOfWork.SeoMeta.GetAll().Where(x => x.EntityId == existingPage.Id).FirstOrDefaultAsync();
        if (seoMeta != null && existingPage.OriginId != null)
        {
            var originSeoMeta = await _unitOfWork.SeoMeta.GetAll().Where(x => x.EntityId == existingPage.OriginId).FirstOrDefaultAsync();

            if (originSeoMeta != null)
            {
                originSeoMeta.CanonicalUrl = seoMeta.CanonicalUrl;
                originSeoMeta.Description = seoMeta.Description;
                originSeoMeta.Title = seoMeta.Title;
                originSeoMeta.Keywords = seoMeta.Keywords;
                originSeoMeta.UpdatedAt = seoMeta.UpdatedAt;
                originSeoMeta.EntityType = seoMeta.EntityType;
                originSeoMeta.EntityId = existingPage.OriginId;
                await _unitOfWork.Save();
            }
            else
            {
                var newSeoMeta = new SeoMeta
                {
                    CanonicalUrl = seoMeta.CanonicalUrl,
                    Description = seoMeta.Description,
                    Title = seoMeta.Title,
                    Keywords = seoMeta.Keywords,
                    UpdatedAt = seoMeta.UpdatedAt,
                    EntityType = seoMeta.EntityType,
                    EntityId = existingPage.OriginId,
                    CreatedAt = dateTimeNow,
                };

                await _unitOfWork.SeoMeta.Add(newSeoMeta);
                await _unitOfWork.Save();
            }
        }
    }

    private async Task HandleUnpublishing(Page existingPage)
    {
        if (existingPage.Origin != null)
        {
            var recordToDelete = await _unitOfWork.Pages.GetAllPages().Where(x => x.Id == existingPage.OriginId).FirstOrDefaultAsync();

            if (recordToDelete != null)
            {
                var sectionsToRemove = await _unitOfWork.Sections.GetAll().Where(x => x.PageId == recordToDelete.Id).ToListAsync();

                foreach (var section in sectionsToRemove)
                {
                    var sectionFieldsToRemove = _unitOfWork.SectionFields.GetAll().Where(x => x.SectionId == section.Id).ToList();
                    sectionFieldsToRemove.ForEach(sf => _unitOfWork.SectionFields.Remove(sf));
                }
                sectionsToRemove.ForEach(s => _unitOfWork.Sections.Remove(s));
                _unitOfWork.Pages.Remove(recordToDelete);

                existingPage.Origin = null;
                existingPage.PublishedAt = null;
                await _unitOfWork.Save();
            }
        }
    }

    public async Task<SectionUpdateResponseDTO> UpdateSection(SectionUpdateRequest sectionDto)
    {
        var response = new SectionUpdateResponseDTO();
        var sectionToUpdate = await _unitOfWork.Sections.GetAll().Where(x => x.Id == sectionDto.id).FirstOrDefaultAsync();

        if (sectionToUpdate == null)
        {
            response.Errors.Add("Секція не знайдена");
            return response;
        }

        var oldSection = new Section()
        {
            Name = sectionToUpdate.Name,
            Title = sectionToUpdate.Title,
            Id = sectionToUpdate.Id,
            PageId = sectionToUpdate.PageId,
        };

        sectionToUpdate.UpdatedAt = DateTime.UtcNow.SetKindUtc();
        sectionToUpdate.Name = sectionDto.name;
        await _unitOfWork.Save();

        var existingSectionFields = await _unitOfWork.SectionFields.GetAll().Where(s => s.SectionId == sectionDto.id).ToListAsync();
        var oldFieldsDto = existingSectionFields.Select(ToLogHistoryDto).ToList();
        var sectionFieldIdsToUpdate = sectionDto.sectionFields.Select(sf => sf.id).ToList();
        var newSectionFields = new List<SectionField>();

        // Remove SectionFields that are not in the sectionDto
        var sectionFieldsToRemove = existingSectionFields.Where(sf => !sectionFieldIdsToUpdate.Contains(sf.Id)).ToList();
        foreach (var sectionFieldToRemove in sectionFieldsToRemove)
        {
            _unitOfWork.SectionFields.Remove(sectionFieldToRemove);
        }
        await _unitOfWork.Save();
        bool SectionUpdated = false;
        foreach (var sectionFieldDto in sectionDto.sectionFields)
        {
            var existingSectionField = await _unitOfWork.SectionFields.GetAll().FirstOrDefaultAsync(s => s.Key == sectionFieldDto.key && s.Id == sectionFieldDto.id);

            if (existingSectionField != null)
            {
                existingSectionField.Value = sectionFieldDto.value;
                existingSectionField.Title = sectionFieldDto.title;
                existingSectionField.Position = sectionFieldDto.position;
                existingSectionField.Title = sectionFieldDto.title;
                existingSectionField.Placeholder = sectionFieldDto.placeholder;
                existingSectionField.Type = sectionFieldDto.type;
                if (sectionFieldDto.extendable.HasValue) existingSectionField.Extendable = sectionFieldDto.extendable;
                if (!string.IsNullOrEmpty(sectionFieldDto.subSectionTitle)) existingSectionField.SubSectionTitle = sectionFieldDto.subSectionTitle;
                if (!string.IsNullOrEmpty(sectionFieldDto.relatedTitle)) existingSectionField.RelatedTitle = sectionFieldDto.relatedTitle;
                existingSectionField.UpdatedAt = DateTime.UtcNow.SetKindUtc();
                await _unitOfWork.Save();
                SectionUpdated = true;
                if (sectionFieldDto.file != null)
                {
                    await AddedFileForSectionField(sectionFieldDto.file, existingSectionField);
                }
                newSectionFields.Add(existingSectionField);
            }
            else
            {
                var newSectionField = new SectionField
                {
                    Value = sectionFieldDto.value,
                    Title = sectionFieldDto.title,
                    Position = sectionFieldDto.position,
                    Extendable = sectionFieldDto.extendable ?? false,
                    SubSectionTitle = sectionFieldDto.subSectionTitle,
                    CreatedAt = DateTime.UtcNow.SetKindUtc(),
                    UpdatedAt = DateTime.UtcNow.SetKindUtc(),
                    Key = sectionFieldDto.key,
                    Dimensions = sectionFieldDto.dimensions,
                    SectionId = sectionDto.id,
                    Placeholder = sectionFieldDto.placeholder,
                    Type = sectionFieldDto.type,
                    RelatedTitle = sectionFieldDto.relatedTitle,
                };
                SectionUpdated = true;
                var result = await _unitOfWork.SectionFields.Add(newSectionField);
                await _unitOfWork.Save();

                if (sectionFieldDto.file != null)
                {
                    await AddedFileForSectionField(sectionFieldDto.file, result);
                }
                newSectionFields.Add(result);
            }
        }
        if (SectionUpdated)
        {

            
            var newFieldsDto = newSectionFields.Select(ToLogHistoryDto).ToList();

            await CreateSectionUpdateLogHistory(sectionToUpdate, oldSection, oldFieldsDto, newFieldsDto);
            SectionUpdated = false;
        }

        if (sectionToUpdate.PageId != null)
        {
            await UpdatePageAfterSectionChanges(sectionToUpdate.PageId);
        }

        return new SectionUpdateResponseDTO
        {
            Item = new SectionDTO
            {
                Id = sectionToUpdate.Id,
                PageId = sectionToUpdate.PageId,
                Name = sectionToUpdate.Name,
                Title = sectionToUpdate.Title,
                Position = sectionToUpdate.Position,
                CreatedAt = sectionToUpdate.CreatedAt,
                UpdatedAt = sectionToUpdate.UpdatedAt,
                SectionFields = sectionToUpdate.SectionFields.Select(sf => new SectionFieldDTO
                {
                    Id = sf.Id,
                    SectionId = sf.SectionId ?? 0,
                    Title = sf.Title,
                    Key = sf.Key,
                    MediaFilesId = sf.MediaFilesId,
                    Type = sf.Type,
                    Placeholder = sf.Placeholder,
                    SubSectionTitle = sf.SubSectionTitle ?? string.Empty,
                    Value = sf.Value,
                    Extendable = sf.Extendable,
                    Dimensions = sf.Dimensions,
                    Position = sf.Position,
                    CreatedAt = sf.CreatedAt,
                    UpdatedAt = sf.UpdatedAt,
                    RelatedTitle = sf.RelatedTitle,
                }).ToList()
            }
        };
    }

    private static SectionFieldLogHistoryDTO ToLogHistoryDto(SectionField sectionField)
    {
        return new SectionFieldLogHistoryDTO
        {
            Id = sectionField.Id,
            SectionId = sectionField.SectionId,
            MediaFilesId = sectionField.MediaFilesId,
            Key = sectionField.Key,
            Value = sectionField.Value,
            Type = sectionField.Type,
            Title = sectionField.Title,
            Position = sectionField.Position,
            Extendable = sectionField.Extendable,
            SubSectionTitle = sectionField.SubSectionTitle,
            Placeholder = sectionField.Placeholder,
            RelatedTitle = sectionField.RelatedTitle,
        };
    }

    private async Task AddedFileForSectionField(IFormFile? file, SectionField sectionField)
    {
        var newFilePath = await _mediaService.UploadAsync(file);
        var newFileName = Path.GetFileName(newFilePath.ToString());

        var media = new MediaFiles
        {
            MimeType = file.ContentType,
            CreatedAt = DateTime.UtcNow.SetKindUtc(),
            Name = newFileName,
            Size = file.Length.ToString()
        };

        // Save the MediaFiles entity to the database
        var result = await _unitOfWork.MediaFiles.Add(media);
        await _unitOfWork.Save(true);

        // Update the SectionField with the MediaFilesId and the URL of the saved file
        sectionField.MediaFilesId = result.Id;
        sectionField.Value = newFilePath.ToString();

        await _unitOfWork.Save(true);
    }

    private async Task UpdatePageAfterSectionChanges(int? pageId)
    {
        var existingPage = await _unitOfWork.Pages.GetAll().FirstOrDefaultAsync(s => s.Id == pageId)
            ?? throw new NotFoundException(_pageNotFoundMessage);
        existingPage.UpdatedAt = DateTime.UtcNow.SetKindUtc();
        await _unitOfWork.Save(true);
    }

    private async Task<Page> GetRootPage(RootPage rootPage, int branchId)
    {
        var page = await _unitOfWork.Pages.GetAllPages().Where(x => x.RootPage == rootPage && x.BranchId == branchId).FirstOrDefaultAsync()
            ?? throw new NotFoundException(_pageNotFoundMessage);

        return page;
    }

    private async Task<Page> CreatePage(SubPageCreateRequest subPage, int branchId, Page page)
    {
        var newPage = new Page
        {
            Published = false,
            Name = subPage.Name,
            ParentId = page.Id,
            CreatedAt = DateTime.UtcNow.SetKindUtc(),
            Link = page.Link,
            BranchId = (byte)branchId,
            ScheduledPublishDate = subPage.ScheduledPublishDate,
            Title = subPage.Title,
        };
        await _unitOfWork.Pages.Add(newPage);
        await _unitOfWork.Save(skipLogHistory: true);
        var newPageLog = new LogHistory
        {
            Action = ActionConstant.Inserted,
            AdminId = _userContext.AdminId,
            Date = DateTime.UtcNow,
            EntityType = EntityTypesConstant.Log.Page,
            EntityId = newPage.Id,
            UpdatedTo = JsonConvert.SerializeObject(new PageLogHistoryDTO()
            {
                Id = newPage.Id,
                Name = newPage.Name,
            })
        };

        await _unitOfWork.LogsHistory.Add(newPageLog);
        await _unitOfWork.Save(skipLogHistory: true);

        return newPage;
    }

    private async Task<PageDTO> CreatePolicySubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
        {
            new Section
            {
                PageId = newPage.Id,
                Name = "Політика приватності",
                Title = "Policy",
                Position = 1,
                IsActive = true,
                IsHtml = false,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                SectionFields = new List<SectionField>
        {
            new SectionField { Key = "policy.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "policy.data.text", Value = null, Placeholder ="Введіть дані", Title ="Контет сторінки полісі", Type="html", Position=1 },
        }
    }
            };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);
        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateHeaderFooterPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        if (newPage == null)
        {
            throw new Exception("Page not found");
        }

        var newSections = new List<Section>
   {
    new Section
    {
        PageId = newPage.Id,
        Name = "Хедер",
        Title = "Header",
        Position = 1,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "header.logo.link", Value = null, Placeholder = "Логотип", Title = "Логотип", Type = "image", Position = 0, Dimensions = "125x29" },
            new SectionField { Key = "header.link1.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №1", Position = 1},
            new SectionField { Key = "header.link1.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", Position = 2, RelatedTitle = "Посилання №1" },
            new SectionField { Key = "header.link1.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", Position = 3, RelatedTitle = "Посилання №1" },
            new SectionField { Key = "header.link2.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №2", Position = 4},
            new SectionField { Key = "header.link2.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", Position = 5 , RelatedTitle = "Посилання №2"},
            new SectionField { Key = "header.link2.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", Position = 6, RelatedTitle = "Посилання №2" },
            new SectionField { Key = "header.link3.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №3", Position = 7},
            new SectionField { Key = "header.link3.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", Position = 8, RelatedTitle = "Посилання №3"},
            new SectionField { Key = "header.link3.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", Position = 9, RelatedTitle = "Посилання №3"},
            new SectionField { Key = "header.link4.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №4", Position = 10 },
            new SectionField { Key = "header.link4.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", Position = 11, RelatedTitle = "Посилання №4"},
            new SectionField { Key = "header.link4.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", Position = 12, RelatedTitle = "Посилання №4" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Футер",
        Title = "Footer",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "footer.block1.status.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", SubSectionTitle = "Колонка №1", Position = 13, RelatedTitle = "Колонка №1" },
            new SectionField { Key = "footer.block1.block.name", Value = null, Placeholder = "Введіть назву", Title = "Назва колонки", SubSectionTitle = "Колонка №1", Position = 14 },
            new SectionField { Key = "footer.block1.link1.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №1", SubSectionTitle = "Колонка №1", Position = 15},
            new SectionField { Key = "footer.block1.link1.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №1", Position = 16, RelatedTitle = "Посилання №1" },
            new SectionField { Key = "footer.block1.link2.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №2", SubSectionTitle = "Колонка №1", Position = 17 },
            new SectionField { Key = "footer.block1.link2.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №1", Position = 18 , RelatedTitle = "Посилання №2"},
            new SectionField { Key = "footer.block1.link3.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №3", SubSectionTitle = "Колонка №1", Position = 19 },
            new SectionField { Key = "footer.block1.link3.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №1", Position = 20, RelatedTitle = "Посилання №3"},
            new SectionField { Key = "footer.block1.link4.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №4", SubSectionTitle = "Колонка №1", Position = 21 },
            new SectionField { Key = "footer.block1.link4.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №1", Position = 22, RelatedTitle = "Посилання №4" },

            new SectionField { Key = "footer.block2.status.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", SubSectionTitle = "Колонка №2", Position = 23, RelatedTitle = "Колонка №2" },
            new SectionField { Key = "footer.block2.block.name", Value = null, Placeholder = "Введіть назву", Title = "Назва колонки", SubSectionTitle = "Колонка №2", Position = 24 },
            new SectionField { Key = "footer.block2.link1.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №1", SubSectionTitle = "Колонка №2", Position = 25 },
            new SectionField { Key = "footer.block2.link1.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №2", Position = 26, RelatedTitle  = "Посилання №1"},
            new SectionField { Key = "footer.block2.link2.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №2", SubSectionTitle = "Колонка №2", Position = 27 },
            new SectionField { Key = "footer.block2.link2.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №2", Position = 28, RelatedTitle = "Посилання №2"},
            new SectionField { Key = "footer.block2.link3.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №3", SubSectionTitle = "Колонка №2", Position = 29 },
            new SectionField { Key = "footer.block2.link3.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №2", Position = 30, RelatedTitle = "Посилання №3"},
            new SectionField { Key = "footer.block2.link4.name", Value = null, Placeholder = "Назва посилання", Title = "Посилання №4", SubSectionTitle = "Колонка №2", Position = 31 },
            new SectionField { Key = "footer.block2.link4.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №2", Position = 32, RelatedTitle = "Посилання №4" },

            new SectionField { Key = "footer.block3.status.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", SubSectionTitle = "Колонка №3", Position = 33 , RelatedTitle = "Колонка №3"},
            new SectionField { Key = "footer.block3.block.name", Value = null, Placeholder = "Введіть назву", Title = "Назва колонки", SubSectionTitle = "Колонка №3", Position = 34 },
            new SectionField { Key = "footer.block3.link1.image", Value = null, Placeholder = "", Title = "Іконка для посилання", Type = "image", SubSectionTitle = "Колонка №3", Position = 35, Extendable = true, Dimensions = "24x24" },
            new SectionField { Key = "footer.block3.link1.name", Value = null, Placeholder = "Назва посилання", Title = "Назва посилання", SubSectionTitle = "Колонка №3", Position = 36, Extendable = true },
            new SectionField { Key = "footer.block3.link1.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Колонка №3", Position = 37, Extendable = true },


            new SectionField { Key = "footer.subscribe.status.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", SubSectionTitle = "Колонка з підпискою", Position = 38, RelatedTitle = "Колонка з підпискою" },
            new SectionField { Key = "footer.subscribe.block.logo", Value = null, Placeholder = "Логотип", Title = "Логотип", Type = "image", SubSectionTitle = "Колонка з підпискою", Position = 39, Dimensions = "125x29" },
            new SectionField { Key = "footer.subscribe.block.name", Value = null, Placeholder = "Введіть назву", Title = "Назва колонки", SubSectionTitle = "Колонка з підпискою", Position = 40 },
            new SectionField { Key = "footer.subscribe.button.name", Value = null, Placeholder = "Введіть текст кнопки", Title = "Текст кнопки", SubSectionTitle = "Колонка з підпискою", Position = 41 },

            new SectionField { Key = "footer.policy.status.isActive", Value = "true", Placeholder = "", Title = "", Type = "boolean", SubSectionTitle = "Додаткові інпути", Position = 42 , RelatedTitle = "Додаткові інпути"},
            new SectionField { Key = "footer.policy.block.name", Value = null, Placeholder = "Введіть текст авторського права", Title = "Авторське право", SubSectionTitle = "Додаткові інпути", Position = 43 },
            new SectionField { Key = "footer.policy.link1.name", Value = null, Placeholder = "Вставте посилання", Title = "Політика приватності", SubSectionTitle = "Додаткові інпути", Position = 44 },
            new SectionField { Key = "footer.policy.link1.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Додаткові інпути", Position = 45, RelatedTitle = "Політика приватності"},
            new SectionField { Key = "footer.policy.link2.name", Value = null, Placeholder = "Вставте посилання", Title = "Політика кукіс", SubSectionTitle = "Додаткові інпути", Position = 46 },
            new SectionField { Key = "footer.policy.link2.link", Value = null, Placeholder = "Вставте посилання", Title = "Посилання", SubSectionTitle = "Додаткові інпути", Position = 47, RelatedTitle = "Політика кукіс"}

        }
    }
};

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);
        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateCharitySubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
{
  new Section
    {
        PageId = newPage.Id,
        Name = "Хіро секція",
        Title = "Hero",
        Position = 1,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "hero.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "hero.subtitle.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок сторінки", Type = "textarea", Position=1 },
            new SectionField { Key = "hero.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис сторінки", Type = "textarea", Position=2 },
            new SectionField { Key = "hero.button1.text", Value = null, Placeholder ="Введіть дані", Title ="Кнопка №1", Position=3 },
            new SectionField { Key = "hero.button2.text", Value = null, Placeholder ="Введіть дані", Title ="Кнопка №2", Position=4 },
            new SectionField { Key = "hero.block1.image.link", Value = null, Placeholder ="Вставте посилання", Title ="Фонове зображення", Type = "image", SubSectionTitle="Блок 1", Position=5, Dimensions="250x260"  },
            new SectionField { Key = "hero.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок блоку", SubSectionTitle="Блок 1", Type = "textarea", Position=6 },
            new SectionField { Key = "hero.block1.value.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок", SubSectionTitle="Блок 1", Position=7 },
            new SectionField { Key = "hero.block2.image.link", Value = null, Placeholder ="Вставте посилання", Title ="Фонове зображення", Type = "image", SubSectionTitle="Блок 2", Position=8, Dimensions="250x260" },
            new SectionField { Key = "hero.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок блоку", SubSectionTitle="Блок 2", Type = "textarea", Position=9 },
            new SectionField { Key = "hero.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", SubSectionTitle="Блок 2", Type = "textarea", Position=10 },
            new SectionField { Key = "hero.block2.value.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок", SubSectionTitle="Блок 2", Position=11 },
            new SectionField { Key = "hero.block3.image.link", Value = null, Placeholder ="Вставте посилання", Title ="Фонове зображення", Type = "image", SubSectionTitle="Блок 3", Position=12, Dimensions="251x88" },
            new SectionField { Key = "hero.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок блоку", SubSectionTitle="Блок 3", Type = "textarea", Position=13 },
            new SectionField { Key = "hero.block3.value.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок", SubSectionTitle="Блок 3", Position=14 },
            new SectionField { Key = "hero.block4.image.link", Value = null, Placeholder ="Вставте посилання", Title ="Фонове зображення", Type = "image", SubSectionTitle="Блок 4", Position=15, Dimensions="252x262" },
            new SectionField { Key = "hero.block4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок блоку", SubSectionTitle="Блок 4", Type = "textarea", Position=16 },
            new SectionField { Key = "hero.block4.value.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок", SubSectionTitle="Блок 4", Position=17 },
            new SectionField { Key = "hero.block5.image.link", Value = null, Placeholder ="Вставте посилання", Title ="Фонове зображення", Type = "image", SubSectionTitle="Блок 5", Position=18, Dimensions="250x353" },
            new SectionField { Key = "hero.block5.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок блоку", SubSectionTitle="Блок 5", Type = "textarea", Position=19 },
            new SectionField { Key = "hero.block5.value.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок", SubSectionTitle="Блок 5", Position=20 },
            new SectionField { Key = "hero.block6.image.link", Value = null, Placeholder ="Вставте посилання", Title ="Фонове зображення", Type = "image", SubSectionTitle="Блок 6", Position=21, Dimensions="250x260" },
            new SectionField { Key = "hero.block6.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок блоку", SubSectionTitle="Блок 6", Type = "textarea", Position=22 },
            new SectionField { Key = "hero.block6.value.text", Value = null, Placeholder ="Введіть дані", Title ="Підзаголовок", SubSectionTitle="Блок 6", Position=23 },
            new SectionField { Key = "hero.block6.header.image", Value = null, Placeholder ="Вставте посилання", Title ="Зображення", Type = "image", SubSectionTitle="Блок 6", Position=24, Dimensions="40x40" },
            new SectionField { Key = "hero.block6.header.title", Value = null, Placeholder ="Введіть дані", Title ="Опис", SubSectionTitle="Блок 6", Position=25 },
        }
    },
  new Section
    {
      PageId = newPage.Id,
      Name = "Банер",
      Title = "Banner",
      Position = 2,
      IsActive = false,
      IsHtml = true,
      CreatedAt = DateTime.UtcNow.SetKindUtc(),
      SectionFields = new List<SectionField>
      {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position = 24, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 24, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.alt", Placeholder = "Введіть alt значення", Title = "Alt значення", Value = null, Position = 26 },
          new SectionField { Key = "banner.block.image.link", Placeholder = "Вставте посилання банера", Title = "Посилання банера", Value = null, Position = 27 },
      }
  },
};

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);
        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateCookiesSubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
        {
             new Section
             {
                PageId = newPage.Id,
                Name = "Політика Кукіс",
                Title = "Cookies",
                Position = 1,
                IsActive = true,
                IsHtml = false,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                SectionFields = new List<SectionField>
        {
            new SectionField { Key = "cookies.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "cookies.data.text", Value = null, Placeholder ="Введіть дані", Title ="Контент сторінки кукіс", Type="html", Position=1 },
        }
    },
            };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);
        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateAccamulationsSubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
        {
            new Section
            {
                PageId = newPage.Id,
                Name = "Хіро секція",
                Title = "Hero",
                Position = 1,
                IsActive = true,
                IsHtml = false,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                SectionFields = new List<SectionField>
            {
            new SectionField { Key = "hero.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "hero.title.activeWordIndex", Value = null, Placeholder ="Введіть позицію активного слова", Title ="Позиція активного слова", Type = "number", Position=1 },
            new SectionField { Key = "hero.title.activeWordColor", Value = null, Placeholder ="", Title ="Колір активного слова", Type = "colorPicker", Position = 2 },
            new SectionField { Key = "hero.advantages.first.text", Value = null, Placeholder = "Введіть дані", Title = "Вигода 1", Position=3, SubSectionTitle="Вигода"},
            new SectionField { Key = "hero.advantages.second.text", Value = null, Placeholder = "Введіть дані", Title = "Вигода 2", Position=4 , SubSectionTitle="Вигода"},
            new SectionField { Key = "hero.advantages.third.text", Value = null, Placeholder = "Введіть дані", Title = "Вигода 3", Position=5, SubSectionTitle="Вигода"},
            new SectionField { Key = "hero.button.text", Value = null, Placeholder ="Введіть текст кнопки", Title ="Текст кнопки", Position=6 },
            new SectionField { Key = "hero.button.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=7 },
            new SectionField { Key = "hero.image.link", Value = null, Placeholder = "", Title = "Додайте зображення", Type = "image", Position=8, Dimensions="647x620"  },
            new SectionField { Key = "hero.image.alt", Value = null, Placeholder = "Введіть alt значення", Title = "Alt значення", Position=9 },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Як це працює",
        Title = "HowItWorks",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "calculation.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=10 },
           new SectionField { Key = "calculation.conditions.condition1.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Type = "textarea", Position=11, SubSectionTitle="Умови" },
           new SectionField { Key = "calculation.conditions.condition2.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 2", Type = "textarea", Position=12, SubSectionTitle="Умови" },
           new SectionField { Key = "calculation.conditions.condition3.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 3", Type = "textarea", Position=13, SubSectionTitle="Умови" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Використання карти +1",
        Title = "UseCard",
        Position = 3,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "use.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=14 },
           new SectionField { Key = "use.step1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва", Position=15, SubSectionTitle="Крок 1" },
           new SectionField { Key = "use.step1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=16, SubSectionTitle="Крок 1", Dimensions="714x530" },
           new SectionField { Key = "use.step1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=17, SubSectionTitle="Крок 1" },
           new SectionField { Key = "use.step2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=18, SubSectionTitle="Крок 2" },
           new SectionField { Key = "use.step2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=19, SubSectionTitle="Крок 2", Dimensions="714x530" },
           new SectionField { Key = "use.step2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=20, SubSectionTitle="Крок 2" },
           new SectionField { Key = "use.step3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=21, SubSectionTitle="Крок 3" },
           new SectionField { Key = "use.step3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=22, SubSectionTitle="Крок 3", Dimensions="714x530" },
           new SectionField { Key = "use.step3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=23, SubSectionTitle="Крок 3" },
           new SectionField { Key = "use.step4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=24, SubSectionTitle="Крок 4" },
           new SectionField { Key = "use.step4.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=25, SubSectionTitle="Крок 4", Dimensions="714x530" },
           new SectionField { Key = "use.step4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=26, SubSectionTitle="Крок 4" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Що входить в програму лояльності",
        Position = 4,
        Title = "OtherFeatures",
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "additional.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=29 },
           new SectionField { Key = "additional.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=30 },
           new SectionField { Key = "additional.block1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=31, SubSectionTitle="Блок 1", Dimensions="498x364" },
           new SectionField { Key = "additional.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=32, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=33, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=34, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=35, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=36, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=37, SubSectionTitle="Блок 2", Dimensions="498x364" },
           new SectionField { Key = "additional.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=38, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=39, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=40, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=41, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=42, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=43, SubSectionTitle="Блок 3", Dimensions="498x364" },
           new SectionField { Key = "additional.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=44, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=45, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=46, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=47, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=48, SubSectionTitle="Блок 3" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Банер",
        Title = "Banner",
        Position = 5,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position = 49, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 50, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.link", Value = null, Placeholder = null, Title ="Вставте посилання", Position=51 },
          new SectionField { Key = "banner.block.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=52 },
        }
    },
     };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);

        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateVaucherSubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
     {
new Section
    {
        PageId = newPage.Id,
        Name = "Хіро секція",
        Title = "Hero",
        Position = 1,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "hero.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "hero.title.activeWordIndex", Value = null, Placeholder ="Введіть позицію активного слова", Title ="Позиція активного слова", Type = "number", Position=1 },
            new SectionField { Key = "hero.title.activeWordColor", Value = null, Placeholder ="", Title ="Колір активного слова", Type = "colorPicker", Position = 2 },
            new SectionField { Key = "hero.description.text", Value = null, Placeholder ="Введіть опис", Title ="Опис сторінки", Type = "textarea", Position=3 },
            new SectionField { Key = "hero.button.text", Value = null, Placeholder ="Введіть текст кнопки", Title ="Текст кнопки", Position=4 },
            new SectionField { Key = "hero.button.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=5 },
            new SectionField { Key = "hero.image.link", Value = null, Placeholder = "", Title = "Додайте зображення", Type = "image", Position=6, Dimensions="647x620"  },
            new SectionField { Key = "hero.image.alt", Value = null, Placeholder = "Введіть alt значення", Title = "Alt значення", Position=7  },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Нарахування ваучерів",
        Title = "VouchersCalculation",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "calculation.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=8 },
           new SectionField { Key = "calculation.conditions.condition1.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Type = "textarea", Position=9, SubSectionTitle="Умови" },
           new SectionField { Key = "calculation.conditions.condition2.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 2", Type = "textarea", Position=10, SubSectionTitle="Умови" },
           new SectionField { Key = "calculation.conditions.condition3.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 3", Type = "textarea", Position=11, SubSectionTitle="Умови" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Використання ваучерів",
        Title = "UseVouchers",
        Position = 3,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "use.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=12 },
           new SectionField { Key = "use.step1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва", Position=13, SubSectionTitle="Крок 1" },
           new SectionField { Key = "use.step1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=14, SubSectionTitle="Крок 1", Dimensions="714x530" },
           new SectionField { Key = "use.step1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=15, SubSectionTitle="Крок 1" },
           new SectionField { Key = "use.step2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=16, SubSectionTitle="Крок 2" },
           new SectionField { Key = "use.step2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=17, SubSectionTitle="Крок 2", Dimensions="714x530" },
           new SectionField { Key = "use.step2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=18, SubSectionTitle="Крок 2" },
           new SectionField { Key = "use.step3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=19, SubSectionTitle="Крок 3" },
           new SectionField { Key = "use.step3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=20, SubSectionTitle="Крок 3", Dimensions="714x530" },
           new SectionField { Key = "use.step3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=21, SubSectionTitle="Крок 3" },
           new SectionField { Key = "use.step4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=22, SubSectionTitle="Крок 4" },
           new SectionField { Key = "use.step4.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=23, SubSectionTitle="Крок 4", Dimensions="714x530" },
           new SectionField { Key = "use.step4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=24, SubSectionTitle="Крок 4" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Що входить в програму лояльності",
        Title = "Features",
        Position = 4,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "additional.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=27 },
           new SectionField { Key = "additional.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=28 },
           new SectionField { Key = "additional.block1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=29, SubSectionTitle="Блок 1", Dimensions="498x364" },
           new SectionField { Key = "additional.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=30, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=31, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=32, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=33, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=34, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=35, SubSectionTitle="Блок 2", Dimensions="498x364" },
           new SectionField { Key = "additional.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=36, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=37, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=38, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=39, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=40, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=41, SubSectionTitle="Блок 3", Dimensions="498x364" },
           new SectionField { Key = "additional.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=42, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=43, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=44, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=45, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=46, SubSectionTitle="Блок 3" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Банер",
        Title = "Banner",
        Position = 5,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position = 47, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 48, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.link", Value = null, Placeholder = null, Title ="Вставте посилання", Position=49 },
          new SectionField { Key = "banner.block.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=50 },
        }
    },
     };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);
        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateBonusSubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
     {
new Section
    {
        PageId = newPage.Id,
        Name = "Хіро секція",
        Title = "Hero",
        Position = 1,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "hero.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "hero.title.activeWordIndex", Value = null, Placeholder ="Введіть позицію активного слова", Title ="Позиція активного слова", Type = "number", Position=1 },
            new SectionField { Key = "hero.title.activeWordColor", Value = null, Placeholder ="", Title ="Колір активного слова", Type = "colorPicker", Position=2 },
            new SectionField { Key = "hero.description.text", Value = null, Placeholder ="Введіть опис", Title ="Опис сторінки", Type = "textarea", Position=3 },
            new SectionField { Key = "hero.button.text", Value = null, Placeholder ="Введіть текст кнопки", Title ="Текст кнопки", Position=4 },
            new SectionField { Key = "hero.button.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=5 },
            new SectionField { Key = "hero.image.link", Value = null, Placeholder = "", Title = "Додайте зображення", Type = "image", Position=6, Dimensions="647x620"  },
            new SectionField { Key = "hero.image.alt", Value = null, Placeholder = "Введіть alt значення", Title = "Alt значення", Position=7  },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Нарахування бонусів",
        Title = "BonusesCalculation",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "calculation.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=8 },
           new SectionField { Key = "calculation.conditions.condition1.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Type = "textarea", Position=9, SubSectionTitle="Умови" },
           new SectionField { Key = "calculation.conditions.condition2.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 2", Type = "textarea", Position=10, SubSectionTitle="Умови" },
           new SectionField { Key = "calculation.conditions.condition3.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 3", Type = "textarea", Position=11, SubSectionTitle="Умови" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Використання бонусів",
        Title = "UseBonuses",
        Position = 3,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "use.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=12 },
           new SectionField { Key = "use.step1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва", Position=13, SubSectionTitle="Крок 1" },
           new SectionField { Key = "use.step1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=14, SubSectionTitle="Крок 1", Dimensions="714x530" },
           new SectionField { Key = "use.step1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=15, SubSectionTitle="Крок 1" },
           new SectionField { Key = "use.step2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=16, SubSectionTitle="Крок 2" },
           new SectionField { Key = "use.step2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=17, SubSectionTitle="Крок 2", Dimensions="714x530" },
           new SectionField { Key = "use.step2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=18, SubSectionTitle="Крок 2" },
           new SectionField { Key = "use.step3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=19, SubSectionTitle="Крок 3" },
           new SectionField { Key = "use.step3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=20, SubSectionTitle="Крок 3", Dimensions="714x530" },
           new SectionField { Key = "use.step3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=21, SubSectionTitle="Крок 3" },
           new SectionField { Key = "use.step4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Назва ", Position=22, SubSectionTitle="Крок 4" },
           new SectionField { Key = "use.step4.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=23, SubSectionTitle="Крок 4", Dimensions="714x530" },
           new SectionField { Key = "use.step4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=24, SubSectionTitle="Крок 4" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Що входить в програму лояльності",
        Title = "OtherFeatures",
        Position = 4,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "additional.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=27 },
           new SectionField { Key = "additional.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=28 },
           new SectionField { Key = "additional.block1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=29, SubSectionTitle="Блок 1", Dimensions="498x364" },
           new SectionField { Key = "additional.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=30, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=31, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=32, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=33, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=34, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=35, SubSectionTitle="Блок 2", Dimensions="498x364" },
           new SectionField { Key = "additional.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=36, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=37, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=38, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=39, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=40, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=41, SubSectionTitle="Блок 3", Dimensions="498x364" },
           new SectionField { Key = "additional.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=42, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=43, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=44, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=45, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=46, SubSectionTitle="Блок 3" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Банер",
        Title = "Banner",
        Position = 5,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position = 47, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 48, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.link", Value = null, Placeholder = null, Title ="Вставте посилання", Position=49 },
          new SectionField { Key = "banner.block.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=50 },
        }
    },
     };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);

        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    private async Task<PageDTO> CreateHolidayPromotionsPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
        {
            new Section
            {
                PageId = newPage.Id,
                Name = "Хіро секція",
                Title = "Hero",
                Position = 1,
                IsActive = true,
                IsHtml = false,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                SectionFields = new List<SectionField>
             {
            new SectionField { Key = "hero.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0 },
            new SectionField { Key = "hero.title.activeWordIndex", Value = null, Placeholder ="Введіть позицію активного слова", Title ="Позиція активного слова", Type = "number", Position=1 },
            new SectionField { Key = "hero.title.activeWordColor", Value = null, Placeholder ="", Title ="Колір активного слова", Type = "colorPicker", Position=2 },
            new SectionField { Key = "hero.description.text", Value = null, Placeholder ="Введіть опис", Title ="Опис сторінки", Type = "textarea", Position=3 },
            new SectionField { Key = "hero.button.text", Value = null, Placeholder ="Введіть текст кнопки", Title ="Текст кнопки", Position=4 },
            new SectionField { Key = "hero.button.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=5 },
            new SectionField { Key = "hero.image.link", Value = null, Placeholder = "", Title = "Додайте зображення", Type = "image", Position=6, Dimensions="647x620"  },
            new SectionField { Key = "hero.image.alt", Value = null, Placeholder = "Введіть alt значення", Title = "Alt значення", Position=7  },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Нарахування святкових бонусів",
        Title = "BonusesCalculation",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "calculation.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=8 },
           new SectionField { Key = "calculation.conditions.condition1.text", Value = null, Placeholder ="Введіть дані", Title ="Умова", Type = "textarea", Position=9, SubSectionTitle="Умови", Extendable = true },
           new SectionField { Key = "calculation.conditions.condition2.text", Value = null, Placeholder ="Введіть дані", Title ="Умова", Type = "textarea", Position=10, SubSectionTitle="Умови", Extendable = true },
           new SectionField { Key = "calculation.conditions.condition3.text", Value = null, Placeholder ="Введіть дані", Title ="Умова", Type = "textarea", Position=11, SubSectionTitle="Умови", Extendable = true },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Що входить в програму лояльності",
        Title = "OtherFeatures",
        Position = 3,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "additional.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=12 },
           new SectionField { Key = "additional.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=13 },
           new SectionField { Key = "additional.block1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=14, SubSectionTitle="Блок 1", Dimensions="498x364" },
           new SectionField { Key = "additional.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=15, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=16, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Position=17, Type = "textarea", SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=18, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=19, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=20, SubSectionTitle="Блок 2", Dimensions="498x364" },
           new SectionField { Key = "additional.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=21, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=22, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=23, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=24, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=25, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=26, SubSectionTitle="Блок 3", Dimensions="498x364" },
           new SectionField { Key = "additional.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=27, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=28, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=29, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=30, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=31, SubSectionTitle="Блок 3" },
        }
    },
    new Section
    {
        PageId = newPage.Id,
        Name = "Банер",
        Title = "Banner",
        Position = 4,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position =32, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 33, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.link", Value = null, Placeholder = null, Title ="Вставте посилання", Position=34 },
          new SectionField { Key = "banner.block.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=35 },
        }
    },
     };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);

        return new PageDTO
        {
            Id = newPage.Id,
        };


    }

    private async Task<PageDTO> CreateHomePage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        List<Section> newSections = new();

        switch (subPage.BranchId)
        {
            case (byte)Branches.BirdJet:
                newSections = GetBirdJetSections(newPage.Id);
                break;
            case (byte)Branches.CatJet:
                newSections = GetCatJetSections(newPage.Id);
                break;
            default:
                break;
        }

        if (newSections.Count == 0)
        {
            throw new NotFoundException("No branchId found");
        }
        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);
        await SeoUpdate(newPage.Id);

        return new PageDTO { Id = newPage.Id };
    }

    private List<Section> GetBirdJetSections(int pageId)
    {
        return new List<Section>
        {
     new Section
     {
        PageId = pageId,
        Name = "Хіро секція",
        Title = "Hero",
        Position = 1,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "hero.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.title.activeWordIndex", Value = null, Placeholder ="Введіть позицію активного слова", Title ="Позиція активного слова", Type = "number", Position=1, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.title.activeWordColor", Value = null, Placeholder ="", Title ="Колір активного слова", Type = "colorPicker", Position=2 },
            new SectionField { Key = "hero.block1.description.text", Value = null, Placeholder ="Введіть опис", Title ="Опис сторінки", Type = "textarea", Position=3, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.button.text", Value = null, Placeholder ="Введіть текст кнопки", Title ="Текст кнопки", Position=4, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.image.link", Value = null, Placeholder = "", Title = "Додайте зображення", Type = "image", Position=5, SubSectionTitle="Основний контент", Dimensions="647x620"  },
            new SectionField { Key = "hero.block1.image.alt", Value = null, Placeholder = "Введіть alt значення", Title = "Alt значення", Position=6, SubSectionTitle="Основний контент"  },
            new SectionField { Key = "hero.block2.data.number", Value = null, Placeholder = "Введіть дані", Title = "Цифра (для маленького блоку)", Position=7  },
            new SectionField { Key = "hero.block2.data.text", Value = null, Placeholder = "Введіть дані", Title = "Заголовок (для маленького блоку)", Position=8  },
            new SectionField { Key = "hero.block2.images.link1", Value = null, Placeholder = "", Title = "Зображення №1", Type = "image", Position=9, Dimensions="72x72"  },
            new SectionField { Key = "hero.block2.images.link2", Value = null, Placeholder = "", Title = "Зображення №2", Type = "image", Position=10, Dimensions="72x72"  },
            new SectionField { Key = "hero.block2.images.link3", Value = null, Placeholder = "", Title = "Зображення №3", Type = "image", Position=11, Dimensions="72x72" },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Банер",
        Title = "Banner",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position =12, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 13, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.link", Value = null, Placeholder = null, Title ="Вставте посилання", Position=14 },
          new SectionField { Key = "banner.block.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=15 },
        }
    },

       new Section
    {
        PageId = pageId,
        Name = "Що входить в програму лояльності",
        Title = "Features",
        Position = 3,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "additional.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=16 },
           new SectionField { Key = "additional.common.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=17 },
           new SectionField { Key = "additional.block1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=18, SubSectionTitle="Блок 1", Dimensions="624x627" },
           new SectionField { Key = "additional.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=19, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=20, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=21, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=22, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=23, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=24, SubSectionTitle="Блок 2", Dimensions="498x364" },
           new SectionField { Key = "additional.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=25, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=26, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис",Type = "textarea", Position=27, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=28, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=29, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=30, SubSectionTitle="Блок 3", Dimensions="498x364" },
           new SectionField { Key = "additional.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=31, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=32, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=33, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=34, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=35, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block4.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=36, SubSectionTitle="Блок 4", Dimensions="498x364" },
           new SectionField { Key = "additional.block4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=37, SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=38, SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Position=39, Type = "textarea", SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=40, SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=41, SubSectionTitle="Блок 4" },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Чому ми?",
        Title = "WhyWe",
        Position = 4,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "whyWe.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=42 },
           new SectionField { Key = "whyWe.common.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=43 },
           new SectionField { Key = "whyWe.block1.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=44, SubSectionTitle="Блок 1", Dimensions="72x72" },
           new SectionField { Key = "whyWe.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=45, SubSectionTitle="Блок 1" },
           new SectionField { Key = "whyWe.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Position=46, SubSectionTitle="Блок 1" },
           new SectionField { Key = "whyWe.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=47, SubSectionTitle="Блок 1" },
           new SectionField { Key = "whyWe.block2.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=48, SubSectionTitle="Блок 2", Dimensions="72x72" },
           new SectionField { Key = "whyWe.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=49, SubSectionTitle="Блок 2" },
           new SectionField { Key = "whyWe.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Position=50, SubSectionTitle="Блок 2" },
           new SectionField { Key = "whyWe.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=51, SubSectionTitle="Блок 2" },
           new SectionField { Key = "whyWe.block3.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=52, SubSectionTitle="Блок 3", Dimensions="72x72" },
           new SectionField { Key = "whyWe.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=53, SubSectionTitle="Блок 3" },
           new SectionField { Key = "whyWe.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Position=54, SubSectionTitle="Блок 3" },
           new SectionField { Key = "whyWe.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=55, SubSectionTitle="Блок 3" },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Мобільний додаток",
        Title = "MobileApp",
        Position = 5,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "app.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=56 },
           new SectionField { Key = "app.common.appStores.googlePlay", Value = null, Placeholder ="Вставте посилання", Title ="Посилання на Google Play", Position=57 },
           new SectionField { Key = "app.common.appStores.appStore", Value = null, Placeholder ="Вставте посилання", Title ="Посилання на App Store", Position=58 },
           new SectionField { Key = "app.block1.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=59, SubSectionTitle="Блок 1", Dimensions="648x716" },
           new SectionField { Key = "app.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=60, SubSectionTitle="Блок 1" },
           new SectionField { Key = "app.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №1", Position=61, SubSectionTitle="Блок 1" },
           new SectionField { Key = "app.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=62, SubSectionTitle="Блок 1" },
           new SectionField { Key = "app.block2.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=63, SubSectionTitle="Блок 2", Dimensions="648x716" },
           new SectionField { Key = "app.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=64, SubSectionTitle="Блок 2" },
           new SectionField { Key = "app.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №2", Position=65, SubSectionTitle="Блок 2" },
           new SectionField { Key = "app.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=66, SubSectionTitle="Блок 2" },
           new SectionField { Key = "app.block3.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=67, SubSectionTitle="Блок 3", Dimensions="648x716" },
           new SectionField { Key = "app.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=68, SubSectionTitle="Блок 3" },
           new SectionField { Key = "app.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №3", Position=69, SubSectionTitle="Блок 3" },
           new SectionField { Key = "app.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=70, SubSectionTitle="Блок 3" },
           new SectionField { Key = "app.block4.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=71, SubSectionTitle="Блок 4", Dimensions="648x716" },
           new SectionField { Key = "app.block4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=72, SubSectionTitle="Блок 4" },
           new SectionField { Key = "app.block4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №4", Position=73, SubSectionTitle="Блок 4" },
           new SectionField { Key = "app.block4.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=74, SubSectionTitle="Блок 4" },
        }
    },

    new Section
    {
        PageId = pageId,
        Name = "FAQ",
        Title = "FAQ",
        Position = 6,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "faq.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=75 },
           new SectionField { Key = "faq.common.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=76 },
           new SectionField { Key = "faq.question1.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=77, SubSectionTitle="Запитання 1" },
           new SectionField { Key = "faq.question1.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=78, SubSectionTitle="Запитання 1" },

           new SectionField { Key = "faq.question2.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=79, SubSectionTitle="Запитання 2" },
           new SectionField { Key = "faq.question2.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=80, SubSectionTitle="Запитання 2" },

           new SectionField { Key = "faq.question3.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=81, SubSectionTitle="Запитання 3" },
           new SectionField { Key = "faq.question3.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=82, SubSectionTitle="Запитання 3" },

           new SectionField { Key = "faq.question4.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=83, SubSectionTitle="Запитання 4" },
           new SectionField { Key = "faq.question4.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=84, SubSectionTitle="Запитання 4" },


           new SectionField { Key = "faq.additional.title.text", Value = null, Placeholder ="Ведіть дані", Title ="Заголовок", Position=85, SubSectionTitle="Додатковий блок" },
           new SectionField { Key = "faq.additional.descriprion.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=86, SubSectionTitle="Додатковий блок" },
           new SectionField { Key = "faq.additional.button.text", Value = null, Placeholder ="Введіть дані", Title ="Текст кнопки", Position=87, SubSectionTitle="Додатковий блок" },
           new SectionField { Key = "faq.additional.button.link", Value = null, Placeholder ="Вставте посилання", Title ="Посилання кнопки", Position=88, SubSectionTitle="Додатковий блок" },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Акції",
        Title = "Promotions",
        Position = 7,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "promotions.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=89 },
           new SectionField { Key = "promotions.common.button.text", Value = null, Placeholder ="Введіть дані", Title ="Текст кнопки", Position=90 },
           new SectionField { Key = "promotions.common.button.link", Value = null, Placeholder ="Вставте посилання", Title ="Посилання кнопки", Position=91 },
        }
    }
        };
    }

    private List<Section> GetCatJetSections(int pageId)
    {
        return new List<Section>
      {
        new Section
        {
        PageId = pageId,
        Name = "Хіро секція",
        Title = "Hero",
        Position = 1,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
            new SectionField { Key = "hero.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=0, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.title.activeWordIndex", Value = null, Placeholder ="Введіть позицію активного слова", Title ="Позиція активного слова", Type = "number", Position=1, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.title.activeWordColor", Value = null, Placeholder ="", Title ="Колір активного слова", Type = "colorPicker", Position=2 },
            new SectionField { Key = "hero.block1.description.text", Value = null, Placeholder ="Введіть опис", Title ="Опис сторінки", Type = "textarea", Position=3, SubSectionTitle="Основний контент" },
            new SectionField { Key = "hero.block1.button.text", Value = null, Placeholder ="Введіть текст кнопки", Title ="Текст кнопки", Position=4, SubSectionTitle="Основний контент" },

            new SectionField { Key = "hero.block1.data.title", Value = null, Placeholder = "Введіть дані", Title = "Заголовок", Position=5  },
            new SectionField { Key = "hero.block1.data.subtitle", Value = null, Placeholder = "Введіть дані", Title = "Опис", Position=6  },
            new SectionField { Key = "hero.block1.images.link1", Value = null, Placeholder = "", Title = "Зображення №1", Type = "image", Position=7, Dimensions="72x72"  },
            new SectionField { Key = "hero.block1.images.link2", Value = null, Placeholder = "", Title = "Зображення №2", Type = "image", Position=8, Dimensions="72x72"  },
            new SectionField { Key = "hero.block1.images.link3", Value = null, Placeholder = "", Title = "Зображення №3", Type = "image", Position=9, Dimensions="72x72" },

            new SectionField { Key = "hero.block2.data.title", Value = null, Placeholder = "Введіть дані", Title = "Заголовок", Position=10  },
            new SectionField { Key = "hero.block2.data.subtitle", Value = null, Placeholder = "Введіть дані", Title = "Опис", Position=11  },
            new SectionField { Key = "hero.block2.images.link1", Value = null, Placeholder = "", Title = "Зображення №1", Type = "image", Position=12, Dimensions="72x72"  },
            new SectionField { Key = "hero.block2.images.link2", Value = null, Placeholder = "", Title = "Зображення №2", Type = "image", Position=13, Dimensions="72x72"  },
            new SectionField { Key = "hero.block2.images.link3", Value = null, Placeholder = "", Title = "Зображення №3", Type = "image", Position=14, Dimensions="72x72" },
            new SectionField { Key = "hero.block2.images.link4", Value = null, Placeholder = "", Title = "Зображення №3", Type = "image", Position=15, Dimensions="72x72" },

            new SectionField { Key = "hero.block3.data.title", Value = null, Placeholder = "Введіть дані", Title = "Заголовок", Position=16  },
            new SectionField { Key = "hero.block3.data.subtitle", Value = null, Placeholder = "Введіть дані", Title = "Опис", Position=17  },
            new SectionField { Key = "hero.block3.images.link1", Value = null, Placeholder = "", Title = "Зображення", Type = "image", Position=18, Dimensions="72x72"  },

            new SectionField { Key = "hero.block4.data.title", Value = null, Placeholder = "Введіть дані", Title = "Заголовок", Position=19  },
            new SectionField { Key = "hero.block4.data.subtitle", Value = null, Placeholder = "Введіть дані", Title = "Опис", Position=20  },
            new SectionField { Key = "hero.block4.images.link1", Value = null, Placeholder = "", Title = "Зображення", Type = "image", Position=21, Dimensions="72x72"  },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Банер",
        Title = "Banner",
        Position = 2,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
          new SectionField { Key = "banner.block.imageDesktop.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер", Value = null, Position =22, Dimensions="1980x575" },
          new SectionField { Key = "banner.block.imageMobile.link", Placeholder = "Введіть посилання банера", Type = "image", Title = "Банер (мобільна версія)", Value = null, Position = 23, Dimensions="335x708" },
          new SectionField { Key = "banner.block.image.link", Value = null, Placeholder = null, Title ="Вставте посилання", Position=24 },
          new SectionField { Key = "banner.block.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=25},
        }
    },

       new Section
    {
        PageId = pageId,
        Name = "Що входить в програму лояльності",
        Title = "Features",
        Position = 3,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "additional.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=26 },
           new SectionField { Key = "additional.common.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=27 },
           new SectionField { Key = "additional.block1.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=28, SubSectionTitle="Блок 1", Dimensions="624x627" },
           new SectionField { Key = "additional.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=29, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=30, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=31, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=32, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block1.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=33, SubSectionTitle="Блок 1" },
           new SectionField { Key = "additional.block2.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=34, SubSectionTitle="Блок 2", Dimensions="498x364" },
           new SectionField { Key = "additional.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=35, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=36, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис",Type = "textarea", Position=37, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=38, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block2.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=39, SubSectionTitle="Блок 2" },
           new SectionField { Key = "additional.block3.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=40, SubSectionTitle="Блок 3", Dimensions="498x364" },
           new SectionField { Key = "additional.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=41, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=42, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=43, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=44, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block3.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=45, SubSectionTitle="Блок 3" },
           new SectionField { Key = "additional.block4.image.link", Value = null, Placeholder = null, Title ="Зображення", Type = "image", Position=46, SubSectionTitle="Блок 4", Dimensions="498x364" },
           new SectionField { Key = "additional.block4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=47, SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=48, SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Position=49, Type = "textarea", SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.link.text", Value = null, Placeholder ="Введіть назву посилання", Title ="Назва посилання", Position=50, SubSectionTitle="Блок 4" },
           new SectionField { Key = "additional.block4.link.link", Value = null, Placeholder ="Введіть посилання", Title ="Посилання", Position=51, SubSectionTitle="Блок 4" },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Чому ми?",
        Title = "WhyWe",
        Position = 4,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "whyWe.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=52 },
           new SectionField { Key = "whyWe.common.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=53 },
           new SectionField { Key = "whyWe.block1.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=54, SubSectionTitle="Блок 1", Dimensions="72x72" },
           new SectionField { Key = "whyWe.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=55, SubSectionTitle="Блок 1" },
           new SectionField { Key = "whyWe.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Position=56, SubSectionTitle="Блок 1" },
           new SectionField { Key = "whyWe.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=57, SubSectionTitle="Блок 1" },
           new SectionField { Key = "whyWe.block2.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=58, SubSectionTitle="Блок 2", Dimensions="72x72" },
           new SectionField { Key = "whyWe.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=59, SubSectionTitle="Блок 2" },
           new SectionField { Key = "whyWe.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Position=60, SubSectionTitle="Блок 2" },
           new SectionField { Key = "whyWe.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=61, SubSectionTitle="Блок 2" },
           new SectionField { Key = "whyWe.block3.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=62, SubSectionTitle="Блок 3", Dimensions="72x72" },
           new SectionField { Key = "whyWe.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=63, SubSectionTitle="Блок 3" },
           new SectionField { Key = "whyWe.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Умова 1", Position=64, SubSectionTitle="Блок 3" },
           new SectionField { Key = "whyWe.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=65, SubSectionTitle="Блок 3" },
        }
    },
    new Section
    {
        PageId = pageId,
        Name = "Мобільний додаток",
        Title = "MobileApp",
        Position = 5,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "app.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=66 },
           new SectionField { Key = "app.common.appStores.googlePlay", Value = null, Placeholder ="Вставте посилання", Title ="Посилання на Google Play", Position=67 },
           new SectionField { Key = "app.common.appStores.appStore", Value = null, Placeholder ="Вставте посилання", Title ="Посилання на App Store", Position=68 },
           new SectionField { Key = "app.block1.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=69, SubSectionTitle="Блок 1", Dimensions="648x716" },
           new SectionField { Key = "app.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=70, SubSectionTitle="Блок 1" },
           new SectionField { Key = "app.block1.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №1", Position=71, SubSectionTitle="Блок 1" },
           new SectionField { Key = "app.block1.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=72, SubSectionTitle="Блок 1" },
           new SectionField { Key = "app.block2.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=73, SubSectionTitle="Блок 2", Dimensions="648x716" },
           new SectionField { Key = "app.block2.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=74, SubSectionTitle="Блок 2" },
           new SectionField { Key = "app.block2.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №2", Position=75, SubSectionTitle="Блок 2" },
           new SectionField { Key = "app.block2.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=76, SubSectionTitle="Блок 2" },
           new SectionField { Key = "app.block3.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=77, SubSectionTitle="Блок 3", Dimensions="648x716" },
           new SectionField { Key = "app.block3.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=78, SubSectionTitle="Блок 3" },
           new SectionField { Key = "app.block3.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №3", Position=79, SubSectionTitle="Блок 3" },
           new SectionField { Key = "app.block3.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=80, SubSectionTitle="Блок 3" },
           new SectionField { Key = "app.block4.image.link", Value = null, Placeholder ="", Title ="Зображення для секції", Type = "image", Position=81, SubSectionTitle="Блок 4", Dimensions="648x716" },
           new SectionField { Key = "app.block4.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=82, SubSectionTitle="Блок 4" },
           new SectionField { Key = "app.block4.title.text", Value = null, Placeholder ="Введіть дані", Title ="Перевага №4", Position=83, SubSectionTitle="Блок 4" },
           new SectionField { Key = "app.block4.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=84, SubSectionTitle="Блок 4" },
        }
    },

    new Section
    {
        PageId = pageId,
        Name = "FAQ",
        Title = "FAQ",
        Position = 6,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "faq.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=85 },
           new SectionField { Key = "faq.common.description.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=86 },
           new SectionField { Key = "faq.question1.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=87, SubSectionTitle="Запитання 1" },
           new SectionField { Key = "faq.question1.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=88, SubSectionTitle="Запитання 1" },

           new SectionField { Key = "faq.question2.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=89, SubSectionTitle="Запитання 2" },
           new SectionField { Key = "faq.question2.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=90, SubSectionTitle="Запитання 2" },

           new SectionField { Key = "faq.question3.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=91, SubSectionTitle="Запитання 3" },
           new SectionField { Key = "faq.question3.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=92, SubSectionTitle="Запитання 3" },

           new SectionField { Key = "faq.question4.question.text", Value = null, Placeholder ="Введіть запитання", Title ="Запитання", Position=93, SubSectionTitle="Запитання 4" },
           new SectionField { Key = "faq.question4.answer.text", Value = null, Placeholder ="Введіть відповідь", Title ="Відповідь", Type = "textarea", Position=94, SubSectionTitle="Запитання 4" },


           new SectionField { Key = "faq.additional.title.text", Value = null, Placeholder ="Ведіть дані", Title ="Заголовок", Position=95, SubSectionTitle="Додатковий блок" },
           new SectionField { Key = "faq.additional.descriprion.text", Value = null, Placeholder ="Введіть дані", Title ="Опис", Type = "textarea", Position=96, SubSectionTitle="Додатковий блок" },
           new SectionField { Key = "faq.additional.button.text", Value = null, Placeholder ="Введіть дані", Title ="Текст кнопки", Position=97, SubSectionTitle="Додатковий блок" },
           new SectionField { Key = "faq.additional.button.link", Value = null, Placeholder ="Вставте посилання", Title ="Посилання кнопки", Position=98, SubSectionTitle="Додатковий блок" },
        }
    },

    new Section
    {
        PageId = pageId,
        Name = "Акції",
        Title = "Promotions",
        Position = 7,
        IsActive = true,
        IsHtml = false,
        CreatedAt = DateTime.UtcNow.SetKindUtc(),
        SectionFields = new List<SectionField>
        {
           new SectionField { Key = "promotions.common.title.text", Value = null, Placeholder ="Введіть дані", Title ="Заголовок", Position=99 },
           new SectionField { Key = "promotions.common.button.text", Value = null, Placeholder ="Введіть дані", Title ="Текст кнопки", Position=100 },
           new SectionField { Key = "promotions.common.button.link", Value = null, Placeholder ="Вставте посилання", Title ="Посилання кнопки", Position=101 },
        }
    },
        };
    }

    private async Task SeoUpdate(int pageId)
    {
        var newSeo = new SeoMeta
        {
            CreatedAt = DateTime.UtcNow.SetKindUtc(),
            EntityId = pageId,
            EntityType = EntityTypesConstant.Page,
            Title = "",
            Description = ""
        };
        await _unitOfWork.SeoMeta.Add(newSeo);
        await _unitOfWork.Save();
    }

    private async Task<PageDTO> CreateContactSubPage(SubPageCreateRequest subPage)
    {
        var page = await GetRootPage(subPage.RootPage, subPage.BranchId);
        var newPage = await CreatePage(subPage, subPage.BranchId, page);

        var newSections = new List<Section>
        {
            new Section
            {
                PageId = newPage.Id,
                Name = "Основна секція",
                Title = "Main",
                Position = 1,
                IsActive = true,
                IsHtml = false,
                CreatedAt = DateTime.UtcNow.SetKindUtc(),
                SectionFields = new List<SectionField>
                {
                    new SectionField { Key = "banner.block1.imageDesktop.link", Placeholder = "", Type = "image", Title = "Банер", Value = null, Position =0, SubSectionTitle="Зображення", Dimensions="648x640" },
                    new SectionField { Key = "banner.block1.image.alt", Value = null, Placeholder ="Введіть alt значення", Title ="Alt значення", Position=2, SubSectionTitle="Зображення" },
                    new SectionField { Key = "banner.block2.email.text", Value = null, Placeholder ="Введіть емайл", Title ="Емайл", Position=3, SubSectionTitle="Контакти" },
                    new SectionField { Key = "banner.block2.phone.text", Value = null, Placeholder ="Введіть номер телефону", Title ="Номер телефону", Position=4, SubSectionTitle="Контакти" },
                }
            },
        };

        await _unitOfWork.Sections.AddRange(newSections);

        await _unitOfWork.Save(true);

        await SeoUpdate(newPage.Id);
        return new PageDTO
        {
            Id = newPage.Id,
        };
    }

    public Task<List<string>> GetPublishedPageLinks(byte branchId)
        => _unitOfWork.Pages.GetRootPages().Where(x => x.IsActive && x.BranchId == branchId)
            .Select(x => x.Link).ToListAsync();
}


