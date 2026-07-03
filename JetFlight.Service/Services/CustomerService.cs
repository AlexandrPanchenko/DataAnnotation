using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Exceptions;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.Models.Avatars;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.Loyalty;
using JetFlight.Shared.Models.Product;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using JetFlight.Shared.UserContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using JetFlight.Shared.Models;
using Serilog;

namespace JetFlight.Service.Services
{
    public interface ICustomerService
    {
        Task<bool> MarkAsReadAsync(string notificationId, int? customerId);
        Task SendTestNotificationAsync(int customerId, string title, string body, string type);
        Task MarkAllAsReadAsync(int? customerId);
        Task DeleteAllNotificationsAsync(int? customerId);
        Task<(PagedListDTO<NotificationHistoryDTO> Notifications, int TotalRecords, int UnreadRecords)> GetNotificationHistoryAsync(
            PagingDTO pagingDto,
            int? customerId);
        Task<AuthenticateResponse?> Authenticate(CustomerAuthenticateRequest model, Branches branchId, RegistrationPlatform platform);

        Task<CustomerDTO> Get();
        Task<PagedListDTO<AdminCustomerDTO>> GetAll(
            PagingDTO pagingDto,
            string? searchParam = null,
            byte? branchId = null,
            CustomerStatus? customerStatus = null,
            RegistrationPlatform? registrationPlatform = null,
            DateTime? dateOfRegistration = null,
            string? city = null);
        Task SendCode(RegistrationPlatform platform, CustomerSendAuthenticateCodeRequest model, Branches branchId, string? token = null);
        Task Update(CustomerUpdateDTO model, bool isAdmin = false);
        Task<AddCustomerCardDTOResponse> AddCard(AddCustomerCardDTO model);
        Task<bool> IsNumberMatched(string phoneNumber);
        Task SendPhoneNumberVerificationCode(string newPhoneNumber);
        Task<VerifyAndUpdatePhoneNumberResponse> VerifyAndUpdatePhoneNumber(string newPhoneNumber, string verificationCode);
        Task<VerifyEmailResponse> VerifyEmail(string token, byte? branchId = null);
        Task<bool> DeleteCustomerByIdAsync(int customerId);
        Task<AdminCustomerDTO> GetCustomerByIdAsync(int id);
        Task<PagedListDTO<ReceiptDTO>> GetPurchaseHistory(
            PagingDTO pagingDto,
            int? customerId = null,
            string? productName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            byte? branchId = null);

        Task<(int CustomerId, byte BranchId)> GetCustomerAndBranchByCardCode(string cardCode);

        /// <summary>
        /// Повертає (customerId, branchId) по картці. Не кидає, якщо картки немає або вона не прив'язана.
        /// (null, null) — картки немає в базі; (null, branchId) — картка є, але без користувача або користувач видалений.
        /// </summary>
        Task<(int? customerId, byte? branchId)> TryGetCustomerAndBranchByCardCode(string cardCode);

        /// <summary>
        /// Створює запис CustomerCard з Code та BranchId, якщо такої картки ще немає (для прийому чеків по невідомій картці).
        /// </summary>
        Task EnsureCustomerCardExists(string cardCode, byte branchId);

        Task<decimal> GetAvailableBonusesAsync(int customerId);

        Task<bool> UseCustomerBonusesAsync(UseBonusesDto model);
    }

    public partial class CustomerService : ICustomerService
    {
        private readonly IntegrationDataContext _dataContext;
        private readonly IDataUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IFirebaseService _firebaseService;
        private readonly IDistributedCache _cache;
        private readonly IMemoryCache _memoryCache;
        private readonly IJwtUtils _jwtUtils;
        private readonly IUserContext _userContext;
        private readonly IAvatarService _avatarService;
        private readonly SmsSettings _smsSettings;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _appSettings;

        public CustomerService(
            IntegrationDataContext dataContext,
            IDataUnitOfWork unitOfWork,
            INotificationService notificationService,
            IDistributedCache cache,
            IJwtUtils jwtUtils,
            IUserContext userContext,
            IAvatarService avatarService,
            IOptions<SmsSettings> smsSettings,
            ISubscriptionService subscriptionService,
            IMemoryCache memoryCache,
            IFirebaseService firebaseService,
            IRefreshTokenService refreshTokenService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AppSettings> appSettings
            )
        {
            _dataContext = dataContext;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cache = cache;
            _jwtUtils = jwtUtils;
            _firebaseService = firebaseService;
            _userContext = userContext;
            _avatarService = avatarService;
            _smsSettings = smsSettings.Value;
            _subscriptionService = subscriptionService;
            _memoryCache = memoryCache;
            _refreshTokenService = refreshTokenService;
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings.Value;
        }

        [GeneratedRegex(@"\D")]
        private static partial Regex NonDigitRegex();

        public Task<bool> IsNumberMatched(string phoneNumber)
        {
            return _dataContext.Customers
                .AnyAsync(x =>
                !x.IsBlocked && !x.IsDeleted && x.Id == _userContext.CustomerId && x.PhoneNumber == phoneNumber);
        }
        private async Task CheckIfPhoneNumberExists(string phoneNumber)
        {
            var phoneNumberExists = await _dataContext.Customers.AnyAsync(x => x.PhoneNumber == phoneNumber &&
                                   !x.IsDeleted);
            if (phoneNumberExists)
            {
                throw new InvalidOperationException("Користувач з введеним номером телефону вже існує");
            }
        }

        private static string GetPhoneNumberVerificationKey(string phoneNumber, string verificationCode)
            => $"phone_verification_{phoneNumber}_code_{verificationCode}";

