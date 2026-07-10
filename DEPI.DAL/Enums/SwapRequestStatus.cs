using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Enums
{
    public enum SwapRequestStatus
    {
        PendingRecipient = 0,
        RecipientApproved = 1,
        RecipientRejected = 2,
        FinalApproved = 3,
        FinalRejected = 4
    }
}
