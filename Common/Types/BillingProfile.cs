using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Common.Types {
    public class BillingProfile {
        [Display(Name ="First Name", Order = 0)]
        public string FirstName { get; set; }
        [Display(Name = "Last Name", Order = 1)]
        public string LastName { get; set; }
        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }
        [Display(Name = "Phone", Order = 3)]
        public string Phone { get; set; }
        [Display(Name = "Address", Order = 4)]
        public string Address { get; set; }
        [Display(Name = "Address 2", Order = 5)]
        public string Address2 { get; set; }
        [Display(Name = "Address 3", Order = 6)]
        public string Address3 { get; set; }
        [Display(Name = "Zip Code", Order = 7)]
        public string ZipCode { get; set; }
        [Display(Name = "City", Order = 8)]
        public string City { get; set; }
        [Display(Name = "State", Order = 9)]
        public string State { get; set; }
        [Display(Name = "Country", Order = 10)]
        public string Country { get; set; }

        [Display(Name = "Card Type", Order = 12)]
        public string CcType { get; set; }
        [Display(Name = "Card Number", Order = 12)]
        public string CcNumber { get; set; }
        [Display(Name = "Card Expiration", Order = 13)]
        public DateTime CcExpiration { get; set; }
        [Display(Name = "CVV", Order = 14)]
        public string Cvv { get; set; }

        public BillingProfile() { }

        public BillingProfile(string first, string last, string email, string phone, string address, string address2, string address3,
            string zip, string city, string state, string country, string ccType, string ccNumber, DateTime ccExpiration, string cvv) {
            FirstName = first;
            LastName = last;
            Email = email;
            Phone = phone;
            Address = address;
            Address2 = address2;
            Address3 = address3;
            ZipCode = zip;
            City = city;
            State = state;
            Country = country;
            CcType = ccType;
            CcNumber = ccNumber;
            CcExpiration = ccExpiration;
            Cvv = cvv;
        }

        [Browsable(false), JsonIgnore]
        public string SafeCardNumber =>
            new Regex(@"\d", RegexOptions.None).Replace(CcNumber.Substring(0, CcNumber.Length - 4), "*") +
            CcNumber.Substring(CcNumber.Length - 4, CcNumber.Length - 15);

        public override string ToString() {
            return SafeCardNumber;
        }
    }
}
