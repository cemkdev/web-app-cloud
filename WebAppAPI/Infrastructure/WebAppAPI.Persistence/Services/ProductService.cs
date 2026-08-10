using Microsoft.Extensions.Options;
using System.Text.Json;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Storage;
using WebAppAPI.Application.Abstractions.Storage.Models;
using WebAppAPI.Application.Features.Products.Commands.CreateProduct.DTOs;
using WebAppAPI.Application.Features.Products.Commands.UpdateProduct.DTOs;
using WebAppAPI.Application.Features.Products.Commands.UploadProductImage.Policies;
using WebAppAPI.Application.Features.Products.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductById.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductDetail.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct.DTOs;
using WebAppAPI.Application.Options.Storage;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Services
{
    public sealed class ProductService(
        IProductReadRepository productReadRepository,
        IWriteRepository<Product> productWriteRepository,
        IProductImageFileReadRepository productImageFileReadRepository,
        IWriteRepository<ProductImageFile> productImageFileWriteRepository,
        IStorageService storageService,
        IOptions<BaseStorageOptions> baseStorageOptions,
        IQRCodeService qrCodeService,
        IUnitOfWork unitOfWork) : IProductService
    {
        private readonly IProductReadRepository _productReadRepository = productReadRepository;
        private readonly IWriteRepository<Product> _productWriteRepository = productWriteRepository;
        private readonly IProductImageFileReadRepository _productImageFileReadRepository = productImageFileReadRepository;
        private readonly IWriteRepository<ProductImageFile> _productImageFileWriteRepository = productImageFileWriteRepository;
        private readonly IStorageService _storageService = storageService;
        private readonly BaseStorageOptions _baseStorageOptions = baseStorageOptions.Value;
        private readonly IQRCodeService _qrCodeService = qrCodeService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        private const string ProductImagesStorageLocation = "product-images";

        public Task<GetAllProductsDto> GetAllProductsAsync(int page, int size, CancellationToken cancellationToken)
        {
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), page, "Page cannot be less than zero.");

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be greater than zero.");

            return _productReadRepository.GetPagedAsync(page, size, cancellationToken);
        }

        public async Task<ProductByIdDto> GetProductByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Product id is required.", nameof(id));

            if (!Guid.TryParse(id, out Guid productId))
                throw new ArgumentException("Product id is not valid.", nameof(id));

            ProductByIdDto? product = await _productReadRepository.GetProductByIdAsync(productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            return product;
        }

        public async Task<ProductDetailDto> GetProductDetailAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Product id is required.", nameof(id));

            if (!Guid.TryParse(id, out Guid productId))
                throw new ArgumentException("Product id is not valid.", nameof(id));

            ProductDetailDto? product = await _productReadRepository.GetProductDetailAsync(productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            return product;
        }

        public async Task<IReadOnlyList<ProductImageDto>> GetProductImagesAsync(string productId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product id is required.", nameof(productId));

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new ArgumentException("Product id is not valid.", nameof(productId));

            bool productExists = await _productReadRepository.ExistsAsync(productGuid, cancellationToken);

            if (!productExists)
                throw new KeyNotFoundException("Product not found.");

            List<ProductImageFile> images = await _productImageFileReadRepository.GetByProductIdAndStorageAsync(
                    productGuid,
                    _storageService.Provider.ToString(),
                    cancellationToken);

            string baseStorageUrl = _baseStorageOptions.Url.TrimEnd('/');

            return images
                .Select(image => new ProductImageDto
                {
                    Id = image.Id,
                    Path = $"{baseStorageUrl}/{image.Path.TrimStart('/')}",
                    FileName = image.FileName,
                    CoverImage = image.CoverImage
                })
                .ToList();
        }

        public async Task<byte[]> QrCodeFromProductAsync(string productId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product id is required.", nameof(productId));

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new ArgumentException("Product id is not valid.", nameof(productId));

            ProductQrCodeDto? product = await _productReadRepository.GetQrCodeDataAsync(productGuid, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            string content = JsonSerializer.Serialize(product);

            return _qrCodeService.Generate(content);
        }

        public async Task<Guid> CreateProductAsync(CreateProductDto product, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(product);

            Product createdProduct = new()
            {
                Name = product.Name,
                Stock = product.Stock,
                Price = product.Price,
                Title = product.Title,
                Description = product.Description
            };

            await _productWriteRepository.AddAsync(createdProduct);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return createdProduct.Id;
        }

        public async Task UpdateProductAsync(UpdateProductDto product, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (string.IsNullOrWhiteSpace(product.Id))
                throw new ArgumentException("Product id is required.", nameof(product));

            if (!Guid.TryParse(product.Id, out Guid productId))
                throw new ArgumentException("Product id is not valid.", nameof(product));

            Product? existingProduct = await _productReadRepository.GetByIdForUpdateAsync(productId, cancellationToken);

            if (existingProduct is null)
                throw new KeyNotFoundException("Product not found.");

            existingProduct.Name = product.Name ?? existingProduct.Name;
            existingProduct.Stock = product.Stock ?? existingProduct.Stock;
            existingProduct.Price = product.Price ?? existingProduct.Price;
            existingProduct.Title = product.Title ?? existingProduct.Title;
            existingProduct.Description = product.Description ?? existingProduct.Description;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UploadProductImagesAsync(string productId, IReadOnlyCollection<StorageUploadFile> files, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product id is required.", nameof(productId));

            ArgumentNullException.ThrowIfNull(files);

            if (files.Count == 0)
                throw new ArgumentException("At least one product image file is required.", nameof(files));

            foreach (StorageUploadFile file in files)
            {
                if (file.Length <= 0)
                    throw new ArgumentException($"Product image '{file.FileName}' is empty.", nameof(files));

                if (file.Length > ProductImageUploadPolicy.MaxFileSizeBytes)
                    throw new ArgumentException($"Product image '{file.FileName}' exceeds the maximum allowed file size.", nameof(files));

                string extension = Path.GetExtension(file.FileName);

                if (!ProductImageUploadPolicy.AllowedExtensions.Contains(extension))
                    throw new ArgumentException($"Product image '{file.FileName}' has an unsupported file extension.", nameof(files));

                if (!ProductImageUploadPolicy.AllowedContentTypes.Contains(file.ContentType))
                    throw new ArgumentException($"Product image '{file.FileName}' has an unsupported content type.", nameof(files));
            }

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new ArgumentException("Product id is not valid.", nameof(productId));

            Product? product = await _productReadRepository.GetByIdAsync(productGuid, cancellationToken, tracking: true);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            IReadOnlyList<StoredFile> uploadedFiles = await _storageService.UploadAsync(ProductImagesStorageLocation, files, cancellationToken);

            if (uploadedFiles.Count == 0)
                throw new InvalidOperationException("Product image upload failed: no files were uploaded.");

            List<ProductImageFile> productImageFiles = uploadedFiles
                .Select(file => new ProductImageFile
                {
                    FileName = file.FileName,
                    Path = file.Path,
                    Storage = _storageService.Provider.ToString(),
                    Product = new List<Product> { product }
                })
                .ToList();

            await _productImageFileWriteRepository.AddRangeAsync(productImageFiles);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ChangeCoverImageAsync(string productId, string imageId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product id is required.", nameof(productId));

            if (string.IsNullOrWhiteSpace(imageId))
                throw new ArgumentException("Product image id is required.", nameof(imageId));

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new ArgumentException("Product id is not valid.", nameof(productId));

            if (!Guid.TryParse(imageId, out Guid imageGuid))
                throw new ArgumentException("Product image id is not valid.", nameof(imageId));

            ProductImageFile? selectedImage =
                await _productImageFileReadRepository.GetByIdForProductAsync(productGuid, imageGuid, cancellationToken, tracking: true);

            if (selectedImage is null)
                throw new KeyNotFoundException("Selected product image not found.");

            if (selectedImage.CoverImage)
                return;

            ProductImageFile? currentCoverImage =
                await _productImageFileReadRepository.GetCoverByProductIdAsync(productGuid, cancellationToken, tracking: true);

            if (currentCoverImage is not null)
                currentCoverImage.CoverImage = false;

            selectedImage.CoverImage = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveProductImageAsync(string productId, string imageId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product id is required.", nameof(productId));

            if (string.IsNullOrWhiteSpace(imageId))
                throw new ArgumentException("Product image id is required.", nameof(imageId));

            if (!Guid.TryParse(productId, out Guid productGuid))
                throw new ArgumentException("Product id is not valid.", nameof(productId));

            if (!Guid.TryParse(imageId, out Guid imageGuid))
                throw new ArgumentException("Product image id is not valid.", nameof(imageId));

            ProductImageFile? productImage = await _productImageFileReadRepository.GetByIdForProductAsync(productGuid, imageGuid, cancellationToken);

            if (productImage is null)
                throw new KeyNotFoundException("Product image not found.");

            _productImageFileWriteRepository.Remove(productImage);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _storageService.DeleteAsync(ProductImagesStorageLocation, productImage.FileName, cancellationToken);
        }

        public async Task RemoveProductAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Product id is required.", nameof(id));

            if (!Guid.TryParse(id, out Guid productGuid))
                throw new ArgumentException("Product id is not valid.", nameof(id));

            Product? product = await _productReadRepository.GetByIdWithImagesAsync(productGuid, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            List<ProductImageFile> productImages = product.ProductImageFiles.ToList();

            if (productImages.Count > 0)
                _productImageFileWriteRepository.RemoveRange(productImages);

            _productWriteRepository.Remove(product);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (ProductImageFile image in productImages)
            {
                await _storageService.DeleteAsync(
                    ProductImagesStorageLocation,
                    image.FileName,
                    cancellationToken);
            }
        }

        public async Task RemoveRangeProductAsync(IEnumerable<string> productIds, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(productIds);

            HashSet<Guid> productGuids = [];

            foreach (string productId in productIds)
            {
                if (string.IsNullOrWhiteSpace(productId))
                    throw new ArgumentException("Product id is required.", nameof(productIds));

                if (!Guid.TryParse(productId, out Guid productGuid))
                    throw new ArgumentException("Product id is not valid.", nameof(productIds));

                productGuids.Add(productGuid);
            }

            if (productGuids.Count == 0)
                throw new ArgumentException("At least one product must be selected for deletion.", nameof(productIds));

            List<Product> products = await _productReadRepository.GetByIdsWithImagesAsync(productGuids, cancellationToken);

            if (products.Count != productGuids.Count)
                throw new KeyNotFoundException("One or more products were not found.");

            List<ProductImageFile> productImages = products
                .SelectMany(product => product.ProductImageFiles)
                .DistinctBy(image => image.Id)
                .ToList();

            if (productImages.Count > 0)
                _productImageFileWriteRepository.RemoveRange(productImages);

            _productWriteRepository.RemoveRange(products);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (ProductImageFile image in productImages)
            {
                await _storageService.DeleteAsync(
                    ProductImagesStorageLocation,
                    image.FileName,
                    cancellationToken);
            }
        }
    }
}
