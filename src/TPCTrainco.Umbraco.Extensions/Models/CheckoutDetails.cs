using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPCTrainco.Umbraco.Extensions.Models.Account;

namespace TPCTrainco.Umbraco.Extensions.Models
{
    public class CheckoutDetails
    {
        public string CartGuid { get; set; }
        public int RegId { get; set; }

        public string UserToken { get; set; }
        public string Redirect { get; set; }

        public List<temp_Reg> tempRegList { get; set; }
        public List<temp_Att> tempAttList { get; set; }
        public temp_Cust tempCust { get; set; }

        public bool CreateAccount { get; set; }

        public UserModel User { get; set; }
        public CompanyModel Company { get; set; }
        public Dictionary<int, string> seminarTypes { get; set; }

    }
}
