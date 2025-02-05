using System;
using System.Collections.Generic;

namespace RestaurantePro.Common.Results
{
    public class PagedResult<T>
    {
        public bool IsSuccess { get; private set; }
        public IEnumerable<T> Items { get; private set; }
        public string Error { get; private set; }
        public int TotalCount { get; private set; }
        public int PageSize { get; private set; }
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }

        private PagedResult(IEnumerable<T> items, int totalCount, int pageSize, int currentPage)
        {
            IsSuccess = true;
            Items = items;
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageSize, int currentPage)
        {
            return new PagedResult<T>(items, totalCount, pageSize, currentPage);
        }

        public static PagedResult<T> Failure(string error)
        {
            return new PagedResult<T>(null, 0, 0, 0) 
            { 
                IsSuccess = false, 
                Error = error 
            };
        }
    }
}