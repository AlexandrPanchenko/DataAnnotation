using JetFlight.Service.HandlebarsHelpers.Models;
using JetFlight.Shared.Models.Message;
using HandlebarsDotNet;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Constants;

namespace JetFlight.Service.Services;

public interface IHtmlGenerationService
{
    Task<string> GenerateSetupPasswordEmail(string adminName, string url);
    Task<string> GenerateContactUsCreateEmail(string requestorName, string requestTopic, byte? branchId);
    Task<string> GenerateContactUsProcessedEmail(string requestorName, string requestMessage, string responseMessage, string resolveMessage, byte? branchId);
    Task<string> GenerateCouponIsAvailableEmail(string customerName, string couponName, byte? branchId);
    Task<string> GenerateAccumulationCardIsAvailableEmail(string customerName, string accumulationCardName, byte? branchId);
    Task<string> GenerateSavedPromotionExpiresTomorrowEmail(string customerName, string promotionTitle, byte? branchId);
    Task<string> GenerateSavedPromotionStartsTodayEmail(string customerName, string promotionTitle, byte? branchId);
    Task<string> GenerateTargetEmailAsync(TargetEmailHandlebarParameters parameters);
    Task<string> GenerateEmailVerificationEmail(string customerName, string verificationUrl, byte? branchId);
}

public class HtmlGenerationService : IHtmlGenerationService
{
    private const string SharedTemplatePath = "HandlebarsHelpers/Templates/SharedEmailTemplate.html";
    private const string ProcessedTemplatePath = "HandlebarsHelpers/Templates/ContactUsProcessedEmailTemplate.html";
    private const string TargetTemplatePath = "HandlebarsHelpers/Templates/TargetEmailTemplate.html";

    private static async Task<string> CompileTemplateAsync(string relativePath, object data)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (!File.Exists(path))
        {
            return GetFallbackEmailHtml("Посилання для підтвердження email не завантажилося. Заповніть опитувальник знову.", null);
        }
        var html = await File.ReadAllTextAsync(path);
        var template = Handlebars.Compile(html);
        var result = template(data);
        if (string.IsNullOrWhiteSpace(result))
        {
            return GetFallbackEmailHtml("Щоб отримати бонус, перейдіть за посиланням для підтвердження вашого email.", data);
        }
        return result;
    }

    private static string GetFallbackEmailHtml(string description, object? data)
    {
        var ctaUrl = data is Dictionary<string, object> dict && dict.TryGetValue("ctaUrl", out var url) ? url?.ToString() : null;
        var linkHtml = !string.IsNullOrEmpty(ctaUrl)
            ? $@"<p><a href=""{ctaUrl}"" style=""color:#33a853;font-weight:bold"">Перейти за посиланням</a></p>"
            : "";
        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head><body style=""font-family:sans-serif;padding:1rem""><p>{description}</p>{linkHtml}</body></html>";
    }

    private static BranchDataHandlebarDto GetBranchData(Branches branchId)
    {
        var image = branchId == Branches.BirdJet ? BranchImageConstants.BirdJet : BranchImageConstants.CatJet;

        // Use real contact emails for each chain
        var email = branchId == Branches.BirdJet ? "info@birdjet.ua" : "info@catjet.online";

        var logoUrl = new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!)
        {
            Path = $"{StorageConstants.AppPath}/{image}"
        }.ToString();

        return new BranchDataHandlebarDto
        {
            Email = email,
            LogoUrl = logoUrl
        };
    }

    public async Task<string> GenerateSetupPasswordEmail(string adminName, string url)
    {
        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {adminName}" },
            { "Description", "Для вас щойно був створений профіль у адмін порталі. Будь ласка перейдіть за посиланням для оновлення паролю." },
            { "ctaUrl", url },
            { "helpText", "Посилання діє протягом 24 годин з моменту створення." },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }

    public async Task<string> GenerateContactUsCreateEmail(string requestorName, string requestTopic, byte? branchId)
    {
        var branchData = GetBranchData((Branches) branchId);

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {requestorName}" },
            { "Description", $"Заявка на тему \"{requestTopic}\" була створена. Ми обов’язково надамо відповідь" },
            { "branchData", branchData },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }

    public async Task<string> GenerateContactUsProcessedEmail(string requestorName, string requestMessage, string responseMessage, string resolveMessage, byte? branchId)
    {
        var branchData = GetBranchData((Branches)branchId);

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {requestorName}" },
            { "Description", "Ваша заявка була оброблена." },
            { "RequestMessage", requestMessage },
            { "ResponseMessage", responseMessage },
            { "ResolveMessage", resolveMessage },
            { "branchData", branchData },
        };

        return await CompileTemplateAsync(ProcessedTemplatePath, data);
    }

    public async Task<string> GenerateCouponIsAvailableEmail(string customerName, string couponName, byte? branchId)
    {
        var branchData = GetBranchData((Branches) branchId);

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {customerName}" },
            { "Description", $"Купон {couponName} доступний до використання" },
            { "branchData", branchData },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }

    public async Task<string> GenerateAccumulationCardIsAvailableEmail(string customerName, string accumulationCardName , byte? branchId)
    {
        var branchData = GetBranchData((Branches) branchId);

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {customerName}" },
            { "Description", $"Ваша картка {accumulationCardName} доступна до використання" },
            { "branchData", branchData },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }

    public async Task<string> GenerateSavedPromotionExpiresTomorrowEmail(string customerName, string promotionTitle, byte? branchId)
    {
        var branchData = GetBranchData((Branches) branchId);

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {customerName}" },
            { "Description", $"Ваша збережена акція \"{promotionTitle}\" закінчується завтра. Не забудьте скористатися нею!" },
            { "branchData", branchData },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }

    public async Task<string> GenerateSavedPromotionStartsTodayEmail(string customerName, string promotionTitle, byte? branchId)
    {
        var branchData = GetBranchData((Branches)branchId);

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {customerName}" },
            { "Description", $"Ваша збережена акція \"{promotionTitle}\" вже розпочалася. Скористайтеся вигодою просто зараз!" },
            { "branchData", branchData },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }

    public async Task<string> GenerateTargetEmailAsync(TargetEmailHandlebarParameters parameters)
    {
        return await CompileTemplateAsync(TargetTemplatePath, parameters);
    }

    public async Task<string> GenerateEmailVerificationEmail(string customerName, string verificationUrl, byte? branchId)
    {
        var branch = branchId is 1 or 2 ? (Branches)branchId.Value : Branches.CatJet;
        var branchData = GetBranchData(branch);
        var branchDataDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "email", branchData.Email },
            { "logoUrl", branchData.LogoUrl }
        };

        var data = new Dictionary<string, object>
        {
            { "Title", $"Вітаємо, {customerName}" },
            { "Description", "Щоб отримати бонус, перейдіть за посиланням для підтвердження вашого email." },
            { "ctaUrl", verificationUrl },
            { "helpText", "Посилання діє протягом 24 годин з моменту створення." },
            { "branchData", branchDataDict },
        };

        return await CompileTemplateAsync(SharedTemplatePath, data);
    }
}
