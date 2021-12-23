using System;
using BitFleet.Contracts.V1.Requests.Queries;
using FluentValidation;

namespace BitFleet.Validators
{
    public class GetAllRidesQueryValidator : AbstractValidator<GetAllRidesQuery>
    {
        public GetAllRidesQueryValidator()
        {
            RuleFor(x=> x.UserId)
                .Must(userId => Guid.TryParse(userId, out var result) || string.IsNullOrEmpty(userId));
        }   
    }
}