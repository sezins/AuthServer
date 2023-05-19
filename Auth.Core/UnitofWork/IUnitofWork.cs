using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Core.UnitofWork
{
    public interface IUnitofWork
    {
        void Commit();
        Task CommitAsync();
    }
}
