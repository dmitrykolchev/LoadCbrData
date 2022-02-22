using Microsoft.EntityFrameworkCore;

namespace LoadCbrData.Data.Models
{
    public abstract class CbrDbContext: DbContext
    {
        protected CbrDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Record> Record { get; set; } = null!;
        public virtual DbSet<RecordId> RecordId { get; set; } = null!;

    }
}
