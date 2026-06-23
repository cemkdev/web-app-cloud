using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Storage;
using WebAppAPI.Application.DTOs.Product;
using WebAppAPI.Application.Options.Storage;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;
using FileEntity = WebAppAPI.Domain.Entities.File;

namespace WebAppAPI.Persistence.Services
{
    public class ProductService(
        IProductReadRepository productReadRepository,
        IWriteRepository<Product> productWriteRepository,
        IProductImageFileReadRepository productImageFileReadRepository,
        IWriteRepository<ProductImageFile> productImageFileWriteRepository,
        IStorageService storageService,
        IWriteRepository<FileEntity> fileWriteRepository,
        IWebHostEnvironment webHostEnvironment,
        IOptions<BaseStorageOptions> baseStorageOptions,
        IOptions<StorageOptions> storageOptions,
        IQRCodeService qrCodeService,
        IUnitOfWork unitOfWork) : IProductService
    {
        private readonly IProductReadRepository _productReadRepository = productReadRepository;
        private readonly IWriteRepository<Product> _productWriteRepository = productWriteRepository;
        private readonly IProductImageFileReadRepository _productImageFileReadRepository = productImageFileReadRepository;
        private readonly IWriteRepository<ProductImageFile> _productImageFileWriteRepository = productImageFileWriteRepository;
        private readonly IStorageService _storageService = storageService;
        private readonly IWriteRepository<FileEntity> _fileWriteRepository = fileWriteRepository;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly BaseStorageOptions _baseStorageOptions = baseStorageOptions.Value;
        private readonly StorageOptions _storageOptions = storageOptions.Value;
        private readonly IQRCodeService _qrCodeService = qrCodeService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private const string LocalStorageName = "LocalStorage";

        public async Task CreateProductAsync(CreateProductDto product)
        {
            ArgumentNullException.ThrowIfNull(product);

            await _productWriteRepository.AddAsync(new()
            {
                Name = product.Name,
                Stock = product.Stock,
                Price = product.Price,
                Title = product.Title,
                Description = product.Description
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(UpdateProductDto product)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (string.IsNullOrWhiteSpace(product.Id))
                throw new Exception("Product id is required.");

            if (!Guid.TryParse(product.Id, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            Product? existingProduct = await _productReadRepository.GetByIdAsync(productGuid, tracking: true);

            if (existingProduct == null)
                throw new Exception("Product not found.");

            existingProduct.Name = product.Name ?? existingProduct.Name;
            existingProduct.Stock = product.Stock ?? existingProduct.Stock;
            existingProduct.Price = product.Price ?? existingProduct.Price;
            existingProduct.Title = product.Title ?? existingProduct.Title;
            existingProduct.Description = product.Description ?? existingProduct.Description;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveProductAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("Product id is required.");

            if (!Guid.TryParse(id, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            Product? product = await _productReadRepository.GetByIdWithImagesAsync(productGuid, tracking: true);

            if (product == null)
                throw new Exception("Product not found.");

            List<ProductImageFile> productImageFiles = product.ProductImageFiles.ToList();
            List<string> localImagePaths = GetLocalImagePathsForDeletion(productImageFiles);

            if (productImageFiles.Count > 0)
                _fileWriteRepository.RemoveRange(productImageFiles.Cast<FileEntity>().ToList());

            _productWriteRepository.Remove(product);

            await _unitOfWork.SaveChangesAsync();

            DeleteLocalPhysicalFiles(localImagePaths);
        }

        public async Task RemoveRangeProductAsync(IEnumerable<string> productIds)
        {
            ArgumentNullException.ThrowIfNull(productIds);

            HashSet<Guid> productGuids = [];

            foreach (string productId in productIds)
            {
                if (string.IsNullOrWhiteSpace(productId))
                    throw new Exception("Product id is required.");

                if (!Guid.TryParse(productId, out Guid productGuid))
                    throw new Exception("Product id is not valid.");

                productGuids.Add(productGuid);
            }

            if (productGuids.Count == 0)
                throw new Exception("At least one product must be selected for deletion.");

            List<Product> products = await _productReadRepository.GetByIdsWithImagesAsync(productGuids, tracking: true);

            if (products.Count != productGuids.Count)
                throw new Exception("One or more products were not found.");

            List<ProductImageFile> productImageFiles = products
                .SelectMany(p => p.ProductImageFiles)
                .DistinctBy(p => p.Id)
                .ToList();

            List<string> localImagePaths = GetLocalImagePathsForDeletion(productImageFiles);

            if (productImageFiles.Count > 0)
                _fileWriteRepository.RemoveRange(productImageFiles.Cast<FileEntity>().ToList());

            _productWriteRepository.RemoveRange(products);

            await _unitOfWork.SaveChangesAsync();

            DeleteLocalPhysicalFiles(localImagePaths);
        }

        public async Task UploadProductImagesAsync(string productId, IFormFileCollection? files)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new Exception("Product id is required.");

            if (files == null || files.Count == 0)
                throw new Exception("At least one product image file is required.");

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            Product? product = await _productReadRepository.GetByIdAsync(productGuid, tracking: true);

            if (product == null)
                throw new Exception("Product not found.");

            List<(string fileName, string pathOrContainerName)> uploadedFiles = await _storageService.UploadAsync("images", files);

            if (uploadedFiles.Count == 0)
                throw new Exception("Product image upload failed: no files were uploaded.");

            await _productImageFileWriteRepository.AddRangeAsync(uploadedFiles.Select(file => new ProductImageFile
            {
                FileName = file.fileName,
                Path = file.pathOrContainerName,
                Storage = _storageService.StorageName,
                Product = new List<Product> { product }
            }).ToList());

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangeCoverImageAsync(string productId, string imageId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new Exception("Product id is required.");

            if (string.IsNullOrWhiteSpace(imageId))
                throw new Exception("Product image id is required.");

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            if (!Guid.TryParse(imageId, out Guid imageGuid))
                throw new Exception("Product image id is not valid.");

            ProductImageFile? currentCoverImage = await _productImageFileReadRepository.GetCurrentCoverImageAsync(productGuid, tracking: true);

            if (currentCoverImage != null)
                currentCoverImage.CoverImage = false;

            ProductImageFile? selectedImage = await _productImageFileReadRepository.GetByProductIdAndImageIdAsync(productGuid, imageGuid, tracking: true);

            if (selectedImage == null)
                throw new Exception("Selected product image not found.");

            selectedImage.CoverImage = true;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveProductImageAsync(string productId, string imageId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new Exception("Product id is required.");

            if (string.IsNullOrWhiteSpace(imageId))
                throw new Exception("Product image id is required.");

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            if (!Guid.TryParse(imageId, out Guid imageGuid))
                throw new Exception("Product image id is not valid.");

            Product? product = await _productReadRepository.GetByIdWithImagesAsync(productGuid, tracking: true);

            if (product == null)
                throw new Exception("Product not found.");

            ProductImageFile? productImageFile = product.ProductImageFiles
                .FirstOrDefault(p => p.Id == imageGuid);

            if (productImageFile == null)
                throw new Exception("Product image not found.");

            List<string> localImagePaths = GetLocalImagePathsForDeletion(new[] { productImageFile });

            _fileWriteRepository.Remove(productImageFile);

            await _unitOfWork.SaveChangesAsync();

            DeleteLocalPhysicalFiles(localImagePaths);
        }

        public async Task<GetAllProductsDto> GetAllProductsAsync(int page, int size)
        {
            if (page < 0)
                throw new Exception("Page cannot be less than zero.");

            if (size <= 0)
                throw new Exception("Size must be greater than zero.");

            (List<Product> products, int totalCount) = await _productReadRepository.GetPagedWithImagesAsync(page, size);

            return new()
            {
                Products = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Stock,
                    p.Price,
                    p.DateCreated,
                    p.DateUpdated,
                    p.Title,
                    p.Description,
                    p.Rating,
                    ProductImageFiles = p.ProductImageFiles.Select(pif => new
                    {
                        pif.Id,
                        pif.Path,
                        pif.FileName,
                        pif.CoverImage
                    }).ToList()
                }).ToList(),
                TotalProductCount = totalCount
            };
        }

        public async Task<GetByIdProductDto> GetProductByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("Product id is required.");

            if (!Guid.TryParse(id, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            Product? product = await _productReadRepository.GetByIdAsync(productGuid, tracking: false);

            if (product == null)
                throw new Exception("Product not found.");

            return new()
            {
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                Title = product.Title,
                Description = product.Description,
                Rating = product.Rating
            };
        }

        public async Task<List<GetProductImagesDto>> GetProductImagesAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new Exception("Product id is required.");

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            Product? product = await _productReadRepository.GetByIdWithImagesAsync(productGuid, tracking: false);

            if (product == null)
                throw new Exception("Product not found.");

            string storageName = $"{_storageOptions.Provider}Storage";

            return product.ProductImageFiles
                .Where(i => i.Storage == storageName)
                .Select(p => new GetProductImagesDto
                {
                    Id = p.Id,
                    Path = $"{_baseStorageOptions.Url}/{p.Path}",
                    FileName = p.FileName,
                    CoverImage = p.CoverImage
                })
                .ToList();
        }

        public async Task<byte[]> QrCodeFromProductAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new Exception("Product id is required.");

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new Exception("Product id is not valid.");

            Product? product = await _productReadRepository.GetByIdAsync(productGuid, tracking: false);

            if (product == null)
                throw new Exception("Product not found.");

            var plainObject = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.DateCreated
            };
            string plainText = JsonSerializer.Serialize(plainObject);

            return _qrCodeService.GenerateQRCode(plainText);
        }

        #region Helpers
        private List<string> GetLocalImagePathsForDeletion(IEnumerable<ProductImageFile> productImageFiles)
        {
            return productImageFiles
                .Where(file => file.Storage == LocalStorageName)
                .Select(file => file.Path)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct()
                .ToList();
        }

        private void DeleteLocalPhysicalFiles(IEnumerable<string> relativePaths)
        {
            string? webRoot = _webHostEnvironment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
                return;

            foreach (string relativePath in relativePaths)
            {
                string fullPath = Path.Combine(webRoot, relativePath);

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
        }
        #endregion
    }
}
