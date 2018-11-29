using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Ef.LazyLoading.Tests
{
    public class TestDbContext : DbContext
    {
        public virtual DbSet<ParentEntity> Parents { get; set; }
        public virtual DbSet<ChildEntity> Children { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(p => p.Children)
                    .WithOne(c => c.Parent)
                    .HasForeignKey(c => c.ParentId);
            });
        }
    }

    public class LazyLoadingTestContext : TestDbContext
    {
        public LazyLoadingTestContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies();
    }

    public class ParentEntity
    {
        public long Id { get; set; }
        
        public string Property { get; set; }
        
        public virtual ICollection<ChildEntity> Children { get; set; }
    }

    public class ChildEntity
    {
        public long Id { get; set; }
        
        public string StringProperty { get; set; }
        
        public long Int64Property { get; set; }

        public long ParentId { get; set; }
        
        public virtual ParentEntity Parent { get; set; }
    }
}