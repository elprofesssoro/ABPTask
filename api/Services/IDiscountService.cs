namespace api.Services;

public interface IDiscountService
{
    decimal CalculateDiscountPrice(decimal originalPrice, DateTime startTime, DateTime endTime);
}