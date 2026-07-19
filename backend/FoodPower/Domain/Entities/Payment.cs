using System;
using System.Collections.Generic;
using FoodPower.Domain.Enums;

namespace FoodPower.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int SubmittedById { get; set; }
    public decimal TotalAmount { get; set; }
    public string ScreenshotPath { get; set; } = string.Empty;
    public string? Note { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Bkash;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public int? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? SubmittedBy { get; set; }
    public AppUser? ReviewedBy { get; set; }
    public List<PaymentAllocation> Allocations { get; set; } = [];

    public Payment()
    {
    }

    public Payment(int submittedById, decimal totalAmount, string screenshotPath, string? note, PaymentMethod method)
    {
        SubmittedById = submittedById;
        TotalAmount = totalAmount;
        ScreenshotPath = screenshotPath;
        Note = note;
        Method = method;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
}
