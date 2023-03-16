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

namespace UnitTest.Lib
{
    internal class ArticleForge
    {
        // API Url
        private String apiUrl = "https://af.articleforge.com";

        // API key
        private String key = Config.ArticleForgeKey;

        // String error
        private String error_message = "";

        /**
         * Checks the response status, if the error, returns FALSE and writes the error string into a variable
         *
         * @param array server response
         * @return bool
         */
        private bool isStatusSuccess(JObject data)
        {
            bool valid = data["status"] != null && data["status"].ToString() == "Success";
            if (!valid)
            {
                String error_message = data["error_message"] != null ? data["error_message"].ToString() : "Unknown error";
                setErrorMessage(error_message);
            }
            return valid;
        }

        /**
         * Setter $error_message
         *
         * @param string $error
         */
        private void setErrorMessage(String error)
        {
            error_message = error;
        }

        //		/**
        //		 * Returns the error
        //		 *
        //		 * <code>
        //		 * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
        //		 * $error = $af->getLastError();
        //		 * </code>
        //		 *
        //		 * @return string last error
        //		 */
        //		public function getLastError()
        //		{
        //			return $this->error_message;
        //		}

        //		/**
        //		 * Return array with information the usage of your account
        //		 *
        //		 * <code>
        //		 * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
        //		 * $account = $af->checkUsage();
        //		 * </code>
        //		 *
        //		 * Example output
        //		 * [view_articlesview_articles
        //		 *  'status' => 'Success',
        //		 *  'API Requests' => 0,
        //		 *  'Words remaining' => 200000,
        //		 *  'Overuse Protection' => 'NO',
        //		 *  'Prepaid Amount' => '$20.00'
        //		 * ];
        //		 *
        //		 * @return array
        //		 */
        //		public function checkUsage()
        //		{
        //		$result = $this->execute('check_usage');
        //			if ($this->isStatusSuccess($result)) {
        //				return $result;
        //			}

        //			return false;
        //		}

        /**
         * Returns in array format all articles in descending order. You can provide an optional parameter limit to limit the number of the returned results.
         *
         * <code>
         * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
         * $articles = $af->viewArticles();
         * </code>
         *
         * Example output
         * [
         *  0 => [
         *	 'id' => 1,
         *   'title' => 'Title1',
         *   'created_at' => '2015-08-23T14:36:17.000Z',
         *   'spintax' => 'Your Article Spintax',
         *   'keyword' => 'Your Keyword',
         *   'sub_keywords': '',
         *   'quality' => 'custom'
         *  ],
         * ...
         * ];
         *
         * @return array with articles
         */
        public JArray viewArticles()
        {
            JArray articles = null;
            JObject result = execute("view_articles", null);
            if (isStatusSuccess(result) && result["data"] != null)
            {
                articles = result["data"].ToObject<JArray>();
            }
            return articles;
        }

        //		/**
        //		 * Returns the spintax for the article with article_id
        //		 *
        //		 * <code>
        //		 * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
        //		 * $spintax = $af->viewSpintax(5);
        //		 * </code>
        //		 *
        //		 * @param int $article_id Article ID
        //		 * @return string article spintax
        //		 */
        //		public function viewSpintax($article_id)
        //		{
        //		$article_id = (int) $article_id;
        //			if ($article_id == 0) {
        //			$this->setErrorMessage('Parameter "article_id" is required');
        //				return false;
        //			}

        //		$result = $this->execute('view_articles', ['article_id' => $article_id]);
        //			if ($this->isStatusSuccess($result)) {
        //				return isset($result['data']) ? $result['data'] : '';
        //			}

        //			return false;
        //		}

        //		/**
        //		 * Return a spin for the article with article_id
        //		 *
        //		 * <code>
        //		 * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
        //		 * $spin = $af->viewSpin(5);
        //		 * </code>
        //		 *
        //		 * @param int $article_id Article ID
        //		 * @return string article spin
        //		 */
        //		public function viewSpin($article_id)
        //		{
        //		$article_id = (int) $article_id;
        //			if ($article_id == 0) {
        //			$this->setErrorMessage('Parameter "article_id" is required');
        //				return false;
        //			}

        //		$result = $this->execute('view_spin', ['article_id' => $article_id]);
        //			if ($this->isStatusSuccess($result)) {
        //				return isset($result['data']) ? $result['data'] : '';
        //			}

        //			return false;
        //		}

