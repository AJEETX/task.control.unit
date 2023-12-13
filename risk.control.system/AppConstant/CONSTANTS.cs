namespace risk.control.system.AppConstant
{
    public class CONSTANTS
    {
        public class CASE_STATUS
        {
            public const string INITIATED = "INITIATED";
            public const string INPROGRESS = "INPROGRESS";
            public const string FINISHED = "FINISHED";

            public class CASE_SUBSTATUS
            {
                public const string CREATED_BY_CREATOR = "CREATED";
                public const string EDITED_BY_CREATOR = "EDITED";
                public const string ASSIGNED_TO_ASSIGNER = "ASSIGNED";
                public const string ALLOCATED_TO_VENDOR = "ALLOCATED";
                public const string ASSIGNED_TO_AGENT = "TASKED";
                public const string SUBMITTED_TO_SUPERVISOR = "INVESTIGATED";
                public const string SUBMITTED_TO_ASSESSOR = "SUBMITTED";
                public const string APPROVED_BY_ASSESSOR = "APPROVED";
                public const string REJECTED_BY_ASSESSOR = "REJECTED";
                public const string REASSIGNED_TO_ASSIGNER = "REASSIGNED";
                public const string WITHDRAWN_BY_COMPANY = "WITHDRAWN";
            }
        }
    }
}