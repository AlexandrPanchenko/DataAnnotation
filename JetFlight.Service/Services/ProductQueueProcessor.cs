using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.Shared.Models.Promotion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JetFlight.Shared.Models.Product;

namespace JetFlight.Service.Services
{
    public class ProductQueueProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ProductQueueProcessor(IServiceScopeFactory scope)
        {
            _serviceScopeFactory = scope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            while (!stoppingToken.IsCancellationRequested)
            {
                var dateTime = DateTime.UtcNow;
                var mediaService = scope.ServiceProvider.GetRequiredService<IMediaService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationDataContext>();
                var ids = await dbContext.ProductQueues.Select(x => x.Code).Distinct().ToListAsync();
                foreach (var id in ids)
                {
                    byte? branchId = null;
                    var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Code == id, stoppingToken);
                    
                    if (product == null)
                    {
                        continue;
                    }

                    try
                    {
                        product.ImagePath = (await mediaService.UploadAsync(product.Image, product.OriginalFileName))
                            .ToString();
                    }
                    finally
                    {
                        try
                        {
                            var promotionQueueItems = await dbContext.ProductQueues
                                .Where(x => x.Code == id && x.CreatedAt <= dateTime)
                                .ToListAsync(cancellationToken: stoppingToken);

                            if (promotionQueueItems.Count > 0)
                            {
                                dbContext.ProductQueues.RemoveRange(promotionQueueItems);
                                await dbContext.SaveChangesAsync(stoppingToken);
                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // Ігноруємо: записи вже були видалені іншим інстансом.
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
