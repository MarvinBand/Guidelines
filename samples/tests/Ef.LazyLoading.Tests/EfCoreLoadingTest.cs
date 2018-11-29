using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ef.LazyLoading.Tests.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Ef.LazyLoading.Tests
{
    public class EfCoreLoadingTest : LoggingTestBase
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly DbContextOptions<TestDbContext> _dbOptions;
        
        public EfCoreLoadingTest(ITestOutputHelper output) : base(output)
        {
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();

            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _loggerFactory.AddProvider(new XUnitLoggerProvider(this));
            
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json").Build();
            
            var connectionString = config.GetConnectionString("pgStr");
            
            var builder = new DbContextOptionsBuilder<TestDbContext>();
            builder.UseNpgsql(connectionString);
            builder.UseLoggerFactory(_loggerFactory);
            
            _dbOptions = builder.Options;
        }

        [Fact]
        public async Task EagerLoading_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }
            
            using (var context = new TestDbContext(_dbOptions))
            {
                WriteLine("\n=======================");
                WriteLine("==== EAGER LOADING ====");
                WriteLine("=======================\n");
                
                var parents = await context
                    .Parents
                    .Include(p => p.Children)
                    .ToListAsync();
                
                Assert.NotEmpty(parents);
                parents.ForEach(p =>
                {
                    Assert.NotEmpty(p.Children);
                    Assert.Equal(FakeDataGenerator.ChildrenCount, p.Children.Count());
                });
            }  
        }

        [Fact]
        public async Task ExplicitLoading_NoFilters_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }
            
            using (var context = new TestDbContext(_dbOptions))
            {
                WriteLine("\n=======================");
                WriteLine("EXPL LOADING NO FILTERS");
                WriteLine("=======================\n");
                
                var parent = await context
                    .Parents
                    .FirstOrDefaultAsync();

                await context.Entry(parent)
                    .Collection(p => p.Children)
                    .LoadAsync();
                
                Assert.NotNull(parent);
                Assert.NotEmpty(parent.Children);
                Assert.Equal(FakeDataGenerator.ChildrenCount, parent.Children.Count());
            }
        }

        [Fact]
        public async Task ExplicitLoading_WithFilter_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }
            
            using (var context = new TestDbContext(_dbOptions))
            {
                WriteLine("\n========================");
                WriteLine("EXPL LOADING WITH FILTER");
                WriteLine("========================\n");
                
                var parent = await context
                    .Parents
                    .FirstOrDefaultAsync();

                await context.Entry(parent)
                    .Collection(p => p.Children)
                    .Query()
                    .Where(c => c.Int64Property > 0)
                    .LoadAsync();
                
                Assert.NotNull(parent);
                Assert.NotEmpty(parent.Children);
                Assert.Equal(FakeDataGenerator.ChildrenCount-1, parent.Children.Count());
            }
        }

        [Fact]
        public async Task ExplicitLoading_InNewContext_Exception_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }

            ParentEntity parent;
            using (var context = new TestDbContext(_dbOptions))
            {
                WriteLine("\n========================");
                WriteLine("EXPL LOADING WITH FILTER");
                WriteLine("========================\n");

                parent = await context
                    .Parents
                    .FirstOrDefaultAsync();
            }

            using (var context = new TestDbContext(_dbOptions))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => 
                        await context.Entry(parent)
                            .Collection(p => p.Children)
                            .Query()
                            .LoadAsync());
            }

        }

        [Fact]
        public async Task LazyLoading_NoFilters_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }
            
            using (var lazyContext = new LazyLoadingTestContext(_dbOptions))
            {
                WriteLine("\n=======================");
                WriteLine("LAZY LOADING NO FILTERS");
                WriteLine("=======================\n");

                var parent = await lazyContext
                    .Parents
                    .FirstOrDefaultAsync();
                                
                Assert.NotNull(parent);
                Assert.NotEmpty(parent.Children);
                Assert.Equal(FakeDataGenerator.ChildrenCount, parent.Children.Count());
            }       
        }

        [Fact]
        public async Task LazyLoading_WithFilter_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }
            
            using (var lazyContext = new LazyLoadingTestContext(_dbOptions))
            {
                WriteLine("\n========================");
                WriteLine("LAZY LOADING WITH FILTER");
                WriteLine("========================\n");

                var parent = await lazyContext
                    .Parents
                    .FirstOrDefaultAsync();

                var filteredChildren = parent.Children.Where(ch => ch.Int64Property > 0).ToArray();
                
                Assert.NotNull(parent);
                Assert.NotEmpty(filteredChildren);
                Assert.Equal(FakeDataGenerator.ChildrenCount -1, filteredChildren.Count());
            }
        }
        
        [Fact]
        public async Task LazyLoading_OutsideContext_Exception_Test()
        {
            using (var context = new TestDbContext(_dbOptions))
            {
                await FakeDataGenerator.AddFakeDataAsync(context);
            }

            ParentEntity parent;
            using (var lazyContext = new LazyLoadingTestContext(_dbOptions))
            {
                WriteLine("\n=======================");
                WriteLine("LAZY LOADING NO FILTERS");
                WriteLine("=======================\n");

                parent = await lazyContext
                    .Parents
                    .FirstOrDefaultAsync();
            }

            Assert.NotNull(parent);
            Assert.Throws<InvalidOperationException>(() => parent.Children.Count);
        }
    }
}