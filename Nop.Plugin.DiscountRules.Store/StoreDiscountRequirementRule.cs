using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;

namespace Nop.Plugin.DiscountRules.Store
{
    public partial class StoreDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        #endregion

        #region Ctor

        public StoreDiscountRequirementRule(ISettingService settingService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory)
        {
            this._settingService = settingService;
            this._actionContextAccessor = actionContextAccessor;
            this._urlHelperFactory = urlHelperFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var storeId = _settingService.GetSettingByKey<int>($"DiscountRequirement.Store-{request.DiscountRequirementId}");

            if (storeId == 0)
                return result;

            result.IsValid = request.Store.Id == storeId;

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            return urlHelper.Action("Configure", "DiscountRulesStore",
                new { discountId = discountId, discountRequirementId = discountRequirementId }).TrimStart('/');
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Store.Fields.SelectStore", "Select store");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store", "Store");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store.Hint", "Select the store in which this discount will be valid.");
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Store.Fields.SelectStore");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store.Hint");
            base.Uninstall();
        }

        #endregion
    }
}