        /**
         * Creates an article with keyword 'keyword' with given settings. Returns the spintax for the article. This API call has usage restrictions - 
         * to view the specific limitations for your account visit https://af.articleforge.com/api_info
         *
         * <code>
         *  $params = [
         *   'keyword' => 'Starwars',
         *   'sub_keywords' => 'Master Yoda, Darth Vader'
         *  ];
         *  
         *	$article = $af->createArticle($params);
         * </code>
         *
         * @param array $params 
         *	keyword: Word for generation article
         *	sub_keywords: a list of sub-keywords separated by comma (e.g. subkeyword1,subkeyword2,subkeyword3).  
         *  sentence_variation:	number of sentence variations. It can be either 1, 2, or 3. The default value is 1.
         *  paragraph_variation: number of paragraph variations. It can be either 1, 2, or 3. The default value is 1.
         *  shuffle_paragraphs: enable shuffle paragraphs or not. It can be either 0(disabled) or 1(enabled). The default value is 0.
         *  length: the length of the article. It can be either ‘very_short’(approximately 50 words), ‘short’(approximately 200 words), 
         *  		‘medium’(approximately 500 words), or ‘long’(approximately 750 words). The default value is ‘short’.
         *  title: It can be either 0 or 1. If it is set to be 0, the article generated is without titles and headings. The default value is 0.
         *  image: the probability of adding an image into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
         *  video: the probability of adding a video into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
         *  auto_links: replace specific keyword within the article with a designated link. You can choose whetherto replace just the first occurrence orall of them. 
         *				The data structure should be an array following this pattern: [keyword1, url1, all_occurrence?, keyword2, url2, all_occurrence?,...] 
         *				An example scenario would be: Replace ‘keyword1’ with ‘www.keyword1.com’ (Only first occurrence), Replace keyword2 with ‘www.keyword2.com’ 
         *				(All occurrences) auto_links shouldbeasfollows: ["keyword1","www.keyword1.com", false, "keyword2", "www.keyword2.com",true]
         *
         *  The following parameters are only available when your account is linked to valid WordAi API key. You can go to this URL 
         *  (https://af.articleforge.com/users/edit) to update your WordAi API key. Overwrite any of these parameters without a valid WordAi API key 
         *  linked to your account willget anerror.
         *
         *  quality: the quality of article. It can be either 1(Regular), 2(Unique), 3(Very Unique), 4(Readable), or 5(Very Readable). The default value is 4.
         *  regular_spinner: enable regular spinner or not. It can be either 0(disabled) or 1(enabled). The default value is 0.
         *  turing_spinner: enable turing spinner or not. It can be either 0(disabled) or 1(enabled). The default value is 0. 
         *					Note: if regular_spinner and turing_spinner are both set to 1, we will use turing spinner.
         *  rewrite_sentence: enable sentences rewrite or not. It can be either 0(disabled) or 1(enabled). The default value is 0. Note: this will automatically 
         *					  enable regular spinner if both regular_spinner and turing_spinner are disabled.
         *  rearrange_sentence: enable add/remove/rearrange sentences or not. It can be either 0(disabled) or 1(enabled). The default value is 0. 
         *						Note: this will automatically enable regular spinner if both regular_spinner and turing_spinner are disabled.
         *
         * @return string article
         */
        public JObject createArticle(JObject _params)
        {
            if (_params["keyword"] == null)
            {
                setErrorMessage("Parameter \"keyword\" is required");
                return null;
            }

            JObject result = execute("create_article", _params);
            if (isStatusSuccess(result))
            {
                return result["article"].ToObject<JObject>();
            }

            return null;
        }

        /**
         * Initiates an article with keyword keyword with given settings. Returns number format the ref_key which will be used in getApiProgress method metioned below.
         *
         * <code>
         *  $params = [
         *   'keyword' => 'Starwars',
         *   'sub_keywords' => 'Master Yoda, Darth Vader'
         *  ];
         *  
         *	$article = $af->initiateArticle($params);
         * </code>
         *
         * @param array $params 
         *	keyword: Word for generation article
         *	sub_keywords: a list of sub-keywords separated by comma (e.g. subkeyword1,subkeyword2,subkeyword3).  
         *  sentence_variation:	number of sentence variations. It can be either 1, 2, or 3. The default value is 1.
         *  paragraph_variation: number of paragraph variations. It can be either 1, 2, or 3. The default value is 1.
         *  shuffle_paragraphs: enable shuffle paragraphs or not. It can be either 0(disabled) or 1(enabled). The default value is 0.
         *  length: the length of the article. It can be either ‘very_short’(approximately 50 words), ‘short’(approximately 200 words), 
         *  		‘medium’(approximately 500 words), or ‘long’(approximately 750 words). The default value is ‘short’.
         *  title: It can be either 0 or 1. If it is set to be 0, the article generated is without titles and headings. The default value is 0.
         *  image: the probability of adding an image into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
         *  video: the probability of adding a video into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
         *  auto_links: replace specific keyword within the article with a designated link. You can choose whetherto replace just the first occurrence orall of them. 
         *				The data structure should be an array following this pattern: [keyword1, url1, all_occurrence?, keyword2, url2, all_occurrence?,...] 
         *				An example scenario would be: Replace ‘keyword1’ with ‘www.keyword1.com’ (Only first occurrence), Replace keyword2 with ‘www.keyword2.com’ 
         *				(All occurrences) auto_links shouldbeasfollows: ["keyword1","www.keyword1.com", false, "keyword2", "www.keyword2.com",true]
         *
         *  The following parameters are only available when your account is linked to valid WordAi API key. You can go to this URL 
         *  (https://af.articleforge.com/users/edit) to update your WordAi API key. Overwrite any of these parameters without a valid WordAi API key 
         *  linked to your account willget anerror.
         *
         *  quality: the quality of article. It can be either 1(Regular), 2(Unique), 3(Very Unique), 4(Readable), or 5(Very Readable). The default value is 4.
         *  regular_spinner: enable regular spinner or not. It can be either 0(disabled) or 1(enabled). The default value is 0.
         *  turing_spinner: enable turing spinner or not. It can be either 0(disabled) or 1(enabled). The default value is 0. 
         *					Note: if regular_spinner and turing_spinner are both set to 1, we will use turing spinner.
         *  rewrite_sentence: enable sentences rewrite or not. It can be either 0(disabled) or 1(enabled). The default value is 0. Note: this will automatically 
         *					  enable regular spinner if both regular_spinner and turing_spinner are disabled.
         *  rearrange_sentence: enable add/remove/rearrange sentences or not. It can be either 0(disabled) or 1(enabled). The default value is 0. 
         *						Note: this will automatically enable regular spinner if both regular_spinner and turing_spinner are disabled.
         *
         * @return int the ref_key which will be used in getApiProgress
         */
        public String initiateArticle(JObject _params)
        {
            if (_params["keyword"] == null)
            {
                setErrorMessage("Parameter \"keyword\" is required");
                return null;
            }

            var result = execute("initiate_article", _params);
            if (isStatusSuccess(result))
            {
                return result["ref_key"].ToString();
            }

            return null;
        }

