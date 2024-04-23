using DFApp.Aria2.Repository.Response.TellStatus;
using DFApp.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DFApp.Aria2.Response.TellStatus
{
    public class FilesItemRepository : EfCoreRepository<DFAppDbContext, FilesItem>, IFilesItemRepository
    {
        public FilesItemRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}
