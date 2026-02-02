
using Microsoft.EntityFrameworkCore;
using Library.Services.Interfaces;
using Library.Domain.Repositories;
using Library.Entities.Models;
using Mapster;
using Library.Common.Helpers;
using Library.Common.DTOs.LibraryDtos.Publisher;

namespace Library.Services.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IGenericRepository<Publisher> _publisherRepo;

        public PublisherService(IGenericRepository<Publisher> publisherRepo)
        {
            _publisherRepo = publisherRepo;
        }


        //CRUD
        public async Task<PublisherListDto> CreatePublisherAsync(CreatePublisherDto dto, int createdByUserId)
        {
            Validate.ValidateModel(dto);
            ValidationHelpers.ValidatePositive(createdByUserId, nameof(createdByUserId));

            var publisher = new Publisher
            {
                Name = dto.Name
            };

            await _publisherRepo.AddAsync(publisher, createdByUserId);
            await _publisherRepo.CommitAsync();

            var publisherDto = publisher.Adapt<PublisherListDto>();
            publisherDto.InventoryCount = publisher.InventoryRecords?.Count ?? 0;

            return publisherDto;
        }

        public IQueryable<PublisherListDto> GetAllPublishersQuery()
        {
            return _publisherRepo.GetAll()
                .Include(p => p.InventoryRecords)
                .AsNoTracking()
                .Select(p => new PublisherListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    InventoryCount = p.InventoryRecords.Count()
                });
        }

        public IQueryable<PublisherListDto> GetPublisherByIdQuery(int id)
        {
            ValidationHelpers.ValidatePositive(id, nameof(id));

            return _publisherRepo.GetAll()
                .Include(p => p.InventoryRecords)
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PublisherListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    InventoryCount = p.InventoryRecords.Count()
                });
        }

        public async Task<PublisherListDto> UpdatePublisherAsync(UpdatePublisherDto dto, int userId, int publisherId)
        {
            Validate.ValidateModel(dto);
            ValidationHelpers.ValidatePositive(publisherId, nameof(publisherId));
            ValidationHelpers.ValidatePositive(userId, nameof(userId));

            var publisher = Validate.Exists(
                await _publisherRepo.GetById(publisherId)
                    .Include(p => p.InventoryRecords)
                    .FirstOrDefaultAsync(),
                publisherId
            );

            publisher.Name = dto.Name;
            await _publisherRepo.UpdateAsync(publisher, userId);
            await _publisherRepo.CommitAsync();

            var publisherDto = publisher.Adapt<PublisherListDto>();
            publisherDto.InventoryCount = publisher.InventoryRecords?.Count ?? 0;

            return publisherDto;
        }

        public async Task<bool> ArchivePublisherAsync(int id, int archivedByUserId)
        {
            ValidationHelpers.ValidatePositive(id, nameof(id));
            ValidationHelpers.ValidatePositive(archivedByUserId, nameof(archivedByUserId));

            var publisher = Validate.Exists(
                await _publisherRepo.GetById(id)
                    .Include(p => p.InventoryRecords)
                    .FirstOrDefaultAsync(),
                id
            );

            await _publisherRepo.ArchiveAsync(publisher, archivedByUserId);

            return true;
        }
    }
}