        /**
         * Return the percentage of completion
         *
         * <code>
         * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
         * $progress = $af->getApiProgress(1234);
         * </code>
         *
         * @param int $ref_key ref ID in ArticleForge
         * @return int the percentage of completion, range 0-100
         */
        public int getApiProgress(ref String ref_key)
        {
            if (ref_key.Length == 0)
            {
                setErrorMessage("Parameter \"ref_key\" is required");
                return 0;
            }

            int progress = 0;
            var result = execute("get_api_progress", JObject.Parse("{\"ref_key\": \"" + ref_key + "\"}"));
            if (isStatusSuccess(result))
            {
                if (result["api_status"] == null)
                {
                    result["api_status"] = "0";
                }

                switch (Int32.Parse(result["api_status"].ToString()))
                {
                    case 0:
                        progress = 0;
                        break;
                    case 201:
                        progress = 100;
                        break;
                    default:
                        progress = (result["progress"] != null ? (int)Math.Ceiling(Double.Parse(result["progress"].ToString()) * 100) : 0);
                        break;
                }

                return progress;
            }

            ref_key = null;
            return 0;
        }

        /**
         * Returns the spintax of the article identified by ref_key
         *
         * <code>
         * $af = new viamarvin\ArticleForge\ArticleForge($apiKey);
         * $article = $af->getApiArticleResult(1234);
         * </code>
         *
         * @param int $ref_key ref ID in ArticleForge
         * @return string article text
         */
        public async Task<String> getApiArticleResult(String ref_key, String language)
        {
            String content = "";
            if (ref_key == "")
            {
                setErrorMessage("Parameter 'ref_key' is required");
                content = "";
            }

            var result = execute("get_api_article_result", JObject.Parse("{\"ref_key\": " + ref_key + "}"));
            if (isStatusSuccess(result))
            {
                content = result["article"].ToString();
            }

            if (language.ToUpper().CompareTo(CommonModule.baseLanguage) != 0)
            {
                content = await CommonModule.deepLTranslate.Translate(content, language.ToUpper());
            }

            return content;
        }

        /**
         * Executes the query on the server
         *
         * @param string $method Method API
         * @param array $params Params for method
         * @return array returns the response from the server
         */
        private JObject execute(String method, JObject _params)
        {
            if (_params == null) _params = new JObject();
            _params["key"] = key;
            //if (_params["key"])) {
            //        $this->setErrorMessage('API key is required');
            //    return false;
            //}

            String curlParams = "";
            Dictionary<String, String> _paramsObject = _params.ToObject<Dictionary<String, String>>();
            foreach (KeyValuePair<string, string> d in _paramsObject)
            {
                curlParams += d.Key + '=' + d.Value + "&";
            }
            curlParams = curlParams.Substring(0, curlParams.Length - 1);

            //    $ch = curl_init($this->apiUrl. '/'. $method);
            //curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
            //curl_setopt($ch, CURLOPT_POST, 1);
            //curl_setopt($ch, CURLOPT_POSTFIELDS, $params);
            //    $result = curl_exec($ch);
            //curl_close($ch);
            var client = new RestClient(apiUrl);
            var request = new RestRequest("api/" + method, Method.Post);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("header1", "headerval");
            request.AddParameter("application/x-www-form-urlencoded", curlParams, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            //return json_decode($result, true);
            return JObject.Parse(response.Content);
        }

    }
}
