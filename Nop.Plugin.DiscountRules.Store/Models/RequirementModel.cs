using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.Store.Models
{
    public class RequirementModel
    {
        public RequirementModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.DiscountRules.Store.Fields.Store")]
        public int StoreId { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}