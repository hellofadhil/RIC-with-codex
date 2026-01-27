using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Interfaces
{
    public interface ICreatable
    {
        DateTimeOffset Created { get; set; }
        string CreatedBy { get; set; }
    }

}
