using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    /// <summary>
    /// This is the parent that the business logic recognizes is a entity
    /// </summary>
    [Serializable]
    public class BaseEntity: IBaseEntity
    {
    }
}
