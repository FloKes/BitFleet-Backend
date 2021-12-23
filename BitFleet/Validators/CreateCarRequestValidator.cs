using System;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using FluentValidation;

namespace BitFleet.Validators
{
    public class CreateCarRequestValidator : AbstractValidator<CreateCarRequest>
    {
        public CreateCarRequestValidator()
        {
            RuleFor(x => x.FuelType)
                .NotEmpty()
                .Must(fuelType =>
                    string.Equals(fuelType, "diesel", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(fuelType, "gasoline", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(fuelType, "electricity", StringComparison.InvariantCultureIgnoreCase));

            RuleFor(x => x.MileageWhenBought)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.KilometersNeededBeforeService)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.KilometersPerLiter)
                .GreaterThanOrEqualTo(0);
        }
    }
}