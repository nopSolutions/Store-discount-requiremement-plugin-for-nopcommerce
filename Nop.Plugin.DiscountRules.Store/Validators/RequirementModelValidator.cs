using FluentValidation;
using Nop.Plugin.DiscountRules.Store.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.DiscountRules.Store.Validators
{
    /// <summary>
    /// Represents an <see cref="RequirementModel"/> validator.
    /// </summary>
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.Store.Fields.DiscountId.Required"));
            RuleFor(model => model.StoreId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.Store.Fields.StoreId.Required"));
        }
    }
}
