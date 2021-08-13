using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class LayoutControlModel
    {
        public AuthenticationModel Authentication { get; set; }
        public Result Result { get; set; }

        public LayoutControlModel()
        {
            Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };
        }
    }
}