﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebCreator;

namespace UnitTest.Lib
{
    internal class CloudFlareAPI
    {
        // API Url
        private static String tokenVerifyAPI = "https://api.cloudflare.com/client/v4/user/tokens/verify";
        private static String zoneAPI = "https://api.cloudflare.com/client/v4/zones";

        private static String error_message = "";
        private static String accountID = "3700ccc3ebfeeb677309a5f1e1f4caff";
        private static Hashtable zoneMap = new Hashtable();
        private static Hashtable dnsMap = new Hashtable();

        public CloudFlareAPI() {
            GetZoneID();
        }

        public async Task CreateDnsThreadAsync(String domain, String ip)
        {
            try
            {
                String[] domainInfo = domain.Split(".");
                if (domainInfo.Length >= 2)
                {
                    domain = domainInfo[domainInfo.Length - 2] + "." + domainInfo[domainInfo.Length-1];
                    CreateZone(domain);
                    GetZoneID();
                    CreateDns(domain, domain, ip);
                    ListDns(domain, domain);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public async Task UpdateDnsThreadAsync(String domain, String newDomain, String ip)
        {
            try
            {
                GetZoneID();
                UpdateDns(domain, newDomain, ip);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task DeleteDnsThreadAsync(String domain, String ip)
        {
            try
            {
                String[] domainInfo = domain.Split(".");
                if (domainInfo.Length >= 2)
                {
                    domain = domainInfo[domainInfo.Length - 2] + "." + domainInfo[domainInfo.Length - 1];
                    DeleteZone(domain, "");
                    ListZone(domain, "");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /*
         * curl -X GET "https://api.cloudflare.com/client/v4/user/tokens/verify" \
            -H "Authorization: Bearer vKQiwIFU0Eyz269KeOjBsliYyaaEQMqZRe3QA9TE" \
            -H "Content-Type:application/json"
         */
        public bool VerifyTokenAsync()
        {
            HttpWebRequest request = WebRequest.CreateHttp( tokenVerifyAPI );
            request.Method = "Get";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);

            string sResult = "";
            using (WebResponse response = request.GetResponse())
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                sResult = (streamReader.ReadToEnd());
            dynamic jResult = JsonConvert.DeserializeObject(sResult);
            return jResult.success;
        }

        /*
         * curl -X GET "https://api.cloudflare.com/client/v4/zones" \
            -H "X-Auth-Email: uniqtop@gmail.com" \
            -H "Authorization: Bearer vKQiwIFU0Eyz269KeOjBsliYyaaEQMqZRe3QA9TE" \
            -H "Content-Type: application/json"
         */
        public bool GetZoneID()
        {
            HttpWebRequest request = WebRequest.CreateHttp(zoneAPI);
            request.Method = "Get";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);

            string sResult = "";
            using (WebResponse response = request.GetResponse())
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                sResult = (streamReader.ReadToEnd());
            dynamic jResult = JsonConvert.DeserializeObject(sResult);

            JArray zoneAry = (JArray)jResult.result;
            try
            {
                for (int i = 0; i < zoneAry.Count; i++)
                {
                    zoneMap.Add(zoneAry[i]["name"].ToString(), zoneAry[i]["id"].ToString());
                    Console.WriteLine("Zone name:" + zoneAry[i]["name"].ToString() + " Zone ID:" + zoneAry[i]["id"].ToString());
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            
            return jResult.success;
        }

        /*
         * curl -X GET "https://api.cloudflare.com/client/v4/zones?name=example.com&status=active&account.id=01a7362d577a6c3019a474fd6f485823&account.name=Demo Account&page=1&per_page=20&order=status&direction=desc&match=all" \
             -H "X-Auth-Email: user@example.com" \
             -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
             -H "Content-Type: application/json"
         */
        public String ListZone(String zone, String status="active")
        {
            String zoneName = zone.Length > 0 ? "&name=" + zone : "";
            status = status.Length > 0 ? "&status=" + status : "";
            String param = "?" +
                "account.id=" + accountID +
                zoneName +
                status +
                "&page=1" +
                "&per_page=20" +
                "&order=status" +
                "&direction=desc" +
                "&match=all";
            HttpWebRequest request = WebRequest.CreateHttp(zoneAPI + param);
            request.Method = "Get";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);

            String ret = "";
            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                ret = jResult.success;

                JArray zoneAry = (JArray)jResult.result;
                try
                {
                    for (int i = 0; i < zoneAry.Count; i++)
                    {
                        Console.WriteLine("Zone name:" + zoneAry[i]["name"].ToString() + " Zone ID:" + zoneAry[i]["id"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
            }

            return ret;
        }

        /*
         * curl -X POST "https://api.cloudflare.com/client/v4/zones" \
             -H "X-Auth-Email: user@example.com" \
             -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
             -H "Content-Type: application/json" \
             --data '{"name":"example.com","account":{"id":"01a7362d577a6c3019a474fd6f485823"},"type":"full"}'
         */
        public String CreateZone(String zone)
        {
            HttpWebRequest request = WebRequest.CreateHttp(zoneAPI);
            request.Method = "Post";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);

            //"name":"example.com","account":{"id":"01a7362d577a6c3019a474fd6f485823"},"type":"full"
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = string.Format("{{\"name\":\"{0}\",\"account\":{{\"id\":\"{1}\"}},\"type\":\"full\"}}", zone, accountID);
                //string json = "{\"name\":\"" + item2 + "\"," + "\"email\":\"" + item1 + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            String ret = "";
            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                ret = jResult.success;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string sResult = reader.ReadToEnd();
                        Console.WriteLine(sResult);

                        dynamic jResult = JsonConvert.DeserializeObject(sResult);
                        ret = jResult.success;
                    }
                }
            }

            return ret;
        }

        /*
         * curl -X DELETE "https://api.cloudflare.com/client/v4/zones/023e105f4ecef8ad9ca31a8372d0c353" \
                -H "X-Auth-Email: user@example.com" \
                -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
                -H "Content-Type: application/json"
         */
        public bool DeleteZone(String zone, String status = "active")
        {
            String zoneName = zone.Length > 0 ? "&name=" + zone : "";
            status = status.Length > 0 ? "&status=" + status : "";
            String param = "?" +
                "account.id=" + accountID +
                zoneName +
                status +
                "&page=1" +
                "&per_page=20" +
                "&order=status" +
                "&direction=desc" +
                "&match=all";
            HttpWebRequest request = WebRequest.CreateHttp(zoneAPI + param);
            request.Method = "Get";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);

            bool ret = false;
            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);

                JArray zoneAry = (JArray)jResult.result;
                if(zoneAry.Count > 0)
                {
                    request = WebRequest.CreateHttp(zoneAPI + "/" + zoneAry[0]["id"].ToString());
                    request.Method = "Delete";
                    request.ContentType = "application/json";
                    request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
                    request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);
                    using (WebResponse response = request.GetResponse())
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                        sResult = (streamReader.ReadToEnd());
                    jResult = JsonConvert.DeserializeObject(sResult);
                    ret = jResult.success;
                }
            }
            catch (Exception ex)
            {
            }
            return ret;
        }


        /*
         * curl -X POST "https://api.cloudflare.com/client/v4/zones/1a6b4899498198b052c1d6679ce98400/dns_records" \
             -H "X-Auth-Email: uniqtop@gmail.com" \
             -H "Authorization: Bearer vKQiwIFU0Eyz269KeOjBsliYyaaEQMqZRe3QA9TE" \
             -H "Content-Type: application/json" \
             --data '{"type":"A","name":"example.com","content":"127.0.0.1","ttl":3600,"priority":10,"proxied":false}'
         */
        public bool CreateDns(String zone, String dns, String ip="127.0.0.1") {
            bool result = true;
            if (zoneMap[zone] == null) return false;

            String param = "/" + zoneMap[zone] + "/dns_records";
            String apiURL = zoneAPI + param;
            HttpWebRequest request = WebRequest.CreateHttp( apiURL );
            request.Method = "Post";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);
            //request.Headers.Add("X-Auth-Key", Config.CloudFlareAPIKey);
            

            //"name":"example.com","account":{"id":"01a7362d577a6c3019a474fd6f485823"},"type":"full"
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = string.Format("{{\"type\":\"A\",\"name\":\"{0}" +
                    "\",\"content\":\"{1}" +
                    "\",\"ttl\":3600,\"priority\":10,\"proxied\":false}}", dns, ip);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            String ret = "";
            WebResponse response;
            try
            {
                string sResult = "";
                using (response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                ret = jResult.success;
            }
            catch (WebException e)
            {
                using (response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string sResult = reader.ReadToEnd();
                        //Console.WriteLine(text);

                        dynamic jResult = JsonConvert.DeserializeObject(sResult);
                        ret = jResult.success;
                    }
                }
            }

            return result;
        }

        /*
         * 
         curl -X GET "https://api.cloudflare.com/client/v4/zones/023e105f4ecef8ad9ca31a8372d0c353/dns_records?type=A&name=example.com&content=127.0.0.1&proxied=false&page=1&per_page=100&order=type&direction=desc&match=all" \
             -H "X-Auth-Email: user@example.com" \
             -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
             -H "Content-Type: application/json"
         */
        public bool ListDns(String zone, String dns)
        {
            bool result = false;
            if (zoneMap[zone] == null) return false;

            String param = "/" + zoneMap[zone] + "/dns_records";
            String apiURL = zoneAPI + param;
            //String apiURL = zoneAPI + param;
            HttpWebRequest request = WebRequest.CreateHttp(apiURL);
            request.Method = "Get";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);
            //request.Headers.Add("X-Auth-Key", Config.CloudFlareAPIKey);

            String ret = "";
            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                JArray ary = (JArray)jResult.result;
                try
                {
                    for (int i = 0; i < ary.Count; i++)
                    {
                        dnsMap[ary[i]["name"].ToString()] = ary[i]["id"].ToString();
                        Console.WriteLine("DNS name:" + ary[i]["name"].ToString() + " Dns ID:" + ary[i]["id"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                ret = jResult.success;
                /*
                 * {
  "success": true,
  "errors": [],
  "messages": [],
  "result": [
    {
      "id": "372e67954025e0ba6aaa6d586b9e0b59",
      "type": "A",
      "name": "example.com",
      "content": "198.51.100.4",
      "proxiable": true,
      "proxied": false,
      "ttl": 3600,
      "locked": false,
      "zone_id": "023e105f4ecef8ad9ca31a8372d0c353",
      "zone_name": "example.com",
      "created_on": "2014-01-01T05:20:00.12345Z",
      "modified_on": "2014-01-01T05:20:00.12345Z",
      "data": {},
      "meta": {
        "auto_added": true,
        "source": "primary"
      }
    }
  ]
}
                 */
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        /*
         curl -X DELETE "https://api.cloudflare.com/client/v4/zones/023e105f4ecef8ad9ca31a8372d0c353/dns_records/372e67954025e0ba6aaa6d586b9e0b59" \
             -H "X-Auth-Email: user@example.com" \
             -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
             -H "Content-Type: application/json"
         */
        public bool DeleteDns(String zone, String dns) {
            bool ret = false;
            bool result = false;
            if (dns.CompareTo(zone) != 0) dns = dns + "." + zone;
            if (zoneMap[zone] == null || dnsMap[dns] == null) return false;

            String param = "/" + zoneMap[zone] + "/dns_records/" + dnsMap[dns];
            //String apiURL = "https://api.cloudflare.com/client/v4/zones/1a6b4899498198b052c1d6679ce98400/dns_records";
            String apiURL = zoneAPI + param;
            HttpWebRequest request = WebRequest.CreateHttp(apiURL);
            request.Method = "Delete";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);
            //request.Headers.Add("X-Auth-Key", Config.CloudFlareAPIKey);

            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                ret = jResult.success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }


        /*
         curl -X PUT "https://api.cloudflare.com/client/v4/zones/023e105f4ecef8ad9ca31a8372d0c353/dns_records/372e67954025e0ba6aaa6d586b9e0b59" \
             -H "X-Auth-Email: user@example.com" \
             -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
             -H "Content-Type: application/json" \
             --data '{"type":"A","name":"example.com","content":"127.0.0.1","ttl":3600,"proxied":false}'
         * 
         */
        public bool UpdateDns(String zone, String dns, String newDns, String newIp="127.0.0.1")
        {
            bool ret = false;
            bool result = false;
            if (dns.CompareTo(zone) != 0) dns = dns + "." + zone;
            if (zoneMap[zone] == null || dnsMap[dns] == null) return false;

            String param = "/" + zoneMap[zone] + "/dns_records/" + dnsMap[dns];
            String apiURL = zoneAPI + param;
            HttpWebRequest request = WebRequest.CreateHttp(apiURL);
            request.Method = "Put";
            request.ContentType = "application/json";
            request.Headers.Add("X-Auth-Email", Config.CloudFlareAPIEmail);
            request.Headers.Add("Authorization", "Bearer " + Config.CloudFlareAPIKey);
            //request.Headers.Add("X-Auth-Key", Config.CloudFlareAPIKey);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = string.Format("{{\"type\":\"A\",\"name\":\"{0}" +
                    "\",\"content\":\"{1}" +
                    "\",\"ttl\":3600,\"priority\":10,\"proxied\":false}}", newDns, newIp);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                ret = jResult.success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }
    }
}