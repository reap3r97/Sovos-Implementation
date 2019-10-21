using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace TestApi001
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string ACCESS_KEY = "3a7f5c5be8a64fd8845423058f8d9e84";
            string SECRET_KEY = "757e36496be640c2ab9d1abe9c8a5234";
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            string authHedr = getAuthHeader(timestamp, ACCESS_KEY, SECRET_KEY);
            Label1.Text += "<br/>Authentication:" + authHedr;
            Label1.Text += "<br/>x-request-date:" + timestamp;
            string tomorrow = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"); //To add in commit date

            KeyValuePair<string, string>[] hdrs = new KeyValuePair<string, string>[2];
            hdrs[0] = new KeyValuePair<string, string>("Authorization", authHedr);
            hdrs[1] = new KeyValuePair<string, string>("x-request-date", timestamp);
            string jsnStrin = "{\"currencyCode\": \"USD\",\"invoiceDate\": \"2019-10-09\",\"invoiceNumber\": \"INV-123456\",\"deliveryAmount\": 8.00,\"commitDate\":\"2019-10-17\",\"lineItems\": [{ \"description\": \"Clothing - Modern Fit Jeans (30x28)\",\"goodServiceCode\": \"CLTH867\",\"grossAmount\": 200,\"identifier\": \"INV-123456-001\",\"quantity\": 2,\"deliveryDate\": \"2019-05-13\"},{\"description\": \"Clothing - Blue Buttoned Down Dress Shirt (M)\",\"goodServiceCode\": \"CLTH5309\",\"grossAmount\": 75,\"identifier\": \"INV-123456-002\",\"quantity\": 1,\"deliveryDate\": \"2019-05-13\",\"discounts\": [{\"amount\": 10}]}],\"shipFrom\": {\"address\": \"123 Main Street\",\"cityMunicipality\": \"New York\",\"country\": \"USA\",\"stateProvince\": \"NY\",\"zipPostalCode\": \"10004\"},\"shipTo\": {\"address\": \"451 Broadway\",\"cityMunicipality\": \"Denver\",\"country\": \"USA\",\"stateProvince\": \"CO\",\"zipPostalCode\": \"80203\"},\"billTo\": {\"address\": \"451 Broadway\",\"cityMunicipality\": \"Denver\",\"country\": \"USA\",\"stateProvince\": \"CO\",\"zipPostalCode\": \"80203\"}}";
            this.POST("https://uat-api-s1.sovos.com/api/pre-auth/simple-connect-ar/v1/transactions?state=SCHED_COMMIT", hdrs, jsnStrin);


        }
        static string getAuthHeader(string timestamp, string access_key, string secretKey)
        {
            string plaintextSignature = timestamp + access_key;
            string hmacDigest = determineAuthDigest(plaintextSignature, secretKey);
            string authHeader = access_key + ":" + hmacDigest;
            return authHeader;
        }
        static string determineAuthDigest(string plaintextSignature, string secretKey)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(plaintextSignature);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        // POST a JSON string
        void POST(string url, KeyValuePair<string, string>[] headers, string jsonContent)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            foreach (KeyValuePair<string, string> item in headers)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            request.Headers.Add("Accept-Language", "en-US");
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";


            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            long length = 0;
            try
            {
                using (var response = request.GetResponse())
                {
                    length = response.ContentLength;

                    Label1.Text += "<br/>Length is : " + length;
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        String responseString = reader.ReadToEnd();

                        Label1.Text += "<br/>Response is : " + responseString;
                    }

                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
    }
}