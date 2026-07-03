using JetFlight.Service.Extensions;
using JetFlight.Service.Jobs;
using JetFlight.Shared.Constants;
using Quartz;

namespace JetFlight.Service.Services
{
    public interface IJobSchedulerService
    {
        Task AddJobsAsync();
        Task RemoveAccumulationCardExpirationJobAsync(int accumulationCardId);
        Task RemoveCouponExpirationJobAsync(int couponId);
        Task RemoveQuestionaryExpirationJobAsync(int questionaryId);
        Task SetAccumulationExpirationJobAsync(int accumulationCardId, DateTime scheduleDate, bool checkIfExist = false);
        Task SetCouponExpirationJobAsync(int couponId, DateTime scheduleDate, bool checkIfExist = false);
        Task SetQuestionaryExpirationJobAsync(int questionaryId, DateTime scheduleDate, bool checkIfExist = false);

        Task SetSmsPopulationAsync(int smsMessageId, bool checkIfExist = false);
        Task SetSmsSenderAsync(int smsMessageId, DateTime scheduleDate, bool checkIfExist = false);

        Task SetEmailPopulationAsync(int emailMessageId, bool checkIfExist = false);
        Task SetEmailSenderAsync(int emailMessageId, DateTime scheduleDate, bool checkIfExist = false);

        Task SetCouponActiveNotificationJobAsync(int couponId, DateTime scheduleDate, bool checkIfExist = false);
        Task SetAccumulationCardActiveNotificationJobAsync(int accumulationCardId, DateTime scheduleDate, bool checkIfExist = false);
        Task SetSavedPromotionStartNotificationJobAsync(int promotionId, DateTime startedAt, bool checkIfExist = false);
        Task SetSavedPromotionDayBeforeExpirationJobAsync(int promotionId, DateTime expirationDate, bool checkIfExist = false);
        Task RemoveSavedPromotionStartNotificationJobAsync(int promotionId);
        Task RemoveSavedPromotionDayBeforeExpirationJobAsync(int promotionId);
    }

    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly ISchedulerFactory _schedulerFactory;

        private readonly IServiceProvider _serviceProvider;

        public JobSchedulerService(
        IServiceProvider serviceProvider,
        ISchedulerFactory schedulerFactory)
        {
            _serviceProvider = serviceProvider;
            _schedulerFactory = schedulerFactory;
        }

        private Task<IScheduler> GetSchedulerAsync()
        {
            return _schedulerFactory.GetScheduler();
        }

