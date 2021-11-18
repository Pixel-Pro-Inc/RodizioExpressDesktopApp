using RodizioSmartRestuarant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Controller
{
    public class OrderController:BaseController
    {
        public OrderController()
        {
            _firebaseDataContext = new FirebaseDataContext();
        }

        private readonly FirebaseDataContext _firebaseDataContext;
    }
}
