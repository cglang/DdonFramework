using Ddon.Core.Services.Guids;
using Ddon.Core.Services.LazyService;
using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Auditing;
using Ddon.Repositiry.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ddon.Repositiry
{
    public class BasicDbContext<TDbContext> : DbContext where TDbContext : DbContext
    {
        private readonly ILazyServiceProvider LazyServiceProvider;

        private IGuidGenerator? GuidGenerator => LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>();

        public BasicDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<TDbContext> options) : base(options)
        {
            LazyServiceProvider = lazyServiceProvider;

            Initialize();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 使用EntityFrameWork查询时过滤软删除的数据
            var softDeleteTypes = modelBuilder.Model.GetEntityTypes().Where(x => typeof(ISoftDelete).IsAssignableFrom(x.ClrType));
            Parallel.ForEach(softDeleteTypes, x =>
            {
                modelBuilder.Entity(x.ClrType).AddQueryFilter<ISoftDelete>(e => !e.IsDeleted);
            });

            //var multiTenantTypes = modelBuilder.Model.GetEntityTypes().Where(x => typeof(IMultTenant<TKey>).IsAssignableFrom(x.ClrType));
            //Parallel.ForEach(multiTenantTypes, x =>
            //{
            //    modelBuilder.Entity(x.ClrType).AddQueryFilter<IMultTenant<TKey>>(e => e.TenantId.Equals(Tenant.Id));
            //});
        }

        public virtual void Initialize()
        {
            ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            ChangeTracker.Tracked += ChangeTracker_Tracked;
            ChangeTracker.StateChanged += ChangeTracker_StateChanged;
        }

        protected virtual void ChangeTracker_Tracked(object? sender, EntityTrackedEventArgs e)
        {
            PublishEventsForTrackedEntity(e.Entry);
        }

        protected virtual void ChangeTracker_StateChanged(object? sender, EntityStateChangedEventArgs e)
        {
            PublishEventsForTrackedEntity(e.Entry);
        }

        private void PublishEventsForTrackedEntity(EntityEntry entry)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyAbpConceptsForAddedEntity(entry);
                    break;
                case EntityState.Modified:
                    ApplyAbpConceptsForModifiedEntity(entry);
                    break;
                case EntityState.Deleted:
                    ApplyAbpConceptsForDeletedEntity(entry);
                    break;
            }
        }

        protected virtual void ApplyAbpConceptsForAddedEntity(EntityEntry entry)
        {
            if (entry.Entity is IMultTenant<string> stringMultTenant)
            {
                stringMultTenant.TenantId = Guid.NewGuid().ToString();
            }

            if (entry.Entity is IEntity<Guid> entityWithGuidId)
            {
                if (GuidGenerator != null) entityWithGuidId.Id = GuidGenerator.Create();
                else entityWithGuidId.Id = Guid.NewGuid();
            }

            if (entry.Entity is IEntity<string> entityWithStringId)
            {
                if (GuidGenerator != null) entityWithStringId.Id = GuidGenerator.Create().ToString();
                else entityWithStringId.Id = Guid.NewGuid().ToString();
            }

            if (entry.Entity is IAuditedObject entity)
            {
                entity.CreationTime = DateTime.UtcNow;
            }
        }

        protected virtual void ApplyAbpConceptsForModifiedEntity(EntityEntry entry)
        {
            if (entry.Entity is IAuditedObject entity)
            {
                entity.LastModificationTime = DateTime.UtcNow;
            }
        }

        protected virtual void ApplyAbpConceptsForDeletedEntity(EntityEntry entry)
        {
            if (entry.Entity is ISoftDelete)
            {
                entry.Reload();
                entry.Entity.As<ISoftDelete>().IsDeleted = true;
                entry.State = EntityState.Modified;
            }
        }
    }
}
