using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class _ContactInfo
    {
        public _ContactInfo()
        {
            Brandname = "";
            UseTitleByBrandname = false;
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
        public bool UseTitleByBrandname { get; set; }//: Organic Wine Online
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
    public class _ImageAutoGenInfo
    {
        public enum ImageScrapingFrom
        {
            Pixabay,
            OpenAI,
            Pixabay_OpenAI,
            None = -1
        }

        public _ImageAutoGenInfo()
        {
            ScrappingFrom = ImageScrapingFrom.Pixabay_OpenAI;
            ImageNumber = 1;
            InsteadOfTitle = "";
        }

        [FirestoreProperty]
        public ImageScrapingFrom ScrappingFrom { get; set; }
        [FirestoreProperty]
        public Int32 ImageNumber { get; set; }
        [FirestoreProperty]
        public String InsteadOfTitle { get; set; }
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
        public bool? UseHttps { get; set; }//https enabled
        [FirestoreProperty]
        public String? S3BucketName { get; set; }//If Ip is 0.0.0.0, use to host from AWS S3. So, keep S3BucketDomain in this case.
                                                //for example: {domain}.s3-website.{region}.amazonaws.com
        [FirestoreProperty]
        public String? S3BucketRegion { get; set; }//If Ip is 0.0.0.0, use to host from AWS S3. So, keep S3BucketDomain in this case.
                                                //for example: {domain}.s3-website.{region}.amazonaws.com
        [FirestoreProperty]
        public String Keyword { get; set; }
        [FirestoreProperty]
        public int QuesionsCount { get; set; }
        [FirestoreProperty]
        public bool OnScrapping { get; set; }
        [FirestoreProperty]
        public bool OnAFScrapping { get; set; }
        [FirestoreProperty]
        public bool OnOpenAIScrapping { get; set; }
        [FirestoreProperty]
        public bool OnPublishSchedule { get; set; }
        [FirestoreProperty]
        public String Language { get; set; }
        [FirestoreProperty]
        public String LanguageString { get; set; }
        [FirestoreProperty]
        public _ContactInfo? ContactInfo { get; set; }
        [FirestoreProperty]
        public _ImageAutoGenInfo? ImageAutoGenInfo { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
    }
}
