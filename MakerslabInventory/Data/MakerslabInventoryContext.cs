using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MakerslabInventory.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MakerslabInventory.Data
{
    public class MakerslabInventoryContext : IdentityDbContext
    {
        public MakerslabInventoryContext (DbContextOptions<MakerslabInventoryContext> options)
            : base(options)
        {
        }

        public DbSet<MakerslabInventory.Models.Inventar> Inventar { get; set; } = default!;
        
        public DbSet<MakerslabInventory.Models.Vypozicka> Vypozicka { get; set; } = default!;
    }
}
