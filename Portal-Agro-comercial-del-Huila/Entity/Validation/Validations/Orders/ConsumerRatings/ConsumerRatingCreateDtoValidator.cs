using Entity.DTOs.Order.ConsumerRatings.Create;
using FluentValidation;

namespace Entity.Validation.Validations.Orders.ConsumerRatings
{
    public class ConsumerRatingCreateDtoValidator : AbstractValidator<ConsumerRatingCreateDto>
    {
        public ConsumerRatingCreateDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween((byte)1, (byte)5);

            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Comment));

            RuleFor(x => x.RowVersion)
                .NotEmpty()
                .Must(IsBase64).WithMessage("RowVersion inválido.");
        }

        private static bool IsBase64(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
