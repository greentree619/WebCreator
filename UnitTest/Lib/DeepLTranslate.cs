using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepL;
using WebCreator;

namespace UnitTest.Lib
{
    internal class DeepLTranslate
    {
        static String authKey = Config.DeepLKey;
        Translator translator = new Translator(authKey);

        public async Task<String> Translate(String text, String language = "EN-GB") {
            var translations = await translator.TranslateTextAsync(text, null, language.ToUpper());
            return translations.Text;
        }

        public async Task<String> TranslateForQuestion(String question, String questionLang)
        {
            String orgQuestion = question;
            if ((CommonModule.questionTransMap[question] == null
                || CommonModule.questionTransMap[question].ToString().Length == 0)
                && questionLang.ToUpper().CompareTo(CommonModule.baseLanguage) != 0)
            {
                question = await Translate(question);
                CommonModule.questionTransMap[orgQuestion] = question;
            }
            else if (CommonModule.questionTransMap[question] != null
                && CommonModule.questionTransMap[question].ToString().Length > 0)
                question = CommonModule.questionTransMap[orgQuestion].ToString();

            return question;
        }
    }
}
