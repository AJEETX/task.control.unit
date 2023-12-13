namespace risk.control.system.Models.ViewModel
{
    public class PanVerifyResponse
    {
        public string Action { get; set; }
        public string completed_at { get; set; }
        public string created_at { get; set; }
        public string group_id { get; set; }
        public string request_id { get; set; }
        public Result? result { get; set; }
        public string status { get; set; }
        public string? task_id { get; set; }
        public string? type { get; set; }
        public string? error { get; set; }
        public string? count_remain { get; set; }
    }

    public class Result
    {
        public SourceOutput source_output { get; set; }
    }

    public class SourceOutput
    {
        public bool? aadhaar_seeding_status { get; set; }
        public string first_name { get; set; }
        public object gender { get; set; }
        public string id_number { get; set; }
        public string last_name { get; set; }
        public string? middle_name { get; set; }
        public string? name_on_card { get; set; }
        public string? source { get; set; }
        public string status { get; set; }
    }

    public class PanVerifyRequest
    {
        public string task_id { get; set; }
        public string group_id { get; set; }
        public PanNumber data { get; set; }
    }

    public class PanNumber
    {
        public string id_number { get; set; }
    }

    public class PanInValidationResponse
    {
        public string error { get; set; }
        public string message { get; set; }
        public int status { get; set; }
    }

    public class PanValidationResponse
    {
        public string @entity { get; set; }
        public string pan { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
    }

    public class FaceMatchDetail
    {
        public decimal FaceLeftCoordinate { get; set; }
        public decimal FaceTopCcordinate { get; set; }
        public string Confidence { get; set; }
    }

    public class FaceImageDetail
    {
        public string DocType { get; set; }
        public string DocumentId { get; set; }
        public string MaskedImage { get; set; }
        public string? OcrData { get; set; }
    }

    public class MatchImage
    {
        public string Source { get; set; }
        public string Dest { get; set; }
    }

    public class MaskImage
    {
        public string Image { get; set; }
    }

    public class SubmitData
    {
        public string Email { get; set; }
        public string ClaimId { get; set; }
        public long BeneficiaryId { get; set; }
        public string? Question1 { get; set; }
        public string? Question2 { get; set; }
        public string? Question3 { get; set; }
        public string? Question4 { get; set; }
        public string Remarks { get; set; }
    }

    public class FaceData
    {
        public string Email { get; set; }
        public string ClaimId { get; set; }
        public string LocationImage { get; set; }
        public string LocationData { get; set; }
        public string LocationLongLat { get; set; }
    }

    public class DocumentData
    {
        public string Email { get; set; }
        public string ClaimId { get; set; }
        public string OcrImage { get; set; }
        public string OcrLongLat { get; set; }
    }

    public class VendorData
    {
        public byte[]? Image { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Addressline { get; set; }
        public string State { get; set; }
        public string Created { get; set; }
    }

    public class VendorDataDataTable
    {
        public List<VendorData> data { get; set; }
    }
}