using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Data
{
    public class RomsContext : DbContext
    {
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<RomFile> RomFiles { get; set; }
    }
}
