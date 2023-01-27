using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class _ContactInfo
    {
        public _ContactInfo()
        {
            Brandname = "";
            StreetAddress = "";
            AdrdressLocality = "";
            AddressRegion = "";
            PostalCode = "";
            Country = "";
            Phone = "";
            Website = "";
            DescriptionOfCompany = "";
            OpeningHours = "";
        }

        [FirestoreProperty]
        public String Brandname { get; set; }//: Organic Wine Online
        [FirestoreProperty]
        public String StreetAddress { get; set; }//: 000 W Anywhere Street
        [FirestoreProperty]
        public String AdrdressLocality { get; set; }//: Houston
        [FirestoreProperty]
        public String AddressRegion { get; set; }//: Texas
        [FirestoreProperty]
        public String PostalCode { get; set; }//: 77098
        [FirestoreProperty]
        public String Country { get; set; }//: USA
        [FirestoreProperty]
        public String Phone { get; set; }//: +1(555) 555-5555
        [FirestoreProperty]
        public String Website { get; set; }//: https://organicwine.online
        [FirestoreProperty]
        public String DescriptionOfCompany { get; set; }//: Organic wine online is the best wine company online
        [FirestoreProperty]
        public String OpeningHours { get; set; }//: Mo, Tu, We, Th, Fr, Sa 08:00-20:00
    }

    [FirestoreData]
    public class Project
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String Name { get; set; }//Domain
        [FirestoreProperty]
        public String Ip { get; set; }//Ip address
        [FirestoreProperty]
        public String Keyword { get; set; }
        [FirestoreProperty]
        public int QuesionsCount { get; set; }
        [FirestoreProperty]
        public bool OnScrapping { get; set; }
        [FirestoreProperty]
        public bool OnAFScrapping { get; set; }
        [FirestoreProperty]
        public bool OnPublishSchedule { get; set; }
        [FirestoreProperty]
        public String Language { get; set; }
        [FirestoreProperty]
        public String LanguageString { get; set; }
        [FirestoreProperty]
        public _ContactInfo? ContactInfo { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
    }
}
