using System.Collections.Generic;
using System.Linq;

namespace FoodPower.Features.Auth.Services.Emails;

public static class EmailTemplates
{
    public static string OtpEmailSubject(string purpose) =>
        $"FoodPower - {purpose} verification code";

    public static string OtpEmailBody(string fullName, string code, string purpose, int lifetimeMinutes = 10) =>
        $"""
         <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
             <h2 style="color: #16a34a;">FoodPower</h2>
             <p>Hi {fullName},</p>
             <p>Your {purpose.ToLowerInvariant()} verification code is:</p>
             <p style="font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #111827;">{code}</p>
             <p>This code expires in {lifetimeMinutes} minutes.</p>
             <p>If you did not request this, you can safely ignore this email.</p>
             <p style="color: #6b7280; font-size: 12px;">FoodPower - office lunch management</p>
         </div>
         """;

    public static string GeneralPollPublishedSubject(string question) =>
        $"নতুন পোল — New poll: {question}";

    public static string GeneralPollPublishedBody(
        string question,
        IReadOnlyCollection<string> optionNames,
        string cutoffLocal,
        string pollUrl)
    {
        var optionsHtml = string.Join(
            string.Empty,
            optionNames.Select(name =>
                $"""<li style="margin: 4px 0; color: #111827;">{name}</li>"""));

        return
            $"""
             <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                 <h2 style="color: #16a34a;">FoodPower</h2>
                 <p>নতুন একটি পোল প্রকাশিত হয়েছে। এখনই ভোট দিন!</p>
                 <p>A new poll is live: <strong>{question}</strong>. Cast your vote now!</p>
                 <ul style="padding-left: 20px;">{optionsHtml}</ul>
                 <p><strong>{cutoffLocal}</strong> এর আগে ভোট দিন — Vote before <strong>{cutoffLocal}</strong>.</p>
                 <p style="margin: 24px 0;">
                     <a href="{pollUrl}"
                        style="background-color: #16a34a; color: #ffffff; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold;">
                         ভোট দিন / Vote now
                     </a>
                 </p>
                 <p style="color: #6b7280; font-size: 12px;">FoodPower - office lunch management</p>
             </div>
             """;
    }

    public static string PollPublishedSubject(string lunchDate) =>
        $"আজকের লাঞ্চ পোল — Vote for lunch ({lunchDate})";

    public static string PollPublishedBody(
        string lunchDate,
        IReadOnlyCollection<string> optionNames,
        string cutoffLocal,
        string pollUrl,
        string? bkashNumber,
        string? bankAccount)
    {
        var optionsHtml = string.Join(
            string.Empty,
            optionNames.Select(name =>
                $"""<li style="margin: 4px 0; color: #111827;">{name}</li>"""));

        var bkashHtml = string.IsNullOrWhiteSpace(bkashNumber)
            ? string.Empty
            : $"""<p style="margin: 4px 0;">bKash (Send Money, include cash-out charge): <strong>{bkashNumber}</strong></p>""";

        var bankHtml = string.IsNullOrWhiteSpace(bankAccount)
            ? string.Empty
            : $"""<p style="margin: 4px 0;">Bank account: <strong>{bankAccount}</strong></p>""";

        return
            $"""
             <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
                 <h2 style="color: #16a34a;">FoodPower</h2>
                 <p>আজকের লাঞ্চ পোল প্রকাশিত হয়েছে। এখনই ভোট দিন!</p>
                 <p>The lunch poll for <strong>{lunchDate}</strong> is live. Cast your vote now!</p>
                 <ul style="padding-left: 20px;">{optionsHtml}</ul>
                 <p><strong>{cutoffLocal}</strong> এর আগে ভোট দিন — Vote before <strong>{cutoffLocal}</strong>.</p>
                 <p style="margin: 24px 0;">
                     <a href="{pollUrl}"
                        style="background-color: #16a34a; color: #ffffff; padding: 12px 24px; border-radius: 8px; text-decoration: none; font-weight: bold;">
                         ভোট দিন / Vote now
                     </a>
                 </p>
                 <div style="border-top: 1px solid #e5e7eb; margin-top: 24px; padding-top: 16px; color: #374151; font-size: 14px;">
                     <p style="margin: 4px 0;"><strong>সাপ্তাহিক পেমেন্ট সম্পন্ন করুন / Please complete your weekly payment.</strong></p>
                     {bkashHtml}
                     {bankHtml}
                 </div>
                 <p style="color: #6b7280; font-size: 12px;">FoodPower - office lunch management</p>
             </div>
             """;
    }
}
