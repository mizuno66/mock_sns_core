using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mock_sns_core2.Services
{
    public class PaginatedService<T> : List<T>
    {
        public int Index { get; private set; }
        public int Total { get; private set; }

        public PaginatedService(List<T> list, int count, int index, int dispSize)
        {
            this.Index = index;
            this.Total = (int)Math.Ceiling(count / (double)dispSize);

            this.AddRange(list);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (this.Index > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (this.Index < this.Total);
            }
        }

        public static PaginatedService<T> Create(IEnumerable<T> source, int index, int dispSize)
        {
            var list = source.Skip((index - 1) * dispSize).Take(dispSize).ToList();
            return new PaginatedService<T>(list, source.ToList().Count, index, dispSize);
        }
    }
}
