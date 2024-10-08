﻿using DFApp.Bookkeeping.Expenditure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace DFApp.Web.Pages.Bookkeeping.Expenditure
{
    public class CreateModalModel : DFAppPageModel
    {
        [BindProperty]
        public CreateExpenditureViewModel? Expenditures { get; set; }

        public List<SelectListItem>? Categorys { get; set; }

        private readonly IBookkeepingExpenditureService _bookkeepingExpenditureService;

        public CreateModalModel(IBookkeepingExpenditureService bookkeepingExpenditureService)
        {
            _bookkeepingExpenditureService = bookkeepingExpenditureService;
        }

        public async Task OnGetAsync()
        {
            Expenditures = new CreateExpenditureViewModel();
            
            Categorys = (await _bookkeepingExpenditureService.GetCategoryLookupDto())
                .Select(x => new SelectListItem(x.Category, x.CategoryId.ToString()))
                .ToList();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            await _bookkeepingExpenditureService.CreateAsync(
                ObjectMapper.Map<CreateExpenditureViewModel, CreateUpdateBookkeepingExpenditureDto>(Expenditures)
                );
            return NoContent();
        }


        public class CreateExpenditureViewModel
        {

            public CreateExpenditureViewModel()
            {
                ExpenditureDate = DateTime.Today; // 设置默认日期为今天
                IsBelongToSelf = true;
            }

            [Required]
            [DataType(DataType.Date)]
            [DisplayName("日期")]
            public DateTime ExpenditureDate { get; set; }

            [Required]
            [DisplayName("支出金额")]
            public decimal Expenditure { get; set; }

            [Required]
            [SelectItems(nameof(Categorys))]
            [DisplayName("类别")]
            public long CategoryId { get; set; }

            [DisplayName("备注")]
            public string? Remark { get; set; }

            [Required]
            [DisplayName("自用")]
            public bool IsBelongToSelf { get; set; }

        }

    }
}
