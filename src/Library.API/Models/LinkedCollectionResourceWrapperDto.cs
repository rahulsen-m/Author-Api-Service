using System.Collections.Generic;

namespace Library.API.Models
{
    // This wrapper class is used to separate the links and values (like books/author details)
    public class LinkedCollectionResourceWrapperDto<T> : LinkedResourceBaseDto 
        where T : LinkedResourceBaseDto
    {
        public IEnumerable<T> Value { get; set; }

        public LinkedCollectionResourceWrapperDto(IEnumerable<T> value)
        {
            Value = value;
        }
    }
}