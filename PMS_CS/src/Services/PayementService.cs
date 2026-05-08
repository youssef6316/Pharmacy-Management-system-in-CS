using PMS_CS.src.Models;
using PMS_CS.src.Repositories;

namespace PMS_CS.src.Services;

public class PaymentService
{
    private readonly PaymentRepository _paymentRepo;
    private readonly OrderRepository   _orderRepo;

    public PaymentService()
    {
        _paymentRepo = new PaymentRepository();
        _orderRepo   = new OrderRepository();
    }

    // ── PROCESS PAYMENT ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a payment record for a completed order.
    /// The order must be completed before payment is processed.
    /// </summary>
    public (int PaymentId, string Error) ProcessPayment(
        int orderId, string paymentMethod)
    {
        var order = _orderRepo.GetOrderById(orderId);

        if (order == null)
            return (-1, "Order not found.");

        if (!order.IsPending())
            return (-1, "Payment can only be processed for pending orders.");

        // Guard against double-payment.
        var existing = _paymentRepo.GetPaymentByOrder(orderId);
        if (existing != null)
            return (-1, "A payment already exists for this order.");

        var payment = new Payment(
            amount:        order.TotalPrice,
            paymentDate:   DateTime.Today.ToString("yyyy-MM-dd"),
            paymentMethod: paymentMethod,
            orderId:       orderId
        );

        int newId = _paymentRepo.AddPayment(payment);

        if (newId == -1)
            return (-1, "Failed to save payment record.");

        // Mark it completed immediately on successful save.
        _paymentRepo.UpdateStatus(newId, "Completed");
        _orderRepo.UpdateStatus(orderId, "Completed");

        return (newId, string.Empty);
    }

    // ── REPORTING ─────────────────────────────────────────────────────────
    // Replaces Admin.totalIncome static field — now a live DB query.

    public double GetTotalIncome() => _paymentRepo.GetTotalIncome();

    public Payment?      GetPaymentById(int id)    => _paymentRepo.GetPaymentById(id);
    public Payment?      GetPaymentByOrder(int id) => _paymentRepo.GetPaymentByOrder(id);
}