        public async Task<VerifyAndUpdatePhoneNumberResponse> VerifyAndUpdatePhoneNumber(string newPhoneNumber, string verificationCode)
        {
            newPhoneNumber = EscapePhoneNumber(newPhoneNumber);
            await CheckIfPhoneNumberExists(newPhoneNumber);

            var key = GetPhoneNumberVerificationKey(newPhoneNumber, verificationCode);
            var value = await _cache.GetStringAsync(key);

            if (value != newPhoneNumber)
            {
                return new VerifyAndUpdatePhoneNumberResponse
                {
                    Success = false,
                    Message = "Не вірний код перевірки"
                };
            }

            var customer = await _dataContext.Customers.FirstAsync(x => x.Id == _userContext.CustomerId &&
                                   !x.IsBlocked && !x.IsDeleted);
            customer.PhoneNumber = newPhoneNumber;

            await _dataContext.SaveChangesAsync();
            await _cache.RemoveAsync(key);

            // Revoke all existing refresh tokens (phone number changed)
            await _refreshTokenService.RevokeAllCustomerTokensAsync(customer.Id, "Phone number changed");

            // Regenerate JWT token
            var jwtToken = _jwtUtils.GenerateJwtToken(customer, _userContext.BranchId!.Value);

            // Generate new refresh token
            var (ipAddress, userAgent) = GetClientInfo();
            var (refreshToken, _) = await _refreshTokenService.GenerateRefreshTokenAsync(
                customer.Id,
                (byte)_userContext.BranchId!.Value,
                ipAddress,
                userAgent
            );

            return new VerifyAndUpdatePhoneNumberResponse
            {
                Success = true,
                Message = "Номер телефону успішно оновлено",
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<VerifyEmailResponse> VerifyEmail(string token, byte? branchId = null)
        {
            var verificationToken = await _dataContext.EmailVerificationTokens
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Token == token && x.IsActive);

            if (verificationToken == null)
            {
                var homeRedirectUrl = GetHomeRedirectUrl(branchId);
                return new VerifyEmailResponse
                {
                    Success = false,
                    Message = "Токен верифікації не знайдено або неактивний",
                    RedirectUrl = homeRedirectUrl
                };
            }

            // Check if token is expired (24 hours)
            if (DateTime.UtcNow.Subtract(verificationToken.CreatedAt).TotalHours > 24)
            {
                verificationToken.IsActive = false;
                await _dataContext.SaveChangesAsync();
                // При помилці (прострочений токен) перенаправляємо на головну, а не на профіль.
                var expiredCustomer = verificationToken.Customer;

                string? expiredRedirectUrl = null;
                var expiredCustomerSetting = await _dataContext.CustomerSettings
                    .FirstOrDefaultAsync(x => x.CustomerId == expiredCustomer.Id);

                expiredRedirectUrl = GetHomeRedirectUrl(expiredCustomerSetting?.BranchId);

                return new VerifyEmailResponse
                {
                    Success = false,
                    Message = "Токен верифікації застарів. Будь ласка, заповніть опитувальник знову.",
                    RedirectUrl = expiredRedirectUrl
                };
            }

            var customer = verificationToken.Customer;
            customer.EmailVerified = true;
            verificationToken.IsActive = false;
            verificationToken.VerifiedAt = DateTime.UtcNow.SetKindUtc();

            await _dataContext.SaveChangesAsync();

            // Get customer setting to determine branch for bonus and redirect URL
            var customerSetting = await _dataContext.CustomerSettings
                .FirstOrDefaultAsync(x => x.CustomerId == customer.Id);

            // Check if this is the first time completing the personal questionnaire
            // If so, award bonus
            decimal? bonusAmount = null;
            string? redirectUrl = null;

            if (customer.PersonalQuestionaryCompletedAt.HasValue)
            {
                var questionary = await _dataContext.Questionaries
                    .FirstOrDefaultAsync(x => x.Name == PersonalDataQuestionaryConstants.Name && x.IsLocked);

                if (questionary != null && questionary.BonusReward.HasValue)
                {
                    if (customerSetting != null)
                    {
                        var card = await _dataContext.CustomerCards
                            .FirstOrDefaultAsync(x => x.CustomerId == customer.Id
                                && x.BranchId == customerSetting.BranchId && x.Type == CardType.Virtual);

                        if (card != null)
                        {
                            var bonusTransaction = new CustomerBonusTransaction
                            {
                                BranchId = customerSetting.BranchId,
                                Amount = questionary.BonusReward.Value,
                                AmountRemaining = questionary.BonusReward.Value,
                                Description = $"Бонус за підтвердження email",
                                CardCode = card.Code,
                            };

                            await _dataContext.CustomerBonusTransactions.AddAsync(bonusTransaction);
                            await _dataContext.SaveChangesAsync();
                            bonusAmount = questionary.BonusReward.Value;
                        }
                    }
                }
            }

            // Determine redirect URL based on branch and environment (profile page)
            if (customerSetting != null)
            {
                var redirectBranch = (Branches)customerSetting.BranchId;
                var profilePath = "/profile";

                if (_appSettings.IsProd)
                {
                    redirectUrl = redirectBranch == Branches.BirdJet 
                        ? $"https://www.birdjet.ua{profilePath}"
                        : $"https://www.catjet.online{profilePath}";
                }
                else
                {
                    redirectUrl = redirectBranch == Branches.BirdJet 
                        ? $"https://staging.birdjet.ua{profilePath}"
                        : $"https://staging.catjet.online{profilePath}";
                }
            }

            return new VerifyEmailResponse
            {
                Success = true,
                Message = "Email успішно підтверджено",
                BonusAmount = bonusAmount,
                RedirectUrl = redirectUrl
            };
        }

        private string? GetHomeRedirectUrl(byte? branchId)
        {
            var branch = branchId is 1 or 2 ? (Branches)branchId.Value : Branches.CatJet;
            if (_appSettings.IsProd)
                return branch == Branches.BirdJet ? "https://www.birdjet.ua/" : "https://www.catjet.online/";
            return branch == Branches.BirdJet ? "https://staging.birdjet.ua/" : "https://staging.catjet.online/";
        }

        public async Task<PagedListDTO<ReceiptDTO>> GetPurchaseHistory(

            PagingDTO pagingDto,
            int? customerId = null,
            string? productName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            byte? branchId = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-7);
            endDate ??= DateTime.UtcNow;
            customerId ??= _userContext.CustomerId;

            var stores = await _unitOfWork.Stores.GetAllStores().Where(x=>x.BranchId == branchId || branchId == null)
                .Select(st => new StoreDTO
                {
                    Number = st.Number,
                    Address = st.Address,
                    Address1 = st.Address2,
                    City = st.City.Name
                })
                .ToListAsync();

            var query = _dataContext.Receipts
                .Include(r => r.CustomerBonusTransaction)

                .Include(r => r.ReceiptProducts)
                .ThenInclude(rp => rp.Product)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponCombinationPriceDiscount)
                //.ThenInclude(rp => rp.Product)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponCombinationFixedPrice)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponAdditionalBonus)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponBonusMultiplier)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponDiscountPercent)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponDiscountAmount)

                //.Include(r => r.ReceiptCustomerCoupons)
                //.ThenInclude(rp => rp.CustomerCoupon)
                //.ThenInclude(rp => rp.Coupon)
                //.ThenInclude(rp => rp.CouponProductFixedPrice)
                //.ThenInclude(rp => rp.Product)

                .Include(r => r.CustomerCard)
                .AsNoTracking() // Prevent EF from trying to load all properties including potentially missing ProductCode
                .Where(r => r.CustomerCard.CustomerId == customerId &&
                            r.CreatedAt >= startDate.Value &&
                            r.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(r => r.ReceiptProducts.Any(rp => rp.Product.Title.Contains(productName)));
            }

            if (branchId.HasValue)
            {
                query = query.Where(r => r.BranchId == branchId);
            }

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAt)
                .GetPagedListAsync(pagingDto, r => new ReceiptDTO
            {
                Id = r.Id,
                CardCode = r.CardCode,
                BranchId = r.BranchId,
                CreatedAt = r.CreatedAt,
                TotalPrice = r.TotalAmount,
                TotalPriceWithDiscount = r.TotalAmountWithDiscount,
                Discount = r.TotalAmount - r.TotalAmountWithDiscount,
                StoreDetails = stores.FirstOrDefault(st => st.Number == r.StoreCode),
                ReceiptProducts = r.ReceiptProducts.Select(rp => new ReceiptProductDTO
                {
                    Id = rp.Id,
                    ProductCode = rp.Product?.Code ?? string.Empty, // Use Product navigation property instead of ProductCode column
                    ProductName = rp.Product?.Title ?? string.Empty,
                    ProductImage = rp.Product?.ImagePath ?? string.Empty,
                    Price = rp.Price,
                    Discount = rp.Discount,
                    Quantity = rp.Quantity,
                    ItemUnit = rp.ItemUnit.ToDisplayString(),
                    LineTotalAmount = rp.LineTotalAmount,
                    LineTotalAmountWithDiscount = rp.LineTotalAmountWithDiscount,
                    UsedCoupon = GetCouponForProductLine(rp.LineNo, r.ReceiptCustomerCoupons ?? new List<ReceiptCustomerCoupon>()),
                }).ToList(),
                    SpentBonuses = r.UsedBonusesSnapshot,
                    AccumulatedBonuses = r.AccumulatedBonusesSnapshot,
                    IsReturn = r.IsReturn,
                UsedCoupons = (r.ReceiptCustomerCoupons ?? new List<ReceiptCustomerCoupon>()).Where(x => x.LineNo == 0)
                    .Select(ToUsedCouponDTO)
                    .ToList(),
            });

            

