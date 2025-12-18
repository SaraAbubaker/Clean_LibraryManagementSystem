
using Library.Shared.DTOs.Publisher;

namespace Library.Services.Interfaces
{
    public interface IPublisherService
    {
        Task<PublisherListDto> CreatePublisherAsync(CreatePublisherDto dto, int createdByUserId);
        IQueryable<PublisherListDto> GetAllPublishersQuery();
        IQueryable<PublisherListDto> GetPublisherByIdQuery(int id);
        Task<PublisherListDto> UpdatePublisherAsync(UpdatePublisherDto dto, int userId, int publisherId);
        Task<bool> ArchivePublisherAsync(int id, int archivedByUserId);
    }
}
