namespace AddressService.Dto
{
    public class AddressDto
    {
        public int AddressId { get; set; }

        public int CustomerId { get; set; }

        public string? HouseNumber { get; set; }

        public string? Street { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Country { get; set; }

        public string? PinCode { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
