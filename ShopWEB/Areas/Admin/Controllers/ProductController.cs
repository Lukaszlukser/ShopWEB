using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using System.Diagnostics;

namespace ShopWEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IUnitOfWork _unitofWork;
        private IWebHostEnvironment _hostingEnvironment;

        public ProductController(IUnitOfWork unitofWork, 
            IWebHostEnvironment hostingEnvironment)
        {
            _unitofWork = unitofWork;
            _hostingEnvironment = hostingEnvironment;
        }
        #region APICALL
        public IActionResult AllProducts()
        {
            var products = _unitofWork.Product.GetAll(includeProperties:
                "Category");
            return Json(new { data = products });
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CreateUpdate(int? id)
        {
            ProductVM vm = new ProductVM()
            {
                Product = new(),
                Categories = _unitofWork.Category.GetAll().Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString() 
                })

            };
            if (id==null || id==0)
            {
                return View(vm);
            }
            else
            {
                vm.Product = _unitofWork.Product.GetT(x => x.Id == id);
                if (vm.Product == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(vm);
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUpdate(ProductVM vm,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string fileName = string.Empty;
                if (file != null) 
                {
                    string uploadDir = Path.Combine(_hostingEnvironment.WebRootPath,
                        "ProductImage");
                    fileName = Guid.NewGuid().ToString() + "-" + file.FileName;
                    string filePath = Path.Combine(uploadDir, fileName);

                    if (vm.Product.ImgUrl!=null)
                    {
                        var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, 
                            vm.Product.ImgUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    vm.Product.ImgUrl = @"\ProductImage\" + fileName;
                }
                if (vm.Product.Id==0)
                {
                    _unitofWork.Product.Add(vm.Product);
                    TempData["success"] = "Product Create Done!";
                }
                else
                {
                    _unitofWork.Product.Update(vm.Product);
                    TempData["success"] = "Product Update DOne!";
                }

                _unitofWork.Save();
                return RedirectToAction("Index");

            }
            return RedirectToAction("Index");
        }
        #region DeleteAPICALL
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = _unitofWork.Product.GetT(x => x.Id == id);
            if (product==null)
            {
                return Json(new { success = false, message = "Error in fetching data" });
            }
            else
            {
                var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, product.ImgUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                _unitofWork.Product.Delete(product);
                _unitofWork.Save();
                return Json(new { success = true, message = "Product Delete" });
            }
        }
        #endregion
    }
}
