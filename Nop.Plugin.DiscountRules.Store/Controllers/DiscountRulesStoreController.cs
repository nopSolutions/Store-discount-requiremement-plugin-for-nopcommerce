using System;
using System.Linq;
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

        public IActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);

            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;

            if (discountRequirementId.HasValue)
            {
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var storeId = _settingService.GetSettingByKey<int>($"DiscountRequirement.Store-{discountRequirementId ?? 0}");

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                StoreId = storeId
            };

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Plugins.DiscountRules.Store.Fields.SelectStore"), Value = "0" });

            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = discountRequirement != null && s.Id == storeId });

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesStore{discountRequirementId?.ToString() ?? "0"}";

            return View("~/Plugins/DiscountRules.Store/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(int discountId, int? discountRequirementId, int storeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);

            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;

            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

            if (discountRequirement != null)
            {
                //update existing rule
                _settingService.SetSetting($"DiscountRequirement.Store-{discountRequirement.Id}", storeId);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.Store"
                };

                discount.DiscountRequirements.Add(discountRequirement);

                _discountService.UpdateDiscount(discount);

                _settingService.SetSetting($"DiscountRequirement.Store-{discountRequirement.Id}", storeId);
            }

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        #endregion
    }
}