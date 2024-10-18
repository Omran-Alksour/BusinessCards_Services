namespace Application.UseCases.BusinessCard.Responses
{
    public sealed record BusinessCardResponse(
        string Id,
        string Name,
        int Gender,
        DateTime DateOfBirth,
        string Email,
        string PhoneNumber,
        string Address,
        string Photo,
        DateTime? LastUpdateAt = null
    );

    //For XML & QR Code
    public class BusinessCard
    {
        public string Name { get; set; }
        public int Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Photo { get; set; }

        public BusinessCard()
        {
        }

        public BusinessCard(string name, int gender, string email, string phone, DateTime dateOfBirth, string address, string photo)
        {
            Name = name;
            Gender = gender;
            Email = email;
            PhoneNumber = phone;
            DateOfBirth = dateOfBirth.ToString("yyyy-MM-dd");
            Address = address;
            Photo = photo;
        }
    }


}