using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.DiscountRules.Store.Models
{
    public record RequirementModel
    {
        public RequirementModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableStores { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        [NopResourceDisplayName("Plugins.DiscountRules.Store.Fields.Store")]
        public int StoreId { get; set; }
    }
}