namespace BookLoverECommerce.Order.Domain.Entities;

public sealed class ShippingAddress
{
    private ShippingAddress() { }

    public string RecipientName { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public string AddressLine1 { get; private set; } = string.Empty;

    public string? AddressLine2 { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string Province { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public ShippingAddress(
        string recipientName,
        string phoneNumber,
        string addressLine1,
        string? addressLine2,
        string city,
        string province,
        string postalCode,
        string country)
    {
        RecipientName = RequireValue(
            recipientName,
            nameof(recipientName));

        PhoneNumber = RequireValue(
            phoneNumber,
            nameof(phoneNumber));

        AddressLine1 = RequireValue(
            addressLine1,
            nameof(addressLine1));

        AddressLine2 = string.IsNullOrWhiteSpace(addressLine2)
            ? null
            : addressLine2.Trim();

        City = RequireValue(
            city,
            nameof(city));

        Province = RequireValue(
            province,
            nameof(province));

        PostalCode = RequireValue(
            postalCode,
            nameof(postalCode));

        Country = RequireValue(
            country,
            nameof(country));
    }

    private static string RequireValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{parameterName} is required.",
                parameterName);
        }

        return value.Trim();
    }
}