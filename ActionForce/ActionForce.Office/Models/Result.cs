using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class Result<T> where T : class
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public ResultType resultType { get; set; }
    }

    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public ResultType resultType { get; set; }

        public Result()
        {
            IsSuccess = false;
            Message = string.Empty;
        }
    }

    public enum ResultType
    {
        Information = 0,
        Alert = 1,
        Warning = 2
        
    }
}