using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Shared.Contracts
{
    public sealed record SemesterCreated(
        int Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate
    );
}
