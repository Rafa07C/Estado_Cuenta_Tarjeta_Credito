using System.ComponentModel.DataAnnotations;

namespace CreditCardStatement.Mvc.ViewModels;

public class AddPaymentViewModel
{
    public int CardId { get; set; } = 1;

    [Required(ErrorMessage = "La fecha del pago es requerida.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime TxDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 999999.99, ErrorMessage = "El monto del pago debe ser mayor a $0.00.")]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }
}