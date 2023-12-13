using risk.control.system.Data;
using risk.control.system.Models;

namespace risk.control.system.Seeds
{
    public static class ClientCompanySetupSeed
    {
        public static async Task Seed(ApplicationDbContext context)
        {
            #region BENEFICIARY-RELATION

            var brother = new BeneficiaryRelation
            {
                Name = "BROTHER",
                Code = "BROTHER",
            };
            var brotherEntity = await context.AddAsync(brother);

            var father = new BeneficiaryRelation
            {
                Name = "FATHER",
                Code = "FATHER",
            };
            var fatherEntity = await context.AddAsync(father);

            var mother = new BeneficiaryRelation
            {
                Name = "MOTHER",
                Code = "MOTHER",
            };
            var motherEntity = await context.AddAsync(mother);


            var sister = new BeneficiaryRelation
            {
                Name = "SISTER",
                Code = "SISTER",
            };
            var sisterEntity = await context.AddAsync(sister);

            var uncle = new BeneficiaryRelation
            {
                Name = "UNCLE",
                Code = "UNCLE",
            };
            var uncleEntity = await context.AddAsync(uncle);

            var aunty = new BeneficiaryRelation
            {
                Name = "AUNTY",
                Code = "AUNTY",
            };
            var auntyEntity = await context.AddAsync(aunty);

            var newphew = new BeneficiaryRelation
            {
                Name = "NEWPHEW",
                Code = "NEWPHEW",
            };
            var newphewEntity = await context.AddAsync(newphew);

            var niece = new BeneficiaryRelation
            {
                Name = "NIECE",
                Code = "NIECE",
            };
            var nieceEntity = await context.AddAsync(niece);

            var inlaw = new BeneficiaryRelation
            {
                Name = "INLAW",
                Code = "INLAW",
            };
            var inlawEntity = await context.AddAsync(inlaw);

            #endregion

            #region CASE ENABLER

            var doubtCaseEnabler = new CaseEnabler
            {
                Name = "DOUBTFUL BACKGROUND DETAILS",
                Code = "DBD",
            };
            var doubtCaseEnablerEntity = await context.CaseEnabler.AddAsync(doubtCaseEnabler);

            var highAmountCaseEnabler = new CaseEnabler
            {
                Name = "VERY HIGH INSURANCE PREMIUM",
                Code = "VHIP",
            };
            var highAmountCaseEnablerEntity = await context.CaseEnabler.AddAsync(highAmountCaseEnabler);

            #endregion

            #region COST CENTRE

            var loansCostCentre = new CostCentre
            {
                Name = "LOANS",
                Code = "LOANS",
            };

            var loansCostCentreEntity = await context.CostCentre.AddAsync(loansCostCentre);

            var financeCostCentre = new CostCentre
            {
                Name = "FINANCE",
                Code = "FINANCE",
            };

            var financeCostCentreEntity = await context.CostCentre.AddAsync(financeCostCentre);

            #endregion

            #region CASE OUTCOMES

            var postiveOutcome = new InvestigationCaseOutcome
            {
                Name = "SUCCESS",
                Code = "SUCCESS",
            };

            var postiveOutcomeEntity = await context.InvestigationCaseOutcome.AddAsync(postiveOutcome);

            var negativeOutcome = new InvestigationCaseOutcome
            {
                Name = "FAILURE",
                Code = "FAILURE",
            };

            var negativeOutcomeEntity = await context.InvestigationCaseOutcome.AddAsync(negativeOutcome);

            var unknownOutcome = new InvestigationCaseOutcome
            {
                Name = "UNKNOWN",
                Code = "UNKNOWN",
            };

            var unknownOutcomeEntity = await context.InvestigationCaseOutcome.AddAsync(unknownOutcome);


            #endregion

        }
    }
}
