using Domain.Primitives;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Domain.Entities.BusinessCard
{
    public class BusinessCard : IdentityUser, IDeletable
    {

        public BusinessCard() { }

        public BusinessCard(Guid id, string name, int gender, string email, string address, string phoneNumber, DateTime dateOfBirth, string? photo)
        {
            Id = id.ToString();
            Name = name;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Photo = photo;
        }

        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }

        [MaxLength(1000000)] // Max length of 1MB
        public string? Photo { get; set; }

        public DateTime LastUpdateAt { get; set; } = DateTime.UtcNow;

        [XmlIgnore]
        public bool IsDeleted { get; private set; } = false;


        public void Delete()
        {
            IsDeleted = true;
        }

        public void UnDelete()
        {
            IsDeleted = false;
        }
    }
}
