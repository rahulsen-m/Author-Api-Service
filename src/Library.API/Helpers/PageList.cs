using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Helpers
{
    // helps to add meta data information
    public class PageList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        // Check if we have any previous page
        public bool HasPreviousPage
        {
            get
            {
                return (CurrentPage > 1);
            }
        }

        // Check if we have any Next page
        public bool HasNextPage
        {
            get
            {
                return (CurrentPage < TotalPages);
            }
        }

        public PageList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        // Instead to calling the constructor directly to get/set the property value we have created a static function
        public static PageList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize).ToList();
            return new PageList<T>(items, count, pageNumber, pageSize);
        }
    }
}