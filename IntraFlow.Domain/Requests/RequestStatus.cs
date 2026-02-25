using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Domain.Requests;

public enum RequestStatus
{
    Draft = 0,
    Submitted = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}
