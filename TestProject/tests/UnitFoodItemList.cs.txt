using calcalc.Controllers;
using calcalc.ViewModels;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MvcMovie.Data;

namespace tests;

public class UnitFoodItemList
{
    [Fact]
    public async Task TestFoodItemsSeachNoResults()
    {
        var controller = new FoodItemController(MockData.getFakeDbContext());
        var result = await controller.Index("ThisShuldNotGetHits", null, null, null) as ViewResult;
        var viewmodel = result.Model as FoodItemIndexViewmodel;
        
        Assert.Empty(viewmodel.FoodItems);
    }

    [Fact]
    public async Task TestFoodItemsSeachPagination()
    {
        var controller = new FoodItemController(MockData.getFakeDbContext());
        
        // total 29 items should match search, that is 2 full pages pages
        
        // first page of pagination
        var resultPage1 = await controller.Index("Melk", null, null, null) as ViewResult;
        var viewModelPage1 = resultPage1.Model as FoodItemIndexViewmodel;
        Assert.Equivalent(viewModelPage1.FoodItems.Count, 10);
        Assert.Equivalent(viewModelPage1.HasNextPage, true);
        Assert.Equivalent(viewModelPage1.HasPreviousPage, false);
        
        // second page of pagination
        var resultPage2 = await controller.Index("Melk", "Melk k", null, null) as ViewResult;
        var viewModelPage2 = resultPage2.Model as FoodItemIndexViewmodel;
        Assert.Equivalent(viewModelPage2.FoodItems.Count, 10);
        Assert.Equivalent(viewModelPage2.HasNextPage, true);
        Assert.Equivalent(viewModelPage2.HasPreviousPage, true);
        
        // third (last) page of pagination
        var resultPage3 = await controller.Index("Melk", "Melk u", "Melk k", null) as ViewResult;
        var viewModelPage3 = resultPage3.Model as FoodItemIndexViewmodel;
        Assert.Equivalent(viewModelPage3.FoodItems.Count, 9);
        Assert.Equivalent(viewModelPage3.HasNextPage, false);
        Assert.Equivalent(viewModelPage3.HasPreviousPage, true);
    }
    
    [Fact]
    public async Task TestFoodItemsSearch()
    {
        var controller = new FoodItemController(MockData.getFakeDbContext());
        var result = await controller.Index("Hvetemel", null, null, null) as ViewResult;
        var viewmodel = Assert.IsType<FoodItemIndexViewmodel>(result.Model);
        Assert.Single(viewmodel.FoodItems);
    }
    
}
