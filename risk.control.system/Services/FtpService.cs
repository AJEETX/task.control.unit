using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

using System.Data;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace risk.control.system.Services
{
    public interface IFtpService
    {
        Task UploadFile(string userEmail, string filePath, string docPath, string fileNameWithoutExtension);

        Task DownloadFtp(string userEmail);
    }

    public class FtpService : IFtpService
    {
        private readonly JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        private static readonly string[] firstNames = { "John", "Paul", "Ringo", "George", "Laura", "Stephaney" };
        private static readonly string[] lastNames = { "Lennon", "McCartney", "Starr", "Harrison", "Blanc" };

        private static string NO_DATA = " NO - DATA ";
        private static Regex regex = new Regex("\\\"(.*?)\\\"");
        private readonly ApplicationDbContext _context;
        private readonly IClaimsInvestigationService claimsInvestigationService;
        private readonly IMailboxService mailboxService;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IToastNotification toastNotification;
        private static HttpClient httpClient = new();

        public FtpService(ApplicationDbContext context,
            IClaimsInvestigationService claimsInvestigationService,
            IMailboxService mailboxService,
            UserManager<ClientCompanyApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<ApplicationRole> roleManager,
            IToastNotification toastNotification)
        {
            _context = context;
            this.claimsInvestigationService = claimsInvestigationService;
            this.mailboxService = mailboxService;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this.roleManager = roleManager;
            this.toastNotification = toastNotification;
        }

        public async Task DownloadFtp(string userEmail)
        {
            string path = Path.Combine(webHostEnvironment.WebRootPath, "download-file");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string docPath = Path.Combine(webHostEnvironment.WebRootPath, "download-case");
            if (!Directory.Exists(docPath))
            {
                Directory.CreateDirectory(docPath);
            }
            var files = GetFtpData();
            var zipFiles = files.Where(f => f.EndsWith(".zip"));
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(Applicationsettings.FTP_SITE_LOG, Applicationsettings.FTP_SITE_DATA);

            foreach (var zipFile in zipFiles)
            {
                string fileName = zipFile;
                string fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 4);
                string filePath = Path.Combine(path, fileName);

                string ftpPath = $"{Applicationsettings.FTP_SITE}/{zipFile}";

                client.DownloadFile(ftpPath, filePath);

                using var archive = ZipFile.OpenRead(filePath);

                string zipFilePath = Path.Combine(docPath, fileNameWithoutExtension);

                ZipFile.ExtractToDirectory(filePath, zipFilePath, true);

                var dirNames = Directory.EnumerateDirectories(zipFilePath);
                var fileNames = Directory.EnumerateFiles(dirNames.FirstOrDefault());

                string csvData = await System.IO.File.ReadAllTextAsync(fileNames.First());

                var status = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.Contains(CONSTANTS.CASE_STATUS.INITIATED));
                var subStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.Contains(CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR));

                var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

                DataTable dt = new DataTable();
                bool firstRow = true;
                foreach (string row in csvData.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            if (firstRow)
                            {
                                foreach (string cell in row.Split(','))
                                {
                                    dt.Columns.Add(cell.Trim());
                                }
                                firstRow = false;
                            }
                            else
                            {
                                try
                                {
                                    dt.Rows.Add();
                                    int i = 0;
                                    var output = regex.Replace(row, m => m.Value.Replace(',', '@'));
                                    var rowData = output.Split(',').ToList();
                                    foreach (string cell in rowData)
                                    {
                                        dt.Rows[dt.Rows.Count - 1][i] = cell?.Trim() ?? NO_DATA;
                                        i++;
                                    }
                                    var claim = new ClaimsInvestigation
                                    {
                                        InvestigationCaseStatusId = status.InvestigationCaseStatusId,
                                        InvestigationCaseStatus = status,
                                        InvestigationCaseSubStatusId = subStatus.InvestigationCaseSubStatusId,
                                        InvestigationCaseSubStatus = subStatus,
                                        Updated = DateTime.UtcNow,
                                        UpdatedBy = userEmail,
                                        CurrentUserEmail = userEmail,
                                        CurrentClaimOwner = userEmail,
                                        Created = DateTime.UtcNow,
                                        Deleted = false,
                                        HasClientCompany = true,
                                        IsReady2Assign = true,
                                        IsReviewCase = false,
                                        SelectedToAssign = false,
                                    };

                                    var servicetype = _context.InvestigationServiceType.FirstOrDefault(s => s.Code.ToLower() == (rowData[4].Trim().ToLower()));

                                    var policyImagePath = Path.Combine(dirNames.FirstOrDefault(), rowData[0].Trim(), "POLICY.jpg");

                                    var image = System.IO.File.ReadAllBytes(policyImagePath);
                                    dt.Rows[dt.Rows.Count - 1][9] = $"{Convert.ToBase64String(image)}";
                                    claim.PolicyDetail = new PolicyDetail
                                    {
                                        ContractNumber = rowData[0]?.Trim(),
                                        SumAssuredValue = Convert.ToDecimal(rowData[1]?.Trim()),
                                        ContractIssueDate = DateTime.UtcNow.AddDays(-20),
                                        ClaimType = (ClaimType)Enum.Parse(typeof(ClaimType), rowData[3]?.Trim()),
                                        InvestigationServiceTypeId = servicetype?.InvestigationServiceTypeId,
                                        DateOfIncident = DateTime.UtcNow.AddDays(-5),
                                        CauseOfLoss = rowData[6]?.Trim(),
                                        CaseEnablerId = _context.CaseEnabler.FirstOrDefault(c => c.Code.ToLower() == rowData[7].Trim().ToLower()).CaseEnablerId,
                                        CostCentreId = _context.CostCentre.FirstOrDefault(c => c.Code.ToLower() == rowData[8].Trim().ToLower()).CostCentreId,
                                        LineOfBusinessId = _context.LineOfBusiness.FirstOrDefault(l => l.Code.ToLower() == "claims")?.LineOfBusinessId,
                                        ClientCompanyId = companyUser?.ClientCompanyId,
                                        DocumentImage = image
                                    };

                                    var pinCode = _context.PinCode.Include(p => p.District).Include(p => p.State).FirstOrDefault(p => p.Code == rowData[19].Trim());

                                    var district = _context.District.FirstOrDefault(c => c.DistrictId == pinCode.District.DistrictId);

                                    var state = _context.State.FirstOrDefault(s => s.StateId == pinCode.State.StateId);

                                    var country = _context.Country.FirstOrDefault();

                                    var customerImagePath = Path.Combine(dirNames.FirstOrDefault(), rowData[0].Trim(), "CUSTOMER.jpg");

                                    var customerImage = System.IO.File.ReadAllBytes(customerImagePath);

                                    dt.Rows[dt.Rows.Count - 1][21] = $"{Convert.ToBase64String(customerImage)}";

                                    claim.CustomerDetail = new CustomerDetail
                                    {
                                        CustomerName = rowData[10]?.Trim(),
                                        CustomerType = (CustomerType)Enum.Parse(typeof(CustomerType), rowData[11]?.Trim()),
                                        Gender = (Gender)Enum.Parse(typeof(Gender), rowData[12]?.Trim()),
                                        CustomerDateOfBirth = DateTime.UtcNow.AddYears(-20),
                                        ContactNumber = Convert.ToInt64(rowData[14]?.Trim()),
                                        CustomerEducation = (Education)Enum.Parse(typeof(Education), rowData[15]?.Trim()),
                                        CustomerOccupation = (Occupation)Enum.Parse(typeof(Occupation), rowData[16]?.Trim()),
                                        CustomerIncome = (Income)Enum.Parse(typeof(Income), rowData[17]?.Trim()),
                                        Addressline = rowData[18]?.Trim(),
                                        CountryId = country.CountryId,
                                        PinCodeId = pinCode.PinCodeId,
                                        StateId = state.StateId,
                                        DistrictId = district.DistrictId,
                                        Description = rowData[20]?.Trim(),
                                        ProfilePicture = customerImage
                                    };

                                    var benePinCode = _context.PinCode.Include(p => p.District).Include(p => p.State).FirstOrDefault(p => p.Code == rowData[28].Trim());

                                    var beneDistrict = _context.District.FirstOrDefault(c => c.DistrictId == benePinCode.District.DistrictId);

                                    var beneState = _context.State.FirstOrDefault(s => s.StateId == benePinCode.State.StateId);
                                    var relation = _context.BeneficiaryRelation.FirstOrDefault(b => b.Code.ToLower() == rowData[23].Trim().ToLower());

                                    var beneficairyImagePath = Path.Combine(dirNames.FirstOrDefault(), rowData[0].Trim(), "BENEFICIARY.jpg");

                                    var beneficairyImage = System.IO.File.ReadAllBytes(beneficairyImagePath);
                                    dt.Rows[dt.Rows.Count - 1][29] = $"{Convert.ToBase64String(beneficairyImage)}";

                                    var beneficairy = new CaseLocation
                                    {
                                        BeneficiaryName = rowData[22]?.Trim(),
                                        BeneficiaryRelationId = relation.BeneficiaryRelationId,
                                        BeneficiaryDateOfBirth = DateTime.UtcNow.AddYears(-22),
                                        BeneficiaryIncome = (Income)Enum.Parse(typeof(Income), rowData[25]?.Trim()),
                                        BeneficiaryContactNumber = Convert.ToInt64(rowData[26]?.Trim()),
                                        Addressline = rowData[27]?.Trim(),
                                        PinCodeId = benePinCode.PinCodeId,
                                        DistrictId = beneDistrict.DistrictId,
                                        StateId = beneState.StateId,
                                        CountryId = country.CountryId,
                                        InvestigationCaseSubStatusId = subStatus.InvestigationCaseSubStatusId,
                                        ProfilePicture = beneficairyImage
                                    };

                                    var addedClaim = _context.ClaimsInvestigation.Add(claim);

                                    beneficairy.ClaimsInvestigationId = addedClaim.Entity.ClaimsInvestigationId;

                                    _context.CaseLocation.Add(beneficairy);

                                    var log = new InvestigationTransaction
                                    {
                                        ClaimsInvestigationId = addedClaim.Entity.ClaimsInvestigationId,
                                        CurrentClaimOwner = userEmail,
                                        Created = DateTime.UtcNow,
                                        HopCount = 0,
                                        Time2Update = 0,
                                        InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId,
                                        InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId,
                                        UpdatedBy = userEmail
                                    };
                                    _context.InvestigationTransaction.Add(log);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.StackTrace);
                                    throw ex;
                                }
                            }
                        }
                    }
                }
                var dataObject = ConvertDataTable<UploadClaim>(dt);
                _context.UploadClaim.AddRange(dataObject);
                var company = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

                //await SaveTheClaims(dataObject);
                var fileModel = new FileOnFileSystemModel
                {
                    CreatedOn = DateTime.UtcNow,
                    FileType = "application/x-zip-compressed",
                    Extension = "zip",
                    Name = fileName,
                    Description = "Ftp Download",
                    FilePath = filePath,
                    UploadedBy = userEmail,
                    CompanyId = company.ClientCompanyId
                };
                _context.FilesOnFileSystem.Add(fileModel);
            }

            var rows = _context.SaveChanges();

            foreach (var zipFile in zipFiles)
            {
                string fileName = zipFile;

                FtpWebRequest requestFileDelete = (FtpWebRequest)WebRequest.Create($"{Applicationsettings.FTP_SITE}" + fileName);
                requestFileDelete.Credentials = new NetworkCredential(Applicationsettings.FTP_SITE_LOG, Applicationsettings.FTP_SITE_DATA);
                requestFileDelete.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse responseFileDelete = (FtpWebResponse)requestFileDelete.GetResponse();
            }
        }

        private List<string> GetFtpData()
        {
            var request = WebRequest.Create(Applicationsettings.FTP_SITE);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(Applicationsettings.FTP_SITE_LOG, Applicationsettings.FTP_SITE_DATA);

            var files = new List<string>();

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, true))
                    {
                        while (!reader.EndOfStream)
                        {
                            var file = reader.ReadLine();
                            //Make sure you only get the filename and not the whole path.
                            file = file.Substring(file.LastIndexOf('/') + 1);
                            //The root folder will also be added, this can of course be ignored.
                            if (!file.StartsWith("."))
                                files.Add(file);
                        }
                    }
                }
            }

            return files;
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public async Task UploadFile(string userEmail, string filePath, string docPath, string fileNameWithoutExtension)
        {
            using var archive = ZipFile.OpenRead(filePath);

            string zipFilePath = Path.Combine(webHostEnvironment.WebRootPath, "upload-case", fileNameWithoutExtension);

            ZipFile.ExtractToDirectory(filePath, zipFilePath, true);

            var dirNames = Directory.EnumerateDirectories(zipFilePath);
            var fileNameWithPath = dirNames.FirstOrDefault();
            string fileName = Path.GetFileName(fileNameWithPath);

            string fileNameWithData = Path.Combine(webHostEnvironment.WebRootPath, "upload-case", fileNameWithoutExtension, fileName, "CLAIMS.csv");

            var fileNames = Directory.EnumerateFiles(fileNameWithData);

            string csvData = await System.IO.File.ReadAllTextAsync(fileNameWithData);

            var status = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.Contains(CONSTANTS.CASE_STATUS.INITIATED));
            var subStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.Contains(CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR));

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            DataTable dt = new DataTable();
            bool firstRow = true;
            foreach (string row in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(row))
                {
                    if (firstRow)
                    {
                        foreach (string cell in row.Split(','))
                        {
                            dt.Columns.Add(cell.Trim());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        try
                        {
                            dt.Rows.Add();
                            int i = 0;
                            var output = regex.Replace(row, m => m.Value.Replace(',', '@'));
                            var rowData = output.Split(',').ToList();
                            foreach (string cell in rowData)
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell?.Trim() ?? NO_DATA;
                                i++;
                            }
                            var claim = new ClaimsInvestigation
                            {
                                InvestigationCaseStatusId = status.InvestigationCaseStatusId,
                                InvestigationCaseStatus = status,
                                InvestigationCaseSubStatusId = subStatus.InvestigationCaseSubStatusId,
                                InvestigationCaseSubStatus = subStatus,
                                Updated = DateTime.UtcNow,
                                UpdatedBy = userEmail,
                                CurrentUserEmail = userEmail,
                                CurrentClaimOwner = userEmail,
                                Created = DateTime.UtcNow,
                                Deleted = false,
                                HasClientCompany = true,
                                IsReady2Assign = true,
                                IsReviewCase = false,
                                SelectedToAssign = false,
                            };

                            var servicetype = _context.InvestigationServiceType.FirstOrDefault(s => s.Code.ToLower() == (rowData[4].Trim().ToLower()));

                            var policyImagePath = Path.Combine(webHostEnvironment.WebRootPath, "upload-case", fileNameWithoutExtension, fileName, rowData[0].Trim(), "POLICY.jpg");

                            var image = File.ReadAllBytes(policyImagePath);

                            using MemoryStream stream = new MemoryStream(image);

                            CompressImage.Compressimage(stream, policyImagePath);

                            var savedImage = await File.ReadAllBytesAsync(policyImagePath);

                            dt.Rows[dt.Rows.Count - 1][9] = $"{Convert.ToBase64String(savedImage)}";
                            claim.PolicyDetail = new PolicyDetail
                            {
                                ContractNumber = rowData[0]?.Trim(),
                                SumAssuredValue = Convert.ToDecimal(rowData[1]?.Trim()),
                                ContractIssueDate = DateTime.UtcNow.AddDays(-20),
                                ClaimType = (ClaimType)Enum.Parse(typeof(ClaimType), rowData[3]?.Trim()),
                                InvestigationServiceTypeId = servicetype?.InvestigationServiceTypeId,
                                DateOfIncident = DateTime.UtcNow.AddDays(-5),
                                CauseOfLoss = rowData[6]?.Trim(),
                                CaseEnablerId = _context.CaseEnabler.FirstOrDefault(c => c.Code.ToLower() == rowData[7].Trim().ToLower()).CaseEnablerId,
                                CostCentreId = _context.CostCentre.FirstOrDefault(c => c.Code.ToLower() == rowData[8].Trim().ToLower()).CostCentreId,
                                LineOfBusinessId = _context.LineOfBusiness.FirstOrDefault(l => l.Code.ToLower() == "claims")?.LineOfBusinessId,
                                ClientCompanyId = companyUser?.ClientCompanyId,
                                DocumentImage = savedImage,
                            };

                            var pinCode = _context.PinCode.Include(p => p.District).Include(p => p.State).FirstOrDefault(p => p.Code == rowData[19].Trim());

                            var district = _context.District.FirstOrDefault(c => c.DistrictId == pinCode.District.DistrictId);

                            var state = _context.State.FirstOrDefault(s => s.StateId == pinCode.State.StateId);

                            var country = _context.Country.FirstOrDefault();

                            var customerImagePath = Path.Combine(webHostEnvironment.WebRootPath, "upload-case", fileNameWithoutExtension, fileName, rowData[0].Trim(), "CUSTOMER.jpg");

                            var customerImage = File.ReadAllBytes(customerImagePath);

                            dt.Rows[dt.Rows.Count - 1][21] = $"{Convert.ToBase64String(customerImage)}";

                            claim.CustomerDetail = new CustomerDetail
                            {
                                CustomerName = rowData[10]?.Trim(),
                                CustomerType = (CustomerType)Enum.Parse(typeof(CustomerType), rowData[11]?.Trim()),
                                Gender = (Gender)Enum.Parse(typeof(Gender), rowData[12]?.Trim()),
                                CustomerDateOfBirth = DateTime.UtcNow.AddYears(-20),
                                ContactNumber = Convert.ToInt64(rowData[14]?.Trim()),
                                CustomerEducation = (Education)Enum.Parse(typeof(Education), rowData[15]?.Trim()),
                                CustomerOccupation = (Occupation)Enum.Parse(typeof(Occupation), rowData[16]?.Trim()),
                                CustomerIncome = (Income)Enum.Parse(typeof(Income), rowData[17]?.Trim()),
                                Addressline = rowData[18]?.Trim(),
                                CountryId = country.CountryId,
                                PinCodeId = pinCode.PinCodeId,
                                StateId = state.StateId,
                                DistrictId = district.DistrictId,
                                Description = rowData[20]?.Trim(),
                                ProfilePicture = customerImage,
                            };
                            claim.CustomerDetail.PinCode = pinCode;
                            claim.CustomerDetail.PinCode.Latitude = pinCode.Latitude;
                            claim.CustomerDetail.PinCode.Longitude = pinCode.Longitude;

                            var benePinCode = _context.PinCode.Include(p => p.District).Include(p => p.State).FirstOrDefault(p => p.Code == rowData[28].Trim());

                            var beneDistrict = _context.District.FirstOrDefault(c => c.DistrictId == benePinCode.District.DistrictId);

                            var beneState = _context.State.FirstOrDefault(s => s.StateId == benePinCode.State.StateId);
                            var relation = _context.BeneficiaryRelation.FirstOrDefault(b => b.Code.ToLower() == rowData[23].Trim().ToLower());

                            var beneficairyImagePath = Path.Combine(webHostEnvironment.WebRootPath, "upload-case", fileNameWithoutExtension, fileName, rowData[0].Trim(), "BENEFICIARY.jpg");

                            var beneficairyImage = System.IO.File.ReadAllBytes(beneficairyImagePath);
                            dt.Rows[dt.Rows.Count - 1][29] = $"{Convert.ToBase64String(beneficairyImage)}";

                            var beneficairy = new CaseLocation
                            {
                                BeneficiaryName = rowData[22]?.Trim(),
                                BeneficiaryRelationId = relation.BeneficiaryRelationId,
                                BeneficiaryDateOfBirth = DateTime.UtcNow.AddYears(-22),
                                BeneficiaryIncome = (Income)Enum.Parse(typeof(Income), rowData[25]?.Trim()),
                                BeneficiaryContactNumber = Convert.ToInt64(rowData[26]?.Trim()),
                                Addressline = rowData[27]?.Trim(),
                                PinCodeId = benePinCode.PinCodeId,
                                DistrictId = beneDistrict.DistrictId,
                                StateId = beneState.StateId,
                                CountryId = country.CountryId,
                                InvestigationCaseSubStatusId = subStatus.InvestigationCaseSubStatusId,
                                ProfilePicture = beneficairyImage,
                                Updated = DateTime.UtcNow,
                                UpdatedBy = userEmail,
                                Created = DateTime.UtcNow,
                            };

                            var addedClaim = _context.ClaimsInvestigation.Add(claim);

                            beneficairy.ClaimsInvestigationId = addedClaim.Entity.ClaimsInvestigationId;

                            beneficairy.PinCode = benePinCode;
                            beneficairy.PinCode.Latitude = benePinCode.Latitude;
                            beneficairy.PinCode.Longitude = benePinCode.Longitude;

                            _context.CaseLocation.Add(beneficairy);

                            var log = new InvestigationTransaction
                            {
                                ClaimsInvestigationId = addedClaim.Entity.ClaimsInvestigationId,
                                CurrentClaimOwner = userEmail,
                                Created = DateTime.UtcNow,
                                HopCount = 0,
                                Time2Update = 0,
                                InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId,
                                InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId,
                                UpdatedBy = userEmail
                            };
                            _context.InvestigationTransaction.Add(log);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }
            }
            var dataObject = ConvertDataTable<UploadClaim>(dt);
            _context.UploadClaim.AddRange(dataObject);
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }
}