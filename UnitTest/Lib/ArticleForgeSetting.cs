using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using WebCreator;
using WebCreator.Models;

namespace UnitTest.Lib
{
    internal class ArticleForgeSetting
    {
        public AFSetting setInf = new AFSetting();

        public ArticleForgeSetting()
        {
            setInf.SentenceVariation = SentenceVariationType.Sentence1;
            setInf.ParagraphVariation = ParagraphVariationType.Paragraph1;
            setInf.ShuffleParagraphs = ShuffleParagraphsType.Disable;
            setInf.Length = LengthType.Short;
            setInf.Title = TitleType.Diable;
            setInf.Image = 0.00f;
            setInf.Video = 0.00f;
            setInf.Quality = QualityType.Readable;

            Task.Run(() => initAFSettingFromFirebase());
        }

        public async Task initAFSettingFromFirebase() 
        {
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("AFSetting");
                Query query = col.OrderByDescending("CreatedTime").Limit(1);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count > 0)
                {
                    DocumentSnapshot snapShot = snapshot.Documents.ElementAt(0);
                    setInf = snapShot.ConvertTo<AFSetting>();
                    setInf.Id = snapShot.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task SaveSettingAsync(AFSetting setInf)
        {
            try
            {
                this.setInf = setInf;
                CollectionReference col = Config.FirebaseDB.Collection("AFSetting");
                Query query = col.OrderByDescending("CreatedTime").Limit( 1 );
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count <= 0)
                {
                    setInf.UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    setInf.CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    await col.AddAsync(setInf);
                }
                else
                {
                    DocumentSnapshot snapShot = snapshot.Documents.ElementAt(0);
                    Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                    {
                        { "SentenceVariation", setInf.SentenceVariation},
                        { "ParagraphVariation", setInf.ParagraphVariation},
                        { "ShuffleParagraphs", setInf.ShuffleParagraphs},
                        { "Length", setInf.Length},
                        { "Title", setInf.Title},
                        { "Image", setInf.Image},
                        { "Video", setInf.Video},
                        { "Quality", setInf.Quality},
                        { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)}
                    };
                    await snapShot.Reference.UpdateAsync(userUpdate);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<AFSetting> GetSettingAsync()
        {
            await initAFSettingFromFirebase();
            return setInf;
        }

        public String GetLengthString()
        {
            switch (setInf.Length)
            {
                case LengthType.VeryShort:
                    return "very_short";
                case LengthType.Short:
                    return "short";
                case LengthType.Medium:
                    return "medium";
                case LengthType.Long:
                    return "long";
            }
            return "short";
        }
    }
}