            return pagedResult;
        }

        private static UsedCouponDTO? GetCouponForProductLine(int lineNo, List<ReceiptCustomerCoupon> coupons)
        {
            if (coupons == null || coupons.Count == 0)
            {
                return null;
            }
            var usedCoupon = coupons.FirstOrDefault(x => x.LineNo == lineNo);
            return usedCoupon == null ? null : ToUsedCouponDTO(usedCoupon);
        }

        private static UsedCouponDTO ToUsedCouponDTO(ReceiptCustomerCoupon receiptCustomerCoupon)
            => new UsedCouponDTO
            {
                Id = receiptCustomerCoupon.CustomerCoupon.Id,
                Name = receiptCustomerCoupon.CustomerCoupon.Coupon.Name,
                Type = receiptCustomerCoupon.CustomerCoupon.Coupon.Type,
                Reward = ToRewardDTO(receiptCustomerCoupon.CustomerCoupon.Coupon)
            };

        public static CouponRewardShortInfo ToRewardDTO(Coupon coupon)
    => coupon.Type switch
    {
        CouponType.AdditionalBonus => new CouponRewardShortInfo
        {
            Value = coupon.CouponAdditionalBonus.Bonus.ToString(),
        },
        CouponType.CombinationPriceDiscount => new CouponRewardShortInfo
        {
            Value = coupon.CouponCombinationPriceDiscount.Price.ToString(),
            Quantity = coupon.CouponCombinationPriceDiscount.Quantity,
            Product = coupon.CouponCombinationPriceDiscount.Product != null ? new ProductShortInfoDTO
            {
                Code = coupon.CouponCombinationPriceDiscount.Product.Code,
                Title = coupon.CouponCombinationPriceDiscount.Product.Title,
                Image = coupon.CouponCombinationPriceDiscount.Product.ImagePath,
            } : null,
        },
        CouponType.CombinationFixedPrice => new CouponRewardShortInfo
        {
            Value = coupon.CouponCombinationFixedPrice.FixedPrice.ToString(),
        },
        CouponType.DiscountPercent => new CouponRewardShortInfo
        {
            Value = coupon.CouponDiscountPercent.Percent.ToString(),
        },
        CouponType.DiscountAmount => new CouponRewardShortInfo
        {
            Value = coupon.CouponDiscountAmount.Amount.ToString(),
        },
        CouponType.BonusMultiplier => new CouponRewardShortInfo
        {
            Value = coupon.CouponBonusMultiplier.Multiplier.ToString(),
        },
        CouponType.ProductFixedPrice => new CouponRewardShortInfo
        {
            Value = coupon.CouponProductFixedPrice.Price.ToString(),
            Product = coupon.CouponProductFixedPrice.Product != null ? new ProductShortInfoDTO
            {
                Code = coupon.CouponProductFixedPrice.Product.Code,
                Title = coupon.CouponProductFixedPrice.Product.Title,
                Image = coupon.CouponProductFixedPrice.Product.ImagePath,
            } : null,
            Quantity = coupon.CouponProductFixedPrice.Quanitity,
        },
    };

        public async Task<AuthenticateResponse?> Authenticate(CustomerAuthenticateRequest model, Branches branchId, RegistrationPlatform platform)
        {
            model.PhoneNumber = EscapePhoneNumber(model.PhoneNumber);
            var cacheKey = GetAuthCodeCacheKey(model.PhoneNumber, branchId);
            var cachedCode = await _cache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(cachedCode))
            {
                return null;
            }
            var codeVariants = GetAuthCodeVariants(model.Code).ToList();
            if (!codeVariants.Any(v => string.Equals(v, cachedCode, StringComparison.Ordinal)))
            {
                return null;
            }

            var customer = await _dataContext.Customers
                .Include(x => x.CustomerSettings)
                .Include(x => x.CustomerCards)
                .FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber && !x.IsDeleted);

            if (customer == null)
            {
                customer = new Customer
                {
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow.SetKindUtc(),
                    RegistrationPlatform = platform,
                    CustomerSettings = new List<CustomerSetting>
                    {
                        new CustomerSetting
                        {
                            BranchId = (byte)branchId,
                            Avatar = _avatarService.GetDefaultAvatar(branchId),
                            EnableEmailNotifications = true,
                            EnableSmsNotifications = true,
                            EnablePushNotifications = true,
                            EnableSubscription = true,
                            CreatedAt = DateTime.UtcNow,
                        },
                    },
                };

                customer.CustomerCards = new List<CustomerCard>()
                {
                    new CustomerCard
                    {
                        Code = await GenerateCardCodeAsync((byte)branchId),
                        BranchId = (byte)branchId,
                        Type = CardType.Virtual,
                        CreatedAt = DateTime.UtcNow.SetKindUtc(),
                        UpdatedAt = DateTime.UtcNow.SetKindUtc(),
                    }
                };

                await _dataContext.Customers.AddAsync(customer);

                await _dataContext.SaveChangesAsync();

                await _subscriptionService.SubscribeAsync(new Shared.Models.Subscription.SubscriptionRequest
                {
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                }, branchId, CancellationToken.None);
            }
            else
            {
                if (customer.IsBlocked)
                {
                    throw new InvalidOperationException("Ваш профіль заблоковано. Зверніться будь ласка до адміністратора");
                }

                if (!customer.CustomerSettings.Any(x => x.BranchId == (byte)branchId))
                {
                    customer.CustomerSettings.Add(new CustomerSetting
                    {
                        BranchId = (byte)branchId,
                        Avatar = _avatarService.GetDefaultAvatar(branchId),
                        EnableEmailNotifications = true,
                        EnableSmsNotifications = true,
                        EnablePushNotifications = true,
                        EnableSubscription = true,
                    });

                    await _subscriptionService.SubscribeAsync(new Shared.Models.Subscription.SubscriptionRequest
                    {
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                    }, branchId, CancellationToken.None);
                }

                if (!customer.CustomerCards.Any(x => x.BranchId == (byte)branchId && x.Type == CardType.Virtual))
                {
                    customer.CustomerCards.Add(new CustomerCard
                    {
                        Code = await GenerateCardCodeAsync((byte)branchId),
                        BranchId = (byte)branchId,
                        Type = CardType.Virtual,
                        CreatedAt = DateTime.UtcNow.SetKindUtc(),
                        UpdatedAt = DateTime.UtcNow.SetKindUtc(),
                    });
                }

                await _dataContext.SaveChangesAsync();
            }

            var jwtToken = _jwtUtils.GenerateJwtToken(customer, branchId);

            // Generate refresh token for customer (30-day lifetime)
            var (ipAddress, userAgent) = GetClientInfo();
            var (refreshToken, _) = await _refreshTokenService.GenerateRefreshTokenAsync(
                customer.Id,
                (byte)branchId,
                ipAddress,
                userAgent
            );

            await _cache.RemoveAsync(cacheKey);

            return new AuthenticateResponse(jwtToken, refreshToken);
        }

        /// <summary>
        /// Extracts client IP address and user agent from HTTP context
        /// </summary>
        private (string? ipAddress, string? userAgent) GetClientInfo()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return (null, null);
            }

            // Get IP address (supports X-Forwarded-For for proxies/load balancers)
            var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            // Get user agent
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();

            return (ipAddress, userAgent);
        }



        private async Task ValidateRecaptchaTokenAsync(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("reCAPTCHA token is required for WEB platform.");
            }

            var isValidCaptcha = await VerifyRecaptchaTokenAsync(token);
            if (!isValidCaptcha)
            {
                throw new InvalidOperationException("Invalid reCAPTCHA token.");
            }
        }
        private async Task<bool> VerifyRecaptchaTokenAsync(string token)
        {
            var cacheKey = $"Recaptcha_{token}";

            // Check if the result is already cached
            if (_memoryCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult; // Return the cached result
            }
            var url = "https://www.google.com/recaptcha/api/siteverify";
            var secretKey = Environment.GetEnvironmentVariable("GOOGLE_RECAPTCHA_KEY");

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            var recaptchaResponse = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResponse>(responseBody);
            if (!recaptchaResponse.success)
            {
                throw new InvalidOperationException(responseBody);
            }
            _memoryCache.Set(cacheKey, recaptchaResponse.success, TimeSpan.FromMinutes(2));
            return recaptchaResponse != null && recaptchaResponse.success;
        }


        public async Task SendPhoneNumberVerificationCode(string newPhoneNumber)
        {
            newPhoneNumber = EscapePhoneNumber(newPhoneNumber);
            await CheckIfPhoneNumberExists(newPhoneNumber);

            var code = Random.Shared.Next(0, 10000).ToString("D4");
            await _cache.SetStringAsync(GetPhoneNumberVerificationKey(newPhoneNumber, code), newPhoneNumber, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            });
            var branch = _userContext.BranchId!.Value;
            var message = new SmsMessage
            {
                Message = $"Код перевірки номеру телефона - {code}",
                Recievers = new List<string> { newPhoneNumber },
            };

            switch ((byte)branch)
            {
                case 1:
                    message.Sender = _smsSettings.From.BirdJet.Id;
                    break;
                case 2:
                    message.Sender = _smsSettings.From.CatJet.Id;
                    break;
                default:
                    throw new ArgumentException("Invalid branch ID");
            }

            await _notificationService.SendSmsAsync(message);
        }
        public async Task SendCode(RegistrationPlatform platform, CustomerSendAuthenticateCodeRequest model, Branches branchId, string? token = null)
        {
            // Вхідний номер до нормалізації
            var rawPhone = model.PhoneNumber;
            Log.Information("[CustomerService.SendCode] Start. Platform={Platform}, BranchId={BranchId}, RawPhone={RawPhone}", platform, branchId, rawPhone);

            if (platform == RegistrationPlatform.Web)
            {
                await ValidateRecaptchaTokenAsync(token);
            }

            model.PhoneNumber = EscapePhoneNumber(model.PhoneNumber);
            Log.Information("[CustomerService.SendCode] Normalized phone: {Phone}", model.PhoneNumber);

            // Check if user exists and is blocked before sending code
            var existingCustomer = await _dataContext.Customers
                .FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber && !x.IsDeleted);

            if (existingCustomer != null && existingCustomer.IsBlocked)
            {
                Log.Warning("[CustomerService.SendCode] Customer is blocked. Phone={Phone}, CustomerId={CustomerId}", model.PhoneNumber, existingCustomer.Id);
                throw new InvalidOperationException("Ваш профіль заблоковано. Зверніться будь ласка до адміністратора");
            }

            var code = Random.Shared.Next(0, 10000).ToString("D4");
            var cacheKey = GetAuthCodeCacheKey(model.PhoneNumber, branchId);
            // Один ключ на телефон+гілку: при повторній відправці коду старий перезаписується і переставає діяти
            await _cache.SetStringAsync(cacheKey, code, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            });

            var maskedCode = $"***{code[^2..]}";
            Log.Information("[CustomerService.SendCode] Generated auth code for phone {Phone}, BranchId={BranchId}, CodeMasked={MaskedCode}",
                model.PhoneNumber, branchId, maskedCode);

            var message = new SmsMessage
            {
                Message = $"Код авторизації - {code}",
                Recievers = new List<string> { model.PhoneNumber },
            };

            switch ((byte)branchId)
            {
                case 1:
                    message.Sender = _smsSettings.From.BirdJet.Id;
                    break;
                case 2:
                    message.Sender = _smsSettings.From.CatJet.Id;
                    break;
                default:
                    Log.Error("[CustomerService.SendCode] Invalid branch ID: {BranchId} for phone {Phone}", branchId, model.PhoneNumber);
                    throw new ArgumentException("Invalid branch ID");
            }

            Log.Information("[CustomerService.SendCode] Sending SMS via NotificationService. Phone={Phone}, SenderId={SenderId}, Platform={Platform}, BranchId={BranchId}",
                model.PhoneNumber, message.Sender, platform, branchId);

            await _notificationService.SendSmsAsync(message);

            Log.Information("[CustomerService.SendCode] SMS send requested successfully. Phone={Phone}, SenderId={SenderId}, Platform={Platform}, BranchId={BranchId}",
                model.PhoneNumber, message.Sender, platform, branchId);
        }

        public async Task<CustomerDTO> Get()
        {
            var customer = await _dataContext.Customers
                .Include(x => x.CustomerCards.Where(x => x.BranchId == (byte)_userContext.BranchId!))
                .Include(x => x.CustomerSettings.Where(x => x.BranchId == (byte)_userContext.BranchId!))
                .AsNoTracking()
                .FirstAsync(x => x.Id == _userContext.CustomerId &&
                                   !x.IsBlocked && !x.IsDeleted);
            var dto = ToDTO(customer);

            (dto.AvailableBonuses, dto.UsedBonuses) = await GetBonusesInfoAsync(dto.Id);
            return dto;
        }

        public async Task<AdminCustomerDTO> GetCustomerByIdAsync(int id)
        {
            var customer = await _dataContext.Customers.Where(x=>x.Id == id)
                .Include(x => x.CustomerCards)
                .ThenInclude(x=>x.CustomerBonusTransactions)
                .Include(x => x.CustomerSettings)
                .AsNoTracking()
                .FirstAsync();
            var dto = ToAdminCustomerDTO(customer);
            (dto.AvailableBonuses, dto.UsedBonuses) = await GetBonusesInfoAsync(dto.Id);

            return dto;
        }

        public async Task<bool> DeleteCustomerByIdAsync(int customerId)
        {
            var customer = await _dataContext.Customers
                .Include(c => c.CustomerCards)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null)
            {
                return false;
            }

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTime.UtcNow;

            // Block all cards of this customer when they are deleted
            if (customer.CustomerCards != null && customer.CustomerCards.Any())
            {
                foreach (var card in customer.CustomerCards)
                {
                    if (!card.IsBlocked)
                    {
                        card.IsBlocked = true;
                        card.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            _dataContext.Customers.Update(customer);
            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, int? customerId)
        {
            var notification = await _dataContext.NotificationHistories
                .FirstOrDefaultAsync(nh => nh.MessageId == notificationId 
                    && nh.CustomerId == customerId 
                    && nh.BranchId == (byte)_userContext.BranchId!.Value);

            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            _dataContext.NotificationHistories.Update(notification);
            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task DeleteAllNotificationsAsync(int? customerId)
        {
            var notifications = await _dataContext.NotificationHistories
                .Where(nh => nh.CustomerId == customerId 
                    && nh.BranchId == (byte)_userContext.BranchId!.Value)
                .ToListAsync();

            _dataContext.NotificationHistories.RemoveRange(notifications);
            await _dataContext.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int? customerId)
        {
            var notifications = await _dataContext.NotificationHistories
                .Where(nh => nh.CustomerId == customerId 
                    && !nh.IsRead 
                    && nh.BranchId == (byte)_userContext.BranchId!.Value)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            _dataContext.NotificationHistories.UpdateRange(notifications);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<(PagedListDTO<NotificationHistoryDTO> Notifications, int TotalRecords, int UnreadRecords)> GetNotificationHistoryAsync(
            PagingDTO pagingDto,
            int? customerId)
        {
            // Build the query for notification history
            var query = _dataContext.NotificationHistories
                .Where(nh => nh.CustomerId == customerId 
                    && nh.BranchId == (byte)_userContext.BranchId!.Value)
                .AsNoTracking();

            // Calculate the total number of records
            var totalRecords = await query.CountAsync();

            // Calculate the number of unread notifications
            var unreadRecords = await query.CountAsync(nh => !nh.IsRead);

            // Get paginated results
            var notifications = await query.GetPagedListAsync(pagingDto, ToNotificationHistoryDTO);

            return (notifications, totalRecords, unreadRecords);
        }

        public async Task SendTestNotificationAsync(int customerId, string title, string body, string type)
        {
            var customerSetting = await _dataContext.CustomerSettings
                .FirstOrDefaultAsync(cs => cs.CustomerId == customerId && cs.EnablePushNotifications == true);

            if (customerSetting == null || string.IsNullOrEmpty(customerSetting.PushNotificationToken))
            {
                throw new InvalidOperationException($"No valid push notification token found for customer ID: {customerId}");
            }

            // Send the notification using FirebaseService
            var messageId = await _firebaseService.SendTestMessageAsync(title, body,customerId, type);
            if(string.IsNullOrEmpty(messageId))
            {
                throw new InvalidOperationException("Failed to send notification.");
            }
            // Log the notification to the NotificationHistory table
            var notificationHistory = new NotificationHistory
            {
                CustomerId = customerId,
                BranchId = customerSetting.BranchId,
                Title = title,
                Body = body,
                IsRead = false,
                MessageId = messageId,
                Type = type,
                CreatedAt = DateTime.UtcNow.SetKindUtc()
            };

            await _dataContext.NotificationHistories.AddAsync(notificationHistory);
            await _dataContext.SaveChangesAsync();
        }

        private NotificationHistoryDTO ToNotificationHistoryDTO(NotificationHistory notification)
        {
            return new NotificationHistoryDTO
            {
                Id = notification.Id,
                Title = notification.Title,
                Body = notification.Body,
                IsRead = notification.IsRead,
                MessageId = notification.MessageId,
                Type = notification.Type,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<PagedListDTO<AdminCustomerDTO>> GetAll(
          PagingDTO pagingDto,
          string? searchParam = null,
          byte? branchId = null,
          CustomerStatus? customerStatus = null,
          RegistrationPlatform? registrationPlatform = null,
          DateTime? dateOfRegistration = null,
          string? city = null)
        {
            var query = _dataContext.Customers
                .Include(x => x.CustomerCards)
                .ThenInclude(x => x.CustomerBonusTransactions)
                .Include(x => x.CustomerSettings)
                .AsNoTracking();

            query = ApplyBranchFilter(query, branchId);
            query = ApplyCustomerStatusFilter(query, customerStatus);
            query = ApplyDateOfRegistrationFilter(query, dateOfRegistration);
            query = ApplyRegistrationPlatformFilter(query, registrationPlatform);
            query = ApplyCityFilter(query, city);
            query = ApplySearchFilter(query, searchParam);

            var customers = await query.GetPagedListAsync(pagingDto, ToAdminCustomerDTO);

            foreach (var customer in customers.Items)
            {
                (customer.AvailableBonuses, customer.UsedBonuses) = await GetBonusesInfoAsync(customer.Id);
            }

            return customers;
        }
        private IQueryable<Customer> ApplyCustomerStatusFilter(IQueryable<Customer> query, CustomerStatus? customerStatus)
        {
            if (customerStatus.HasValue)
            {
                switch (customerStatus.Value)
                {
                    case CustomerStatus.Active:
                        query = query.Where(x => !x.IsBlocked && !x.IsDeleted);
                        break;
                    case CustomerStatus.Blocked:
                        query = query.Where(x => x.IsBlocked && !x.IsDeleted);
                        break;
                    case CustomerStatus.Deleted:
                        query = query.Where(x => x.IsDeleted);
                        break;
                }
            }
            return query;
        }

        private IQueryable<Customer> ApplySearchFilter(IQueryable<Customer> query, string? searchParam)
        {
            if (!string.IsNullOrEmpty(searchParam))
            {
                query = query.Where(x =>
                    (x.FirstName != null && x.FirstName.Contains(searchParam)) ||
                    (x.LastName != null && x.LastName.Contains(searchParam)) ||
                    (x.Email != null && x.Email.Contains(searchParam)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(searchParam))
                );
            }
            return query;
        }

        private IQueryable<Customer> ApplyCityFilter(IQueryable<Customer> query, string? city)
        {
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(x => x.City == city);
            }
            return query;
        }

        private IQueryable<Customer> ApplyRegistrationPlatformFilter(IQueryable<Customer> query, RegistrationPlatform? registrationPlatform)
        {
            if (registrationPlatform.HasValue)
            {
                query = query.Where(x => x.RegistrationPlatform == registrationPlatform.Value);
            }
            return query;
        }
        private IQueryable<Customer> ApplyDateOfRegistrationFilter(IQueryable<Customer> query, DateTime? dateOfRegistration)
        {
            if (dateOfRegistration.HasValue)
            {
                query = query.Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value.Date == dateOfRegistration.Value.Date);
            }
            return query;
        }
        private IQueryable<Customer> ApplyBranchFilter(IQueryable<Customer> query, byte? branchId)
        {
            if (branchId.HasValue)
            {
                query = query.Where(x => x.CustomerSettings != null && x.CustomerSettings.Any(cs => cs.BranchId == branchId));
            }
            return query;
        }



        public async Task Update(CustomerUpdateDTO model, bool isAdmin = false)
        {
            var customer = isAdmin == true ? await _dataContext.Customers
                .Include(x => x.CustomerSettings)
                .FirstAsync(x => x.Id == model.Id) : await _dataContext.Customers
                .Include(x => x.CustomerSettings)
                .FirstAsync(x => x.Id == _userContext.CustomerId);

            var oldEmail = customer.Email;
            var oldPhoneNumber = customer.PhoneNumber;
            var oldFirstName = customer.FirstName;
            var oldLastName = customer.LastName;

            if (!string.IsNullOrEmpty(model.FirstName))
            {
                customer.FirstName = model.FirstName;
            }

            if (!string.IsNullOrEmpty(model.LastName))
            {
                customer.LastName = model.LastName;
            }
            if (model.IsBlocked.HasValue)
            {
                customer.IsBlocked = model.IsBlocked.Value;
                
                var customerCards = await _dataContext.CustomerCards
                    .Where(x => x.CustomerId == customer.Id)
                    .ToListAsync();
                
                // If customer is being blocked, also block all their cards
                if (model.IsBlocked.Value)
                {
                    foreach (var card in customerCards)
                    {
                        card.IsBlocked = true;
                        card.UpdatedAt = DateTime.UtcNow.SetKindUtc();
                    }
                }
                // If customer is being unblocked, also unblock all their cards
                else
                {
                    foreach (var card in customerCards)
                    {
                        card.IsBlocked = false;
                        card.UpdatedAt = DateTime.UtcNow.SetKindUtc();
                    }
                }
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                customer.Email = model.Email;
            }

            if (model.Birthday.HasValue)
            {
                customer.Birthday = model.Birthday;
            }

            if (!string.IsNullOrEmpty(model.Country))
            {
                customer.Country = model.Country;
            }

            if (!string.IsNullOrEmpty(model.WhereFindOut) && isAdmin)
            {
                customer.WhereFindOut = model.WhereFindOut;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber) && isAdmin)
            {
                customer.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(model.City))
            {
                customer.City = model.City;
            }

            if (!string.IsNullOrEmpty(model.Street))
            {
                customer.Street = model.Street;
            }

            if (!string.IsNullOrEmpty(model.Longitude))
            {
                customer.Longitude = model.Longitude;
            }

            if (!string.IsNullOrEmpty(model.Latitude))
            {
                customer.Latitude = model.Latitude;
            }

            if (model.Sex.HasValue)
            {
                customer.Sex = model.Sex;
            }

            if (model.TypeOfActivity.HasValue)
            {
                customer.TypeOfActivity = model.TypeOfActivity;
            }

            if (model.StoreNearHomeId.HasValue && isAdmin)
            {
                var storeExists = await _unitOfWork.Stores.Any(x => x.Id == model.StoreNearHomeId && x.isActive);
                if (!storeExists)
                {
                    throw new ArgumentException("Такого активного магазину нема в мережі.");
                }
                customer.StoreNearHomeId = model.StoreNearHomeId;
            }

            if (model.PersonalQuestionaryCompletedAt.HasValue && isAdmin)
            {
                customer.PersonalQuestionaryCompletedAt = model.PersonalQuestionaryCompletedAt;
            }

            if (model.DateOfRegistration.HasValue && isAdmin)
            {
                customer.CreatedAt = model.DateOfRegistration;
            }

            var oldSubscriptions = customer.CustomerSettings.Select(x => Tuple.Create(x.BranchId, x.EnableSubscription)).ToList();

            customer.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            var branchId = _userContext.BranchId;
            if (branchId.HasValue)
            {
                var settings = customer.CustomerSettings.FirstOrDefault(x => x.BranchId == (byte)branchId.Value);
                if (settings != null)
                {
                    settings.PushNotificationToken = model.PushNotificationToken ?? settings.PushNotificationToken;
                    settings.EnableEmailNotifications = model.EnableEmailNotifications ?? settings.EnableEmailNotifications;
                    settings.EnablePushNotifications = model.EnablePushNotifications ?? settings.EnablePushNotifications;
                    settings.EnableSmsNotifications = model.EnableSmsNotifications ?? settings.EnableSmsNotifications;
                    settings.EnableSubscription = model.EnableSubscription ?? settings.EnableSubscription;
                    settings.AutomaticWithdrawal = model.AutomaticWithdrawal ?? settings.AutomaticWithdrawal;
                    settings.AccumulateRest = model.AccumulateRest ?? settings.AccumulateRest;

                    if (model.AvatarKey.HasValue)
                    {
                        var allowedAvatarKeys = _avatarService.GetAvatarKeysPerBranch(branchId.Value);

                        if (!allowedAvatarKeys.Contains(model.AvatarKey.Value))
                        {
                            throw new ArgumentException("Цей аватар не доступний.");
                        }

                        settings.Avatar = _avatarService.GetAvatarPath(model.AvatarKey.Value);
                    }

                    if (model.StoreId.HasValue)
                    {
                        var storeExists = await _unitOfWork.Stores.Any(x => x.Id == model.StoreId && x.isActive && x.BranchId == (byte)branchId.Value);
                        if (!storeExists)
                        {
                            throw new ArgumentException("Такого активного магазину нема в мережі.");
                        }
                        settings.ActiveStoreId = model.StoreId;
                    }
                }
            }

            await _dataContext.SaveChangesAsync();

            await ApplySubscriptionChangesAsync(oldSubscriptions, customer, oldPhoneNumber, oldEmail, oldFirstName, oldLastName);
        }

        private async Task ApplySubscriptionChangesAsync(
            List<Tuple<byte, bool>> oldSubscriptions,
            Customer customer,
            string oldPhoneNumber,
            string? oldEmail,
            string? oldFirstName,
            string? oldLastName)
        {
            foreach (var (branchId, flag) in oldSubscriptions)
            {
                var newSubscription = customer.CustomerSettings.First(x => x.BranchId == branchId);
                if ((oldPhoneNumber != customer.PhoneNumber || oldEmail != customer.Email || !newSubscription.EnableSubscription) && flag)
                {
                    var sendGridContact = await _subscriptionService.GetContactAsync((Branches)branchId, oldPhoneNumber, oldEmail, CancellationToken.None);
                    if (sendGridContact != null)
                    {
                        await _subscriptionService.UnsubscribeAsync(sendGridContact.Id, CancellationToken.None);
                    }
                }

                if (newSubscription.EnableSubscription
                        && (!flag
                        || oldPhoneNumber != customer.PhoneNumber
                        || oldEmail != customer.Email
                        || oldFirstName != customer.FirstName
                        || oldLastName != customer.LastName))
                {
                    await _subscriptionService.SubscribeAsync(new Shared.Models.Subscription.SubscriptionRequest
                    {
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                    }, (Branches)newSubscription.BranchId, CancellationToken.None);
                }
            }
        }

        private string GetAvatarPath(AvatarKey key)
            => new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
            {
                Path = $"{StorageConstants.AppPath}/Avatars/{key}.png"
            }.ToString();

        private AvatarKey GetAvatarKey(string path)
        {
            var file = Path.GetFileName(path);
            var key = file.Replace(".png", string.Empty);

            return Enum.Parse<AvatarKey>(key);
        }

        private static List<AvatarKey> GetAvatarKeysPerBranch(Branches branchId)
        {
            return Enum.GetValues<AvatarKey>()
                .Where(x => IsAvatarForBranch(x, branchId))
                .ToList();
        }

        private static bool IsAvatarForBranch(AvatarKey avatarKey, Branches branchId)
        {
            var attribute = avatarKey.GetCustomAttribute<AvatarFilterAttribute>();
            return attribute == null || attribute.BranchId == branchId;
        }

        public List<AvatarDTO> GetAvatars(Branches branchId)
        {
            var keys = GetAvatarKeysPerBranch(branchId);
            var avatars = keys.Select(x => new AvatarDTO
            {
                Key = x,
                Path = GetAvatarPath(x),
                Type = Enum.Parse<AvatarType>(x.GetEnumMemberValue()),
            }).ToList();

            return avatars;
        }

        private AdminCustomerDTO ToAdminCustomerDTO(Customer customer)
        {
            var dto = new AdminCustomerDTO
            {
                PhoneNumber = customer.PhoneNumber,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Birthday = customer.Birthday,
                City = customer.City,
                Street = customer.Street,
                Latitude = customer.Latitude,
                Longitude = customer.Longitude,
                Sex = customer.Sex,
                Country = customer.Country,
                Id = customer.Id,
                DateOfRegistration = customer.CreatedAt,
                RegistrationPlatform = customer.RegistrationPlatform,
                CustomerStatus = GetCustomerStatus(customer),
                Setting = ToCustomerSettingsDTO(customer.CustomerSettings.First()),
                TypeOfActivity = customer.TypeOfActivity,
                PersonalQuestionaryCompletedAt = customer.PersonalQuestionaryCompletedAt,
                StoreNearHomeId = customer.StoreNearHomeId,
                WhereFindOut = customer.WhereFindOut,
                EmailVerified = customer.EmailVerified,
                NumberOfChildren = customer.NumberOfChildren,
            };

            if (customer.CustomerCards != null)
            {
                dto.Cards = customer.CustomerCards.Select(x =>
                {
                    var cardDto = new AdminCustomerCardDTO
                    {
                        Code = x.Code,
                        IsBlocked = x.IsBlocked,
                        Type = x.Type,
                        BranchId = (Branches)x.BranchId,
                    };

                    cardDto.AdminCustomerTransactionsDTO = x.CustomerBonusTransactions.Select(t => new AdminCustomerTransactionDTO
                    {
                        Id = t.Id,
                        BranchId = t.BranchId,
                        CardCode = t.CardCode,
                        Description = t.Description,
                        Amount = t.Amount,
                        AmountRemaining = t.AmountRemaining,
                        TransactionDate = t.TransactionDate,
                        TransactionType = t.TransactionType,
                        ExpiredAt = t.ExpiredAt
                    }).ToList();

                    return cardDto;
                }).ToList();
            };

            return dto;
        }
        private CustomerStatus GetCustomerStatus(Customer customer)
        {
            if (customer.IsDeleted == true)
            {
                return CustomerStatus.Deleted;
            }
            if (customer.IsBlocked == true)
            {
                return CustomerStatus.Blocked;
            }
            return CustomerStatus.Active;
        }
        private CustomerDTO ToDTO(Customer customer)
        {
            var dto = new CustomerDTO
            {
                PhoneNumber = customer.PhoneNumber,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Birthday = customer.Birthday,
                City = customer.City,
                Street = customer.Street,
                Latitude = customer.Latitude,
                Longitude = customer.Longitude,
                Sex = customer.Sex,
                Id = customer.Id,
                Setting = ToCustomerSettingsDTO(customer.CustomerSettings.First()),
                TypeOfActivity = customer.TypeOfActivity,
                PersonalQuestionaryCompletedAt = customer.PersonalQuestionaryCompletedAt,
                StoreNearHomeId = customer.StoreNearHomeId,
                WhereFindOut = customer.WhereFindOut,
            };

            dto.Cards = customer.CustomerCards.Select(card => new CustomerCardDTO
            {
                Code = card.Code,
                IsBlocked = card.IsBlocked,
                Type = card.Type,
                BranchId = (Branches)card.BranchId,
            }).ToList();

            return dto;
        }

        /// <summary>
        /// Один ключ на пару телефон+гілка: зберігається лише останній відправлений код.
        /// При повторній відправці коду старий перезаписується.
        /// </summary>
        private static string GetAuthCodeCacheKey(string phoneNumber, Branches branchId)
            => $"customer_auth_phone_{phoneNumber}_branch_{(byte)branchId}";

        /// <summary>
        /// Повертає варіанти коду для порівняння: як є, та з ведучими нулями (0123).
        /// Клієнт може надіслати "123" замість "0123", тому приймаємо обидва варіанти.
        /// </summary>
        private static IEnumerable<string> GetAuthCodeVariants(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                yield return string.Empty;
                yield break;
            }
            var trimmed = code.Trim();
            yield return trimmed;
            if (trimmed.Length > 0 && trimmed.Length < 4 && trimmed.All(char.IsDigit))
            {
                yield return trimmed.PadLeft(4, '0');
            }
        }

        public async Task<AddCustomerCardDTOResponse> AddCard(AddCustomerCardDTO model)
        {
            var branchIdNumber = model.Code[4];
            var cardBranchId = int.Parse(branchIdNumber.ToString());

            if (cardBranchId != (int)_userContext.BranchId!.Value)
            {
                throw new ArgumentException("Картка не належить мережі");
            }

            var customer = await _dataContext.Customers
                .Include(x => x.CustomerCards)
                .FirstAsync(x => x.Id == _userContext.CustomerId &&
                                   !x.IsBlocked &&
                                   !x.IsDeleted);

            if (customer.CustomerCards.Any(x => x.BranchId == (byte) _userContext.BranchId!.Value  && x.Type == CardType.Plastic))
            {
                return new AddCustomerCardDTOResponse
                {
                    Errors = new List<string> { "Не можливо додати ще одну картку." },
                };
            }

            var card = (await _dataContext.CustomerCards.FirstOrDefaultAsync(x => x.Code == model.Code))
                ?? new CustomerCard
                {
                    Code = model.Code,
                    BranchId = (byte) _userContext.BranchId!.Value,
                    Type = CardType.Plastic,
                    CreatedAt = DateTime.UtcNow.SetKindUtc(),
                    UpdatedAt = DateTime.UtcNow.SetKindUtc(),
                };

            if (card.IsBlocked
                || card.CustomerId.HasValue
                || card.BranchId != (byte)_userContext.BranchId!.Value
                || card.Type != CardType.Plastic)
            {
                return new AddCustomerCardDTOResponse
                {
                    Errors = new List<string> { "Не можливо використати цю картку." },
                };
            }

            card.UpdatedAt = DateTime.UtcNow.SetKindUtc();
            customer.CustomerCards.Add(card);
            await _dataContext.SaveChangesAsync();

            return new AddCustomerCardDTOResponse
            {
                Item = new CustomerCardDTO
                {
                    Code = card.Code,
                    Type = card.Type,
                    BranchId = (Branches)card.BranchId,
                    IsBlocked = card.IsBlocked,
                }
            };
        }

        private CustomerSettingsDTO ToCustomerSettingsDTO(CustomerSetting setting)
        {
            var avatar = setting.Avatar;

            if (avatar == string.Empty)
            {
                avatar = _avatarService.GetDefaultAvatar((Branches)setting.BranchId);
            }

            var avatarKey = GetAvatarKey(avatar);
            return new CustomerSettingsDTO
            {
                ActiveStoreId = setting.ActiveStoreId,
                EnableEmailNotifications = setting.EnableEmailNotifications,
                EnablePushNotifications = setting.EnablePushNotifications,
                PushNotificationToken = setting.PushNotificationToken,
                EnableSmsNotifications = setting.EnableSmsNotifications,
                EnableSubscription = setting.EnableSubscription,
                AccumulateRest = setting.AccumulateRest,
                AutomaticWithdrawal = setting.AutomaticWithdrawal,
                BranchId = setting.BranchId,
                Avatar = new AvatarDTO
                {
                    Key = avatarKey,
                    Path = avatar,
                    Type = Enum.Parse<AvatarType>(avatarKey.GetEnumMemberValue()),
                }
            };
        }

        private static string EscapePhoneNumber(string phoneNumber)
            => "+" + NonDigitRegex().Replace(phoneNumber, "");

        private async Task<string> GenerateCardCodeAsync(byte branchId)
        {
            string code;

            do
            {
                var uniqueIdentifier = Random.Shared.NextInt64(0, 10000000000).ToString("D10");
                code = $"7773{branchId}{uniqueIdentifier}";
            }
            while (await _dataContext.CustomerCards.AnyAsync(x => x.Code == code && x.BranchId == branchId));

            return code;
        }

        private async Task<(decimal, decimal)> GetBonusesInfoAsync(int customerId)
        {
            var availableAmount = await GetAvailableBonusesAsync(customerId);
            var usedBonuses = await _dataContext.BonusUsages
                .Where(x => x.CustomerBonusTransaction.CustomerCard.CustomerId == customerId)
                .SumAsync(x => x.Amount);

            return (availableAmount, usedBonuses);
        }

        public async Task<decimal> GetAvailableBonusesAsync(int customerId)
        {
            var availableAmount = await _dataContext.CustomerBonusTransactions.Where(x =>
                    x.CustomerCard.CustomerId == customerId && (!x.ExpiredAt.HasValue || x.ExpiredAt > DateTime.UtcNow)
                    && x.AmountRemaining > 0)
                .SumAsync(x => x.AmountRemaining);

            return availableAmount;
        }

        public async Task<(int CustomerId, byte BranchId)> GetCustomerAndBranchByCardCode(string cardCode)
        {
            var code = cardCode?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Код картки не задано");
            }

            var card = await _dataContext.CustomerCards
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Code == code && x.CustomerId.HasValue);

            if (card == null)
            {
                var cardExistsUnlinked = await _dataContext.CustomerCards
                    .AnyAsync(x => x.Code == code && !x.CustomerId.HasValue);
                if (cardExistsUnlinked)
                {
                    throw new NotFoundException("Картка знайдена, але не прив'язана до клієнта");
                }
                throw new NotFoundException("Кастомер з введеною карткою не знайден");
            }

            if (card.Customer.IsDeleted)
            {
                throw new NotFoundException("Кастомер з введеною карткою не знайден");
            }

            if (card.Customer.IsBlocked)
            {
                throw new BadRequestException("Кастомер заблокованний");
            }

            if (card.IsBlocked)
            {
                throw new BadRequestException("Картка заблокована");
            }

            var customerId = card.CustomerId!.Value;

            return (customerId, card.BranchId);
        }

        public async Task<(int? customerId, byte? branchId)> TryGetCustomerAndBranchByCardCode(string cardCode)
        {
            var code = cardCode?.Trim();
            if (string.IsNullOrWhiteSpace(code))
                return (null, null);

            var card = await _dataContext.CustomerCards
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Code == code);

            if (card == null)
                return (null, null);

            if (card.IsBlocked)
                throw new BadRequestException("Картка заблокована");

            if (!card.CustomerId.HasValue || card.Customer == null || card.Customer.IsDeleted)
                return (null, card.BranchId);

            if (card.Customer.IsBlocked)
                throw new BadRequestException("Кастомер заблокованний");

            return (card.CustomerId.Value, card.BranchId);
        }

        public async Task EnsureCustomerCardExists(string cardCode, byte branchId)
        {
            var code = cardCode?.Trim();
            if (string.IsNullOrWhiteSpace(code))
                return;

            var exists = await _dataContext.CustomerCards.AnyAsync(x => x.Code == code);
            if (exists)
                return;

            _dataContext.CustomerCards.Add(new CustomerCard
            {
                Code = code,
                BranchId = branchId,
                CustomerId = null,
                Type = CardType.Plastic,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
            await _dataContext.SaveChangesAsync();
        }

        public async Task<bool> UseCustomerBonusesAsync(UseBonusesDto model)
        {
            var (customerId, _) = await GetCustomerAndBranchByCardCode(model.CardCode);

            if (model.Amount <= 0)
            {
                throw new ArgumentException("Кількість бонусів невалідна");
            }

            // Перевіряємо чи вже є BonusUsage для цієї транзакції (idempotent behavior)
            var existingBonusUsages = await _dataContext.BonusUsages
                .Include(bu => bu.CustomerBonusTransaction)
                .Where(bu =>
                    bu.StoreCode == model.StoreCode
                    && bu.PosTerminal == model.PosTerminal
                    && bu.TransactionNumber == model.TransactionNumber)
                .ToListAsync();

            // Якщо є існуючі записи - відновлюємо бонуси та видаляємо їх
            if (existingBonusUsages.Any())
            {
                foreach (var bonusUsage in existingBonusUsages)
                {
                    if (bonusUsage.CustomerBonusTransaction != null)
                    {
                        bonusUsage.CustomerBonusTransaction.AmountRemaining += bonusUsage.Amount;
                    }
                }
                _dataContext.BonusUsages.RemoveRange(existingBonusUsages);
            }

            var customerBonusTransactions = await _dataContext
                    .CustomerBonusTransactions
                    .Where(x => 
                        x.CustomerCard.CustomerId == customerId    
                        && x.AmountRemaining > 0
                        && x.CardCode == model.CardCode
                        && (!x.ExpiredAt.HasValue || x.ExpiredAt > DateTime.UtcNow))
                    .OrderBy(x => x.ExpiredAt.HasValue)
                    .ThenBy(x => x.ExpiredAt)
                    .ToListAsync();

            var amount = model.Amount;

            foreach (var customerBonusTransaction in customerBonusTransactions)
            {
                var usedAmount = customerBonusTransaction.AmountRemaining > amount ? amount : customerBonusTransaction.AmountRemaining;
                customerBonusTransaction.AmountRemaining = customerBonusTransaction.AmountRemaining - usedAmount;
                amount -= usedAmount;

                 _dataContext.BonusUsages.Add(new BonusUsage
                {
                    Amount = usedAmount,
                    CustomerBonusTransactionId = customerBonusTransaction.Id,
                    PosTerminal = model.PosTerminal,
                    TransactionNumber = model.TransactionNumber,
                    StoreCode = model.StoreCode
                });

                if (amount == 0)
                {
                    break;
                }
            }

            if (amount > 0)
            {
                throw new ArgumentException("Недостатній баланс для списання");
            }


            await _dataContext.SaveChangesAsync();

            return true;
        }
    }
}
