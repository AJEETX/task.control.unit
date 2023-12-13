using Microsoft.AspNetCore.Identity;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Services;

namespace risk.control.system.Seeds
{
    public static class DatabaseSeed
    {
        public static async Task SeedDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var httpClientService = scope.ServiceProvider.GetRequiredService<IHttpClientService>();
            var vendorUserManager = scope.ServiceProvider.GetRequiredService<UserManager<VendorApplicationUser>>();
            var clientUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ClientCompanyApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            //check for users
            if (context.ApplicationUser.Any())
            {
                return; //if user is not empty, DB has been seed
            }

            //CREATE ROLES
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.PortalAdmin.ToString().Substring(0, 2).ToUpper(), AppRoles.PortalAdmin.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.CompanyAdmin.ToString().Substring(0, 2).ToUpper(), AppRoles.CompanyAdmin.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.AgencyAdmin.ToString().Substring(0, 2).ToUpper(), AppRoles.AgencyAdmin.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.Creator.ToString().Substring(0, 2).ToUpper(), AppRoles.Creator.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.Assigner.ToString().Substring(0, 2).ToUpper(), AppRoles.Assigner.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.Assessor.ToString().Substring(0, 2).ToUpper(), AppRoles.Assessor.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.Supervisor.ToString().Substring(0, 2).ToUpper(), AppRoles.Supervisor.ToString()));
            await roleManager.CreateAsync(new ApplicationRole(AppRoles.Agent.ToString().Substring(0, 2).ToUpper(), AppRoles.Agent.ToString()));

            var india = new Country
            {
                Name = "AUSTRALIA",
                Code = "AU",
            };
            var indiaCountry = await context.Country.AddAsync(india);

            await PinCodeStateSeed.SeedPincode(context, indiaCountry.Entity);

            await context.SaveChangesAsync(null, false);

            #region LINE OF BUSINESS

            var claims = new LineOfBusiness
            {
                Name = "CLAIMS",
                Code = "CLAIMS",
                MasterData = true,
            };

            var claimCaseType = await context.LineOfBusiness.AddAsync(claims);

            var underwriting = new LineOfBusiness
            {
                Name = "UNDERWRITING",
                Code = "UNDERWRITING",
                MasterData = true,
            };

            var underwritingCaseType = await context.LineOfBusiness.AddAsync(underwriting);

            #endregion LINE OF BUSINESS

            #region INVESTIGATION SERVICE TYPES

            var claimComprehensive = new InvestigationServiceType
            {
                Name = "COMPREHENSIVE",
                Code = "COMP",
                MasterData = true,
                LineOfBusinessId = claimCaseType.Entity.LineOfBusinessId
            };
            var claimComprehensiveService = await context.InvestigationServiceType.AddAsync(claimComprehensive);

            var claimNonComprehensive = new InvestigationServiceType
            {
                Name = "NON-COMPREHENSIVE",
                Code = "NON-COMP",
                MasterData = true,
                LineOfBusinessId = claimCaseType.Entity.LineOfBusinessId
            };

            var claimNonComprehensiveService = await context.InvestigationServiceType.AddAsync(claimNonComprehensive);

            var claimDocumentCollection = new InvestigationServiceType
            {
                Name = "DOCUMENT-COLLECTION",
                Code = "DOC",
                MasterData = true,
                LineOfBusinessId = claimCaseType.Entity.LineOfBusinessId
            };

            var claimDocumentCollectionService = await context.InvestigationServiceType.AddAsync(claimDocumentCollection);

            var claimDiscreet = new InvestigationServiceType
            {
                Name = "DISCREET",
                Code = "DISCREET",
                MasterData = true,
                LineOfBusinessId = claimCaseType.Entity.LineOfBusinessId
            };

            var claimDiscreetService = await context.InvestigationServiceType.AddAsync(claimDiscreet);

            var underWritingPreVerification = new InvestigationServiceType
            {
                Name = "PRE-ONBOARDING-VERIFICATION",
                Code = "PRE-OV",
                MasterData = true,
                LineOfBusinessId = underwritingCaseType.Entity.LineOfBusinessId
            };

            var underWritingPreVerificationService = await context.InvestigationServiceType.AddAsync(underWritingPreVerification);

            var underWritingPostVerification = new InvestigationServiceType
            {
                Name = "POST-ONBOARDING-VERIFICATION",
                Code = "POST-OV",
                MasterData = true,
                LineOfBusinessId = underwritingCaseType.Entity.LineOfBusinessId
            };

            var underWritingPostVerificationService = await context.InvestigationServiceType.AddAsync(underWritingPostVerification);

            #endregion INVESTIGATION SERVICE TYPES

            #region //CREATE RISK CASE DETAILS

            //CASE STATUS

            var initiated = new InvestigationCaseStatus
            {
                Name = CONSTANTS.CASE_STATUS.INITIATED,
                Code = CONSTANTS.CASE_STATUS.INITIATED,
                MasterData = true,
            };

            var initiatedStatus = await context.InvestigationCaseStatus.AddAsync(initiated);

            var inProgress = new InvestigationCaseStatus
            {
                Name = CONSTANTS.CASE_STATUS.INPROGRESS,
                Code = CONSTANTS.CASE_STATUS.INPROGRESS,
                MasterData = true,
            };

            var inProgressStatus = await context.InvestigationCaseStatus.AddAsync(inProgress);

            var finished = new InvestigationCaseStatus
            {
                Name = CONSTANTS.CASE_STATUS.FINISHED,
                Code = CONSTANTS.CASE_STATUS.FINISHED,
                MasterData = true,
            };

            var finishedStatus = await context.InvestigationCaseStatus.AddAsync(finished);

            //CASE SUBSTATUS

            var created = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR,
                MasterData = true,
                InvestigationCaseStatusId = initiatedStatus.Entity.InvestigationCaseStatusId
            };
            var createdSubStatus = await context.InvestigationCaseSubStatus.AddAsync(created);

            var edited = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.EDITED_BY_CREATOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.EDITED_BY_CREATOR,
                MasterData = true,
                InvestigationCaseStatusId = initiatedStatus.Entity.InvestigationCaseStatusId
            };
            var editedSubStatus = await context.InvestigationCaseSubStatus.AddAsync(edited);

            var assigned = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER,
                MasterData = true,
                InvestigationCaseStatusId = inProgressStatus.Entity.InvestigationCaseStatusId
            };

            var assignedSubStatus = await context.InvestigationCaseSubStatus.AddAsync(assigned);

            var allocated = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR,
                MasterData = true,
                InvestigationCaseStatusId = inProgressStatus.Entity.InvestigationCaseStatusId
            };

            var allocatedSubStatus = await context.InvestigationCaseSubStatus.AddAsync(allocated);

            var assignedToAgent = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT,
                MasterData = true,
                InvestigationCaseStatusId = inProgressStatus.Entity.InvestigationCaseStatusId
            };

            var assignedToAgentSubStatus = await context.InvestigationCaseSubStatus.AddAsync(assignedToAgent);

            var submittedtoSupervisor = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR,
                MasterData = true,
                InvestigationCaseStatusId = inProgressStatus.Entity.InvestigationCaseStatusId
            };

            var submittedtoSupervisorSubStatus = await context.InvestigationCaseSubStatus.AddAsync(submittedtoSupervisor);

            var submittedToAssessor = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR,
                MasterData = true,
                InvestigationCaseStatusId = inProgressStatus.Entity.InvestigationCaseStatusId
            };

            var submittedToAssessorSubStatus = await context.InvestigationCaseSubStatus.AddAsync(submittedToAssessor);

            var approved = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR,
                MasterData = true,
                InvestigationCaseStatusId = finishedStatus.Entity.InvestigationCaseStatusId
            };

            var approvededSubStatus = await context.InvestigationCaseSubStatus.AddAsync(approved);

            var rejected = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REJECTED_BY_ASSESSOR,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REJECTED_BY_ASSESSOR,
                MasterData = true,
                InvestigationCaseStatusId = finishedStatus.Entity.InvestigationCaseStatusId
            };

            var rejectedSubStatus = await context.InvestigationCaseSubStatus.AddAsync(rejected);

            var reassigned = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER,
                MasterData = true,
                InvestigationCaseStatusId = finishedStatus.Entity.InvestigationCaseStatusId
            };
            var acceptedSubStatus = await context.InvestigationCaseSubStatus.AddAsync(reassigned);

            var withdrawnByCompany = new InvestigationCaseSubStatus
            {
                Name = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.WITHDRAWN_BY_COMPANY,
                Code = CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.WITHDRAWN_BY_COMPANY,
                MasterData = true,
                InvestigationCaseStatusId = finishedStatus.Entity.InvestigationCaseStatusId
            };

            var withdrawnByCompanySubStatus = await context.InvestigationCaseSubStatus.AddAsync(withdrawnByCompany);

            #endregion //CREATE RISK CASE DETAILS

            #region BENEFICIARY-RELATION

            await ClientCompanySetupSeed.Seed(context);

            #endregion BENEFICIARY-RELATION

            #region CLIENT/ VENDOR COMPANY

            var (checker, verify, investigate, canaraId, hdfcId) = await ClientVendorSeed.Seed(context, indiaCountry,
                claimComprehensiveService.Entity, claimDiscreetService.Entity,
                claimDocumentCollectionService.Entity, claimCaseType.Entity, httpClientService);

            #endregion CLIENT/ VENDOR COMPANY

            #region APPLICATION USERS ROLES

            await PortalAdminSeed.Seed(context, webHostEnvironment, indiaCountry, userManager, roleManager);

            await ClientApplicationUserSeed.Seed(context, webHostEnvironment, indiaCountry, clientUserManager, canaraId);

            await ClientApplicationUserSeed.Seed(context, webHostEnvironment, indiaCountry, clientUserManager, hdfcId);

            await VendorApplicationUserSeed.Seed(context, webHostEnvironment, indiaCountry, vendorUserManager, checker);

            await VendorApplicationUserSeed.Seed(context, webHostEnvironment, indiaCountry, vendorUserManager, verify);

            await VendorApplicationUserSeed.Seed(context, webHostEnvironment, indiaCountry, vendorUserManager, investigate);

            await context.SaveChangesAsync(null, false);

            #endregion APPLICATION USERS ROLES
        }
    }
}