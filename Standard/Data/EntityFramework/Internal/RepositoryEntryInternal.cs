using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity.Infrastructure;

namespace TraceCore.Standard.Data.EntityFramework.Internal
{
    using TraceCore.Data;

    class RepositoryEntryInternal : IRepositoryEntry
    {
        private DbEntityEntry _entry = null;

        public RepositoryEntryInternal(DbEntityEntry entry)
        {
            _entry = entry;
        }

        public object Entity
        {
            get { return _entry.Entity; }
        }

        public ItemState State
        {
            get
            {
                switch (_entry.State)
                {
                    case System.Data.Entity.EntityState.Added:
                        return ItemState.Added;

                    case System.Data.Entity.EntityState.Deleted:
                        return ItemState.Deleted;

                    case System.Data.Entity.EntityState.Detached:
                        return ItemState.Detached;

                    case System.Data.Entity.EntityState.Modified:
                        return ItemState.Modified;

                    case System.Data.Entity.EntityState.Unchanged:
                        return ItemState.Unchanged;

                    default:
                        return ItemState.Detached;
                }
            }
        }
    }
}
