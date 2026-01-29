
using Microsoft.EntityFrameworkCore;
using Library.Shared.DTOs.Publisher;
using Library.Services.Interfaces;
using Library.Domain.Repositories;
using Library.Entities.Models;
using Mapster;
using Library.Common.Helpers;

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
            Validate.Positive(createdByUserId, nameof(createdByUserId));

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
            Validate.Positive(id, nameof(id));

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
            Validate.Positive(publisherId, nameof(publisherId));
            Validate.Positive(userId, nameof(userId));

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
            Validate.Positive(id, nameof(id));
            Validate.Positive(archivedByUserId, nameof(archivedByUserId));

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
