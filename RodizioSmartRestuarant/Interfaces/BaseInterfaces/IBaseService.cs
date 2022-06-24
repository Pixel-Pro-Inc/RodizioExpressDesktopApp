using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Interfaces
{
    /// <summary>
    /// This is an application logic so that other layers that should be contracted to Services can use this to define that contract
    /// <para> Note the semantics I used there, 'other layers', which means you should only really use services to have different layers talk to each other.
    ///  Or different parts of the same layer talking to each other. That means, you shouldn't be having controller instances newed up in each other, you should instead
    ///  be pulling in a service scope that is responsible for that duty</para>
    /// </summary>
    public interface IBaseService
    {
    }
}
