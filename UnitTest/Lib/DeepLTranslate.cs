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

        public async Task<String> Translate(String text, String languate= "EN-GB") {
            var translations = await translator.TranslateTextAsync(text, null, languate.ToUpper());
            return translations.Text;
        }
    }
}
