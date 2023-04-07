using System;
using Ddon.Core.Services.Guids;
using Ddon.Domain.Entities;
using Ddon.Domain.Entities.Auditing;
using Ddon.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Repositiry
{
    public class BasicDbContext<TDbContext> : DbContext, IDbContextSoftDelete
        where TDbContext : DbContext
    {
        public BasicDbContext(IServiceProvider serviceProvider, DbContextOptions<TDbContext> options) : base(options)
        {
            if (serviceProvider is not null)
            {
                var guidGenerator = serviceProvider.GetRequiredService<IGuidGenerator>();
                BasicDbContext.Initialize(ChangeTracker, guidGenerator);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            OnModleSoftDelete(modelBuilder);

            //var multiTenantTypes = modelBuilder.Model.GetEntityTypes().Where(x => typeof(IMultTenant<TKey>).IsAssignableFrom(x.ClrType));
            //Parallel.ForEach(multiTenantTypes, x =>
            //{
            //    modelBuilder.Entity(x.ClrType).AddQueryFilter<IMultTenant<TKey>>(e => e.TenantId.Equals(Tenant.Id));
            //});
        }


        public void OnModleSoftDelete(ModelBuilder modelBuilder)
        {
            DbContextSoftDelete.Builder(modelBuilder);
        }
    }


    public class BasicDbContext
    {
        private readonly ChangeTracker _changeTracker;
        private readonly IGuidGenerator _guidGenerator;

        public BasicDbContext(ChangeTracker changeTracker, IGuidGenerator guidGenerator)
        {
            _changeTracker = changeTracker;
            _guidGenerator = guidGenerator;
        }

        public static void Initialize(ChangeTracker changeTracker, IGuidGenerator guidGenerator)
        {
            new BasicDbContext(changeTracker, guidGenerator).Initialize();
        }

        public void Initialize()
        {
            _changeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            _changeTracker.Tracked += ChangeTracker_Tracked;
            _changeTracker.StateChanged += ChangeTracker_StateChanged;
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
            if (entry.Entity is IEntity<Guid> entityWithGuidId)
            {
                if (_guidGenerator != null) entityWithGuidId.Id = _guidGenerator.Create();
                else entityWithGuidId.Id = Guid.NewGuid();
            }

            if (entry.Entity is IEntity<string> entityWithStringId)
            {
                if (_guidGenerator != null) entityWithStringId.Id = _guidGenerator.Create().ToString();
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
