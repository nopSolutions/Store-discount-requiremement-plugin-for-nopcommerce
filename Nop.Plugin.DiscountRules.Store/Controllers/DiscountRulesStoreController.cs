using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.Store.Models;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.DiscountRules.Store.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class DiscountRulesStoreController : BasePluginController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public DiscountRulesStoreController(IDiscountService discountService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreService storeService)
        {
            _discountService = discountService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;

            if (discountRequirementId.HasValue)
            {
                discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var storeId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirementId ?? 0));

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                StoreId = storeId
            };

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Plugins.DiscountRules.Store.Fields.SelectStore"), Value = "0" });

            foreach (var s in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = discountRequirement != null && s.Id == storeId });

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HTML_FIELD_PREFIX, discountRequirementId ?? 0);

            return View("~/Plugins/DiscountRules.Store/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(RequirementModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            if (ModelState.IsValid)
            {
                //load the discount
                var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
                if (discount == null)
                    return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

                //get the discount requirement
                var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

                //the discount requirement does not exist, so create a new one
                if (discountRequirement == null)
                {
                    discountRequirement = new DiscountRequirement
                    {
                        DiscountId = discount.Id,
                        DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SYSTEM_NAME
                    };

                    await _discountService.InsertDiscountRequirementAsync(discountRequirement);
                }

                await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirement.Id), model.StoreId);

                return Ok(new { NewRequirementId = discountRequirement.Id });
            }

            return BadRequest(new { Errors = GetErrorsFromModelState() });
        }

        #endregion

        #region Utilities

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
        }

        #endregion
    }
}