        public async Task AddJobsAsync()
        {
            var couponExpirationJob = JobBuilder.Create<CouponExpirationJob>()
                .WithIdentity(CouponExpirationJob.Key)
                .StoreDurably()
                .Build();

            var accumulationCardExpirationJob = JobBuilder.Create<AccumulationCardExpirationJob>()
                .WithIdentity(AccumulationCardExpirationJob.Key)
                .StoreDurably()
                .Build();

            var questionaryExpirationJob = JobBuilder.Create<QuestionaryExpirationJob>()
                .WithIdentity(QuestionaryExpirationJob.Key)
                .StoreDurably()
                .Build();

            var loyaltyExpirationJob = JobBuilder.Create<LoyaltyExpirationJob>()
                .WithIdentity(LoyaltyExpirationJob.Key)
                .StoreDurably()
                .Build();

            var loyaltyExpirationJobTrigger = TriggerBuilder.Create()
                .WithIdentity("LoyaltyExpirationTrigger", JobConstants.LoyaltyGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(JobConstants.LoyaltyExpirationJobIntervalHours)
                    .RepeatForever())
                .ForJob(loyaltyExpirationJob.Key)
                .Build();

            var smsMessageSenderJob = JobBuilder.Create<SmsMessageSenderJob>()
                .WithIdentity(SmsMessageSenderJob.Key)
                .StoreDurably()
                .Build();

            var smsPopulationJob = JobBuilder.Create<SmsPopulationJob>()
                .WithIdentity(SmsPopulationJob.Key)
                .StoreDurably()
                .Build();

            var emailMessageSenderJob = JobBuilder.Create<EmailMessageSenderJob>()
                .WithIdentity(EmailMessageSenderJob.Key)
                .StoreDurably()
                .Build();

            var emailPopulationJob = JobBuilder.Create<EmailPopulationJob>()
                .WithIdentity(EmailPopulationJob.Key)
                .StoreDurably()
                .Build();

            var targetNotificationReshedulerJob = JobBuilder.Create<TargetNotificationReshedulerJob>()
                .WithIdentity(TargetNotificationReshedulerJob.Key)
                .StoreDurably()
                .Build();

            var targetNotificationReshedulerTrigger = TriggerBuilder.Create()
                .WithIdentity("TargetNotificationReshedulerTrigger", JobConstants.TargetNotificationGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(1)
                    .RepeatForever())
                .ForJob(targetNotificationReshedulerJob.Key)
                .Build();

            var accumulationCardActiveNotificationJob = JobBuilder.Create<AccumulationCardActiveNotificationJob>()
                .WithIdentity(AccumulationCardActiveNotificationJob.Key)
                .StoreDurably()
                .Build();

            var couponActiveNotificationJob = JobBuilder.Create<CouponActiveNotificationJob>()
                .WithIdentity(CouponActiveNotificationJob.Key)
                .StoreDurably()
                .Build();

            var savedPromotionDayBeforeExpirationJob = JobBuilder.Create<SavedPromotionDayBeforeExpirationJob>()
                .WithIdentity(SavedPromotionDayBeforeExpirationJob.Key)
                .StoreDurably()
                .Build();

            var savedPromotionStartNotificationJob = JobBuilder.Create<SavedPromotionStartNotificationJob>()
                .WithIdentity(SavedPromotionStartNotificationJob.Key)
                .StoreDurably()
                .Build();

            var loyaltyActiveNotificationJob = JobBuilder.Create<LoyaltyActiveNotificationJob>()
                .WithIdentity(LoyaltyActiveNotificationJob.Key)
                .StoreDurably()
                .Build();

            var loyaltyActiveNotificationJobTrigger = TriggerBuilder.Create()
                .WithIdentity("LoyaltyActiveNotificationJobTrigger", JobConstants.LoyaltyGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(30)
                    .RepeatForever())
                .ForJob(loyaltyActiveNotificationJob.Key)
                .Build();

            var savedPromotionStatusChangedNotificationJob = JobBuilder.Create<SavedPromotionStatusChangedNotificationJob>()
                .WithIdentity(SavedPromotionStatusChangedNotificationJob.Key)
                .StoreDurably()
                .Build();

            var savedPromotionRefreshJob = JobBuilder.Create<SavedPromotionRefreshJob>()
                .WithIdentity(SavedPromotionRefreshJob.Key)
                .StoreDurably()
                .Build();

            var rfmSnapshotJob = JobBuilder.Create<RfmSnapshotJob>()
                .WithIdentity(RfmSnapshotJob.Key)
                .StoreDurably()
                .Build();

            var pageAutoPublishJob = JobBuilder.Create<PageAutoPublishJob>()
                .WithIdentity(PageAutoPublishJob.Key)
                .StoreDurably()
                .Build();

            var analyticsTimeZone = TimeZoneConstants.ResolveUATimeZone();

            var savedPromotionStatusChangedNotificationJobTrigger = TriggerBuilder.Create()
                .WithIdentity("SavedPromotionStatusChangedNotificationJobTrigger", JobConstants.LoyaltyGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(2)
                    .RepeatForever())
                .ForJob(savedPromotionStatusChangedNotificationJob.Key)
                .Build();

            var rfmSnapshotTrigger = TriggerBuilder.Create()
                .WithIdentity("RfmSnapshotTrigger", JobConstants.AnalyticsGroup)
                .WithSchedule(CronScheduleBuilder
                    .DailyAtHourAndMinute(JobConstants.RfmSnapshotJobHour, JobConstants.RfmSnapshotJobMinute)
                    .InTimeZone(analyticsTimeZone))
                .ForJob(rfmSnapshotJob.Key)
                .Build();

            var savedPromotionRefreshTrigger = TriggerBuilder.Create()
                .WithIdentity("SavedPromotionRefreshTrigger", JobConstants.LoyaltyGroup)
                .WithSchedule(CronScheduleBuilder
                    .DailyAtHourAndMinute(3, 0)
                    .InTimeZone(analyticsTimeZone))
                .ForJob(savedPromotionRefreshJob.Key)
                .Build();

            var pageAutoPublishTrigger = TriggerBuilder.Create()
                .WithIdentity("PageAutoPublishTrigger", JobConstants.ContentGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(JobConstants.PageAutoPublishJobIntervalMinutes)
                    .RepeatForever())
                .ForJob(pageAutoPublishJob.Key)
                .Build();


            IScheduler scheduler = await GetSchedulerAsync();
            scheduler.Context.Put("IServiceProvider", _serviceProvider);

            // Add or replace jobs (replaceExisting: true allows safe re-deployment)
            await scheduler.AddJob(couponExpirationJob, true);
            await scheduler.AddJob(accumulationCardExpirationJob, true);
            await scheduler.AddJob(questionaryExpirationJob, true);
            await scheduler.AddJob(loyaltyExpirationJob, true);
            
            // Schedule triggers, replacing if they already exist
            if (await scheduler.CheckExists(loyaltyExpirationJobTrigger.Key))
            {
                await scheduler.RescheduleJob(loyaltyExpirationJobTrigger.Key, loyaltyExpirationJobTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(loyaltyExpirationJobTrigger);
            }

            await scheduler.AddJob(smsMessageSenderJob, true);
            await scheduler.AddJob(smsPopulationJob, true);

            await scheduler.AddJob(emailMessageSenderJob, true);
            await scheduler.AddJob(emailPopulationJob, true);

            await scheduler.AddJob(targetNotificationReshedulerJob, true);
            if (await scheduler.CheckExists(targetNotificationReshedulerTrigger.Key))
            {
                await scheduler.RescheduleJob(targetNotificationReshedulerTrigger.Key, targetNotificationReshedulerTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(targetNotificationReshedulerTrigger);
            }

            await scheduler.AddJob(accumulationCardActiveNotificationJob, true);
            await scheduler.AddJob(couponActiveNotificationJob, true);
            await scheduler.AddJob(savedPromotionStartNotificationJob, true);
            await scheduler.AddJob(savedPromotionDayBeforeExpirationJob, true);
            await scheduler.AddJob(loyaltyActiveNotificationJob, true);
            if (await scheduler.CheckExists(loyaltyActiveNotificationJobTrigger.Key))
            {
                await scheduler.RescheduleJob(loyaltyActiveNotificationJobTrigger.Key, loyaltyActiveNotificationJobTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(loyaltyActiveNotificationJobTrigger);
            }

            await scheduler.AddJob(savedPromotionStatusChangedNotificationJob, true);
            if (await scheduler.CheckExists(savedPromotionStatusChangedNotificationJobTrigger.Key))
            {
                await scheduler.RescheduleJob(savedPromotionStatusChangedNotificationJobTrigger.Key, savedPromotionStatusChangedNotificationJobTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(savedPromotionStatusChangedNotificationJobTrigger);
            }

            await scheduler.AddJob(savedPromotionRefreshJob, true);
            if (await scheduler.CheckExists(savedPromotionRefreshTrigger.Key))
            {
                await scheduler.RescheduleJob(savedPromotionRefreshTrigger.Key, savedPromotionRefreshTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(savedPromotionRefreshTrigger);
            }

            await scheduler.AddJob(rfmSnapshotJob, true);
            if (await scheduler.CheckExists(rfmSnapshotTrigger.Key))
            {
                await scheduler.RescheduleJob(rfmSnapshotTrigger.Key, rfmSnapshotTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(rfmSnapshotTrigger);
            }

            await scheduler.AddJob(pageAutoPublishJob, true);
            if (await scheduler.CheckExists(pageAutoPublishTrigger.Key))
            {
                await scheduler.RescheduleJob(pageAutoPublishTrigger.Key, pageAutoPublishTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(pageAutoPublishTrigger);
            }

            // Don't start scheduler here - AddQuartzServer handles it automatically
            // Only start if not already started
            if (!scheduler.IsStarted)
            {
                await scheduler.Start();
            }
        }

        private static TriggerKey GetCouponExpirationTriggerKey(int couponId)
            => new TriggerKey($"{nameof(CouponExpirationJob)}_{couponId}", JobConstants.LoyaltyGroup);

        public async Task SetCouponExpirationJobAsync(int couponId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetCouponExpirationTriggerKey(couponId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData(nameof(CouponExpirationJob.CouponId), couponId)
                .StartAt(scheduleDate)
                .ForJob(CouponExpirationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        public async Task RemoveCouponExpirationJobAsync(int couponId)
        {
            var key = GetCouponExpirationTriggerKey(couponId);

            var scheduler = await GetSchedulerAsync();

            if (await scheduler.CheckExists(key))
            {
                await scheduler.UnscheduleJob(key);
            }
        }

        private static TriggerKey GetAccumulationCardExpirationTriggerKey(int accumulationCardId)
            => new TriggerKey($"{nameof(AccumulationCardExpirationJob)}_{accumulationCardId}", JobConstants.LoyaltyGroup);

        public async Task SetAccumulationExpirationJobAsync(int accumulationCardId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetCouponExpirationTriggerKey(accumulationCardId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData(nameof(AccumulationCardExpirationJob.AccumulationCardId), accumulationCardId)
                .StartAt(scheduleDate)
                .ForJob(AccumulationCardExpirationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        public async Task RemoveAccumulationCardExpirationJobAsync(int accumulationCardId)
        {
            var key = GetAccumulationCardExpirationTriggerKey(accumulationCardId);

            var scheduler = await GetSchedulerAsync();

            if (await scheduler.CheckExists(key))
            {
                await scheduler.UnscheduleJob(key);
            }
        }

        private static TriggerKey GetQuestionaryExpirationTriggerKey(int questionaryId)
            => new TriggerKey($"{nameof(QuestionaryExpirationJob)}_{questionaryId}", JobConstants.LoyaltyGroup);

        public async Task SetQuestionaryExpirationJobAsync(int questionaryId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetQuestionaryExpirationTriggerKey(questionaryId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData(nameof(QuestionaryExpirationJob.QuestionaryId), questionaryId)
                .StartAt(scheduleDate)
                .ForJob(QuestionaryExpirationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        public async Task RemoveQuestionaryExpirationJobAsync(int questionaryId)
        {
            var key = GetQuestionaryExpirationTriggerKey(questionaryId);

            var scheduler = await GetSchedulerAsync();

            if (await scheduler.CheckExists(key))
            {
                await scheduler.UnscheduleJob(key);
            }
        }

        private static TriggerKey GetSmsMessageSenderTriggerKey(int smsMessageId)
            => new TriggerKey($"{nameof(SmsMessageSenderJob)}_{smsMessageId}", JobConstants.TargetNotificationGroup);

        public async Task SetSmsSenderAsync(int smsMessageId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetSmsMessageSenderTriggerKey(smsMessageId);

            var scheduler = await GetSchedulerAsync();
            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData("SmsMessageId", smsMessageId)
                .StartAt(scheduleDate)
                .ForJob(SmsMessageSenderJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        private static TriggerKey GetSmsPopulationTriggerKey(int smsMessageId)
            => new TriggerKey($"{nameof(SmsPopulationJob)}_{smsMessageId}", JobConstants.TargetNotificationGroup);


        public async Task SetSmsPopulationAsync(int smsMessageId, bool checkIfExist = false)
        {
            var key = GetSmsPopulationTriggerKey(smsMessageId);

            var scheduler = await GetSchedulerAsync();
            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData("SmsMessageId", smsMessageId)
                .StartAt(DateTime.UtcNow)
                .ForJob(SmsPopulationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        private static TriggerKey GetEmailPopulationTriggerKey(int emailMessageId)
            => new TriggerKey($"{nameof(EmailPopulationJob)}_{emailMessageId}", JobConstants.TargetNotificationGroup);

        public async Task SetEmailPopulationAsync(int emailMessageId, bool checkIfExist = false)
        {
            var key = GetEmailPopulationTriggerKey(emailMessageId);

            var scheduler = await GetSchedulerAsync();
            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
            .WithIdentity(key)
                .UsingJobData("EmailMessageId", emailMessageId)
                .StartAt(DateTime.UtcNow)
                .ForJob(EmailPopulationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        private static TriggerKey GetEmailMessageSenderTriggerKey(int emailMessageId)
            => new TriggerKey($"{nameof(EmailMessageSenderJob)}_{emailMessageId}", JobConstants.TargetNotificationGroup);

        public async Task SetEmailSenderAsync(int emailMessageId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetEmailMessageSenderTriggerKey(emailMessageId);

            var scheduler = await GetSchedulerAsync();
            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData("EmailMessageId", emailMessageId)
                .StartAt(scheduleDate)
                .ForJob(EmailMessageSenderJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        private static TriggerKey GetCouponActiveNotificationTriggerKey(int couponId)
            => new TriggerKey($"{nameof(CouponActiveNotificationJob)}_{couponId}", JobConstants.LoyaltyGroup);

        public async Task SetCouponActiveNotificationJobAsync(int couponId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetCouponActiveNotificationTriggerKey(couponId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
            .WithIdentity(key)
                .UsingJobData(nameof(CouponActiveNotificationJob.CouponId), couponId)
                .StartAt(scheduleDate)
                .ForJob(CouponActiveNotificationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        private static TriggerKey GetAccumulationCardActiveNotificationTriggerKey(int accumulationCardId)
            => new TriggerKey($"{nameof(AccumulationCardActiveNotificationJob)}_{accumulationCardId}", JobConstants.LoyaltyGroup);


        public async Task SetAccumulationCardActiveNotificationJobAsync(int accumulationCardId, DateTime scheduleDate, bool checkIfExist = false)
        {
            var key = GetAccumulationCardActiveNotificationTriggerKey(accumulationCardId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
            .WithIdentity(key)
                .UsingJobData(nameof(AccumulationCardActiveNotificationJob.AccumulationCardId), accumulationCardId)
                .StartAt(scheduleDate)
                .ForJob(AccumulationCardActiveNotificationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        private static TriggerKey GetSavedPromotionDayBeforeExpirationTriggerKey(int promotionId)
            => new TriggerKey($"{nameof(SavedPromotionDayBeforeExpirationJob)}_{promotionId}", JobConstants.LoyaltyGroup);

        private static TriggerKey GetSavedPromotionStartNotificationTriggerKey(int promotionId)
            => new TriggerKey($"{nameof(SavedPromotionStartNotificationJob)}_{promotionId}", JobConstants.LoyaltyGroup);

        public async Task SetSavedPromotionStartNotificationJobAsync(int promotionId, DateTime startedAt, bool checkIfExist = false)
        {
            var key = GetSavedPromotionStartNotificationTriggerKey(promotionId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            // Ignore time from database, use only date
            var startedAtDateOnly = startedAt.Date;
            var startedAtLocal = startedAtDateOnly.FromUtcToTimezone(TimeZoneConstants.UATimezone);
            var notificationLocal = new DateTime(
                startedAtLocal.Year,
                startedAtLocal.Month,
                startedAtLocal.Day,
                8, 0, 0,
                DateTimeKind.Unspecified);
            var notificationUtc = notificationLocal.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);

            if (notificationUtc <= DateTime.UtcNow)
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData("PromotionId", promotionId)
                .StartAt(notificationUtc)
                .ForJob(SavedPromotionStartNotificationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        public async Task SetSavedPromotionDayBeforeExpirationJobAsync(int promotionId, DateTime expirationDate, bool checkIfExist = false)
        {
            var key = GetSavedPromotionDayBeforeExpirationTriggerKey(promotionId);

            var scheduler = await GetSchedulerAsync();

            if (checkIfExist && await scheduler.CheckExists(key))
            {
                return;
            }

            // Ignore time from database, use only date
            var expirationDateOnly = expirationDate.Date;
            var expirationLocal = expirationDateOnly.FromUtcToTimezone(TimeZoneConstants.UATimezone);
            
            // Always schedule notification for 15:00 on expiration day (6 hours before expiration at 21:00)
            var notificationLocal = new DateTime(
                expirationLocal.Year,
                expirationLocal.Month,
                expirationLocal.Day,
                15, 0, 0,
                DateTimeKind.Unspecified);
            
            var notificationTime = notificationLocal.FromTimeZoneToUtc(TimeZoneConstants.UATimezone);

            // Only schedule if notification time is in the future and before expiration
            if (notificationTime <= DateTime.UtcNow || notificationTime >= expirationDate)
            {
                return;
            }

            var trigger = TriggerBuilder.Create()
                .WithIdentity(key)
                .UsingJobData("PromotionId", promotionId)
                .StartAt(notificationTime)
                .ForJob(SavedPromotionDayBeforeExpirationJob.Key)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }

        public async Task RemoveSavedPromotionStartNotificationJobAsync(int promotionId)
        {
            var key = GetSavedPromotionStartNotificationTriggerKey(promotionId);

            var scheduler = await GetSchedulerAsync();

            if (await scheduler.CheckExists(key))
            {
                await scheduler.UnscheduleJob(key);
            }
        }

        public async Task RemoveSavedPromotionDayBeforeExpirationJobAsync(int promotionId)
        {
            var key = GetSavedPromotionDayBeforeExpirationTriggerKey(promotionId);

            var scheduler = await GetSchedulerAsync();

            if (await scheduler.CheckExists(key))
            {
                await scheduler.UnscheduleJob(key);
            }
        }
    }
}
