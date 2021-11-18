using RodizioSmartRestuarant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Controller
{
    public class BaseController
    {
        public BaseController()
        {
            _firebaseDataContext = new FirebaseDataContext();
        }

        private readonly FirebaseDataContext _firebaseDataContext;
    }
}
