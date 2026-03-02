using System.ComponentModel.DataAnnotations;

namespace CreditCardStatement.Mvc.ViewModels;

public class AddPurchaseViewModel
{
    public int CardId { get; set; } = 1;

    [Required(ErrorMessage = "La fecha de la compra es requerida.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime TxDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "La descripción es requerida.")]
    [MaxLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres.")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a $0.00.")]
    [Display(Name = "Monto")]
    public decimal Amount { get; set; }
}