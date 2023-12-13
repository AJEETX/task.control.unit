using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using risk.control.system.Helpers;
using risk.control.system.Models;
using System.Net.Http;

using risk.control.system.Models.ViewModel;
using risk.control.system.Controllers.Api;
using risk.control.system.Data;

namespace risk.control.system.Services
{
    public interface IICheckifyService
    {
        Task<AppiCheckifyResponse> GetFaceId(FaceData data);

        Task<AppiCheckifyResponse> GetDocumentId(DocumentData data);
    }

    public class ICheckifyService : IICheckifyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientService httpClientService;
        private readonly IClaimsInvestigationService claimsInvestigationService;
        private readonly IMailboxService mailboxService;
        private readonly IWebHostEnvironment webHostEnvironment;
        private static HttpClient httpClient = new();

        private ILogger<AgentController> logger;

        //test PAN FNLPM8635N
        public ICheckifyService(ApplicationDbContext context, IHttpClientService httpClientService, IClaimsInvestigationService claimsInvestigationService, IMailboxService mailboxService, IWebHostEnvironment webHostEnvironment, ILogger<AgentController> logger)
        {
            this._context = context;
            this.httpClientService = httpClientService;
            this.claimsInvestigationService = claimsInvestigationService;
            this.mailboxService = mailboxService;
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }

        public async Task<AppiCheckifyResponse> GetFaceId(FaceData data)
        {
            var claimCase = _context.CaseLocation
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.ClaimReport)
                .Include(c => c.PinCode)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.Country)
                .FirstOrDefault(c => c.ClaimsInvestigationId == data.ClaimId);

            if (claimCase == null)
            {
                return null;
            }
            claimCase.ClaimReport.AgentEmail = data.Email;

            var claim = _context.ClaimsInvestigation
            .Include(c => c.PolicyDetail)
            .Include(c => c.CustomerDetail)
                .FirstOrDefault(c => c.ClaimsInvestigationId == data.ClaimId);

            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claim.PolicyDetail.ClientCompanyId);

            #region FACE IMAGE PROCESSING

            //var (claimWithDigitalId, ClaimCaseWithDitialId) = await ProcessDigitalId(data, claim, claimCase);
            if (!string.IsNullOrWhiteSpace(data.LocationImage))
            {
                byte[]? registeredImage = null;
                this.logger.LogInformation("DIGITAL ID : FACE image {LocationImage} ", data.LocationImage);

                if (claim.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredImage = claim.CustomerDetail.ProfilePicture;
                    this.logger.LogInformation("DIGITAL ID : HEALTH image {registeredImage} ", registeredImage);
                }
                if (claim.PolicyDetail.ClaimType == ClaimType.DEATH)
                {
                    registeredImage = claimCase.ProfilePicture;
                    this.logger.LogInformation("DIGITAL ID : DEATH image {registeredImage} ", registeredImage);
                }

                string ImageData = string.Empty;
                try
                {
                    if (registeredImage != null)
                    {
                        var image = Convert.FromBase64String(data.LocationImage);
                        var locationRealImage = ByteArrayToImage(image);
                        MemoryStream stream = new MemoryStream(image);
                        var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"loc{DateTime.UtcNow.ToString("dd-MMM-yyyy-HH-mm-ss")}.{locationRealImage.ImageType()}");
                        claimCase.ClaimReport.AgentLocationPictureUrl = filePath;
                        CompressImage.Compressimage(stream, filePath);

                        var savedImage = await File.ReadAllBytesAsync(filePath);
                        claimCase.ClaimReport.AgentLocationPicture = savedImage;
                        var saveImageBase64String = Convert.ToBase64String(savedImage);

                        claimCase.ClaimReport.LocationLongLatTime = DateTime.UtcNow;
                        this.logger.LogInformation("DIGITAL ID : saved image {registeredImage} ", registeredImage);

                        var base64Image = Convert.ToBase64String(registeredImage);

                        this.logger.LogInformation("DIGITAL ID : HEALTH image {base64Image} ", base64Image);
                        try
                        {
                            var faceImageDetail = await httpClientService.GetFaceMatch(new MatchImage { Source = base64Image, Dest = saveImageBase64String }, company.ApiBaseUrl);

                            claimCase.ClaimReport.LocationPictureConfidence = faceImageDetail.Confidence;
                        }
                        catch (Exception)
                        {
                            claimCase.ClaimReport.LocationPictureConfidence = string.Empty;
                        }
                    }
                    else
                    {
                        claimCase.ClaimReport.LocationPictureConfidence = "no face image";
                    }
                }
                catch (Exception ex)
                {
                    claimCase.ClaimReport.LocationPictureConfidence = "err " + ImageData;
                }
                if (registeredImage == null)
                {
                    claimCase.ClaimReport.LocationPictureConfidence = "no image";
                }
            }

            #endregion FACE IMAGE PROCESSING

            if (!string.IsNullOrWhiteSpace(data.LocationLongLat))
            {
                claimCase.ClaimReport.LocationLongLatTime = DateTime.UtcNow;
                claimCase.ClaimReport.LocationLongLat = data.LocationLongLat;
            }

            if (!string.IsNullOrWhiteSpace(claimCase.ClaimReport.LocationLongLat))
            {
                var longLat = claimCase.ClaimReport.LocationLongLat.IndexOf("/");
                var latitude = claimCase.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                var longitude = claimCase.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();
                var latLongString = latitude + "," + longitude;
                var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,windspeed_10m&hourly=temperature_2m,relativehumidity_2m,windspeed_10m";
                var weatherData = await httpClient.GetFromJsonAsync<Weather>(weatherUrl);
                string weatherCustomData = $"Temperature:{weatherData.current.temperature_2m} {weatherData.current_units.temperature_2m}." +
                    $"\r\n" +
                    $"\r\nWindspeed:{weatherData.current.windspeed_10m} {weatherData.current_units.windspeed_10m}" +
                    $"\r\n" +
                    $"\r\nElevation(sea level):{weatherData.elevation} metres";
                claimCase.ClaimReport.LocationData = weatherCustomData;
            }

            _context.CaseLocation.Update(claimCase);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw exception;
            }

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "no-photo.jpg");

            var noDataimage = await File.ReadAllBytesAsync(noDataImagefilePath);
            return new AppiCheckifyResponse
            {
                BeneficiaryId = claimCase.CaseLocationId,
                LocationImage = !string.IsNullOrWhiteSpace(claimCase.ClaimReport.AgentLocationPictureUrl) ?
                Convert.ToBase64String(File.ReadAllBytes(claimCase.ClaimReport.AgentLocationPictureUrl)) :
                Convert.ToBase64String(noDataimage),
                LocationLongLat = claimCase.ClaimReport.LocationLongLat,
                LocationTime = claimCase.ClaimReport.LocationLongLatTime,
                OcrImage = !string.IsNullOrWhiteSpace(claimCase.ClaimReport.AgentOcrUrl) ?
                Convert.ToBase64String(File.ReadAllBytes(claimCase.ClaimReport.AgentOcrUrl)) :
                Convert.ToBase64String(noDataimage),
                OcrLongLat = claimCase.ClaimReport.OcrLongLat,
                OcrTime = claimCase.ClaimReport.OcrLongLatTime,
                FacePercent = claimCase.ClaimReport.LocationPictureConfidence,
                PanValid = claimCase.ClaimReport.PanValid
            };
        }

        private System.Drawing.Image? ByteArrayToImage(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        public async Task<AppiCheckifyResponse> GetDocumentId(DocumentData data)
        {
            var claimCase = _context.CaseLocation
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.ClaimReport)
                .Include(c => c.PinCode)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.Country)
                .FirstOrDefault(c => c.ClaimsInvestigationId == data.ClaimId);

            if (claimCase == null)
            {
                return null;
            }
            claimCase.ClaimReport.AgentEmail = data.Email;

            var claim = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .Include(c => c.CustomerDetail)
                .FirstOrDefault(c => c.ClaimsInvestigationId == data.ClaimId);

            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claim.PolicyDetail.ClientCompanyId);

            #region PAN IMAGE PROCESSING

            if (!string.IsNullOrWhiteSpace(data.OcrImage))
            {
                var byteimage = Convert.FromBase64String(data.OcrImage);

                var locationRealImage = ByteArrayToImage(byteimage);
                MemoryStream mstream = new MemoryStream(byteimage);
                var mfilePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"loc{DateTime.UtcNow.ToString("dd-MMM-yyyy-HH-mm-ss")}.{locationRealImage.ImageType()}");
                claimCase.ClaimReport.AgentLocationPictureUrl = mfilePath;
                CompressImage.Compressimage(mstream, mfilePath);

                var savedImage = await File.ReadAllBytesAsync(mfilePath);

                var base64Image = Convert.ToBase64String(savedImage);
                var inputImage = new MaskImage { Image = base64Image };

                this.logger.LogInformation("DOCUMENT ID : PAN image {ocrImage} ", data.OcrImage);

                var maskedImage = await httpClientService.GetMaskedImage(inputImage, company.ApiBaseUrl);

                this.logger.LogInformation("DOCUMENT ID : PAN maskedImage image {maskedImage} ", maskedImage);
                if (maskedImage != null)
                {
                    try
                    {
                        #region// PAN VERIFICATION ::: //test PAN FNLPM8635N, BYSPP5796F

                        if (maskedImage.DocType.ToUpper() == "PAN")
                        {
                            if (company.VerifyOcr)
                            {
                                try
                                {
                                    var body = await httpClientService.VerifyPan(maskedImage.DocumentId, company.PanIdfyUrl, company.RapidAPIKey, company.RapidAPITaskId, company.RapidAPIGroupId);
                                    company.RapidAPIPanRemainCount = body.count_remain;

                                    if (body != null && body?.status == "completed" &&
                                        body?.result != null &&
                                        body.result?.source_output != null
                                        && body.result?.source_output?.status == "id_found")
                                    {
                                        claimCase.ClaimReport.PanValid = true;
                                    }
                                    else
                                    {
                                        claimCase.ClaimReport.PanValid = false;
                                    }
                                }
                                catch (Exception)
                                {
                                    claimCase.ClaimReport.PanValid = false;
                                }
                            }
                            else
                            {
                                claimCase.ClaimReport.PanValid = true;
                            }
                        }

                        #endregion PAN IMAGE PROCESSING

                        var image = Convert.FromBase64String(maskedImage.MaskedImage);
                        var OcrRealImage = ByteArrayToImage(image);
                        MemoryStream stream = new MemoryStream(image);
                        claimCase.ClaimReport.AgentOcrPicture = image;
                        var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"{maskedImage.DocType}{DateTime.UtcNow.ToString("dd-MMM-yyyy-HH-mm-ss")}.{OcrRealImage.ImageType()}");
                        CompressImage.Compressimage(stream, filePath);
                        claimCase.ClaimReport.AgentOcrUrl = filePath;
                        claimCase.ClaimReport.OcrLongLatTime = DateTime.UtcNow;
                        claimCase.ClaimReport.ImageType = maskedImage.DocType;
                        claimCase.ClaimReport.AgentOcrData = maskedImage.DocType + " data: ";

                        if (!string.IsNullOrWhiteSpace(maskedImage.OcrData))
                        {
                            claimCase.ClaimReport.AgentOcrData = claimCase.ClaimReport.AgentOcrData + ". \r\n " +
                                "" + maskedImage.OcrData.Replace(maskedImage.DocumentId, "xxxxxxxxxx");
                        }
                    }
                    catch (Exception)
                    {
                        var image = Convert.FromBase64String(maskedImage.MaskedImage);
                        var OcrRealImage = ByteArrayToImage(image);
                        MemoryStream stream = new MemoryStream(image);
                        claimCase.ClaimReport.AgentOcrPicture = image;
                        var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"{maskedImage.DocType}{DateTime.UtcNow.ToString("dd-MMM-yyyy-HH-mm-ss")}.{OcrRealImage.ImageType()}");
                        claimCase.ClaimReport.AgentOcrUrl = filePath;
                        CompressImage.Compressimage(stream, filePath);
                        claimCase.ClaimReport.OcrLongLatTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    this.logger.LogInformation("DOCUMENT ID : PAN maskedImage image {maskedImage} ", maskedImage);
                    var image = Convert.FromBase64String(data.OcrImage);
                    var OcrRealImage = ByteArrayToImage(image);
                    MemoryStream stream = new MemoryStream(image);
                    claimCase.ClaimReport.AgentOcrPicture = image;
                    var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"ocr{DateTime.UtcNow.ToString("dd-MMM-yyyy-HH-mm-ss")}.{OcrRealImage.ImageType()}");
                    CompressImage.Compressimage(stream, filePath);
                    claimCase.ClaimReport.AgentOcrUrl = filePath;
                    claimCase.ClaimReport.OcrLongLatTime = DateTime.UtcNow;
                    claimCase.ClaimReport.AgentOcrData = "no data: ";
                }
            }

            #endregion PAN IMAGE PROCESSING

            if (!string.IsNullOrWhiteSpace(data.OcrLongLat))
            {
                claimCase.ClaimReport.OcrLongLat = data.OcrLongLat;
                claimCase.ClaimReport.OcrLongLatTime = DateTime.UtcNow;
            }

            _context.CaseLocation.Update(claimCase);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw exception;
            }

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "no-photo.jpg");

            var noDataimage = await File.ReadAllBytesAsync(noDataImagefilePath);
            return new AppiCheckifyResponse
            {
                BeneficiaryId = claimCase.CaseLocationId,
                LocationImage = !string.IsNullOrWhiteSpace(claimCase.ClaimReport.AgentLocationPictureUrl) ?
                Convert.ToBase64String(System.IO.File.ReadAllBytes(claimCase.ClaimReport.AgentLocationPictureUrl)) :
                Convert.ToBase64String(noDataimage),
                LocationLongLat = claimCase.ClaimReport.LocationLongLat,
                LocationTime = claimCase.ClaimReport.LocationLongLatTime,
                OcrImage = !string.IsNullOrWhiteSpace(claimCase.ClaimReport.AgentOcrUrl) ?
                Convert.ToBase64String(System.IO.File.ReadAllBytes(claimCase.ClaimReport.AgentOcrUrl)) :
                Convert.ToBase64String(noDataimage),
                OcrLongLat = claimCase.ClaimReport.OcrLongLat,
                OcrTime = claimCase.ClaimReport.OcrLongLatTime,
                FacePercent = claimCase.ClaimReport.LocationPictureConfidence,
                PanValid = claimCase.ClaimReport.PanValid
            };
        }
    }
}