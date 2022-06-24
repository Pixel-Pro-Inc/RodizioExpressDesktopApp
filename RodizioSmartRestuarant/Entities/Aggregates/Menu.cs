using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities.Aggregates
{
    /// <summary>
    /// This is an <see cref="BaseAggregates{T}"/> of <see cref="MenuItem"/>s. This provides everything that should be in Menu along with List functionality 
    /// </summary>
    public class Menu:BaseAggregates<MenuItem>
    {
    }
}
