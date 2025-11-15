using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Shared;

public enum ErrorType
{
    Failure = 0,
    BadRequest = 1,
    NotFound = 2,   
    Unauthorized = 3,
    Forbidden = 4
}
