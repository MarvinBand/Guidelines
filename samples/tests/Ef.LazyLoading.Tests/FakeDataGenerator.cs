using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ef.LazyLoading.Tests
{
    public static class FakeDataGenerator
    {
        public static int ParentsCount {get; set; }
        
        public static int ChildrenCount {get; set; }

        static FakeDataGenerator()
        {
            ParentsCount = 10;
            ChildrenCount = 10;
        }

        public static async Task AddFakeDataAsync(TestDbContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            for (int i = 0; i < ParentsCount; i++)
            {
                var children = new List<ChildEntity>();
                for (int j = 0; j < ChildrenCount; j++)
                {
                    children.Add(new ChildEntity()
                    {
                        StringProperty = nameof(ChildEntity) + j,
                        Int64Property = j
                    });
                }

                await context.Parents.AddAsync(new ParentEntity()
                {
                    Property = nameof(ParentEntity) + i,
                    Children = children
                });
            }
            
            await context.SaveChangesAsync();
        }
    }
}