using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BookStroreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHost)
        {
            _unitOfWork = unitOfWork;
			_WebHostEnvironment = webHost;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category").ToList();
            
            return View(objProductList);
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
			IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(i => new SelectListItem
			{
				Text = i.Name,
				Value = i.Id.ToString()
			});
			ProductVM productVM = new ProductVM()
			{
				Product = new Product(),
				CategoryList = CategoryList
			};
            if(id == null || id == 0)
				return View(productVM);
			else
            {
                // Update Functionality
                productVM.Product = _unitOfWork.ProductRepository.Get(u => u.Id == id);
			    return View(productVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _WebHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + "-"+ file.FileName;
                    string productPath = Path.Combine(wwwRootPath, @"images\product",fileName);
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(productPath, FileMode.Create))
                    {
                   
                        file.CopyTo(fileStream);
                   
                    }
                    
                    productVM.Product.ImageUrl = @"\images\product\" + fileName; 
                }
                if (productVM.Product.Id != 0)
                {
					_unitOfWork.ProductRepository.Update(productVM.Product);
                }
                else
                    // Create Product
                    _unitOfWork.ProductRepository.Add(productVM.Product);
				
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(i => new SelectListItem
			{
				Text = i.Name,
				Value = i.Id.ToString()
			});
			productVM.CategoryList = CategoryList;
            return View(productVM);
        }
       
        #region API Calls
            
        [HttpGet]
        public IActionResult GetAll()
        {
            var objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category");
            return Json(new { data = objProductList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.ProductRepository.Get(u => u.Id == id);
            if(productToBeDeleted == null)
                return Json(new { success = false, message = "Error while deleting" });
            var oldImagePath = Path.Combine(_WebHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if(System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.ProductRepository.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new {Success = true, Message = "Delete Successful"});
        }

        #endregion
    }
}
