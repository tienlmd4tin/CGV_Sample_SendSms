using Newtonsoft.Json;
using RestSharp;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SendSms
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var userName = "";
            var password = "";
            var brandName = "";
            var phone = "0856973887";
            var smsContent = "CVG Kiem tra tin nhan thuong hieu";

            var smsId = Guid.NewGuid().ToString();

            //sending sms
            SendSmsCSKH(userName, password, brandName, smsContent, phone, smsId);

            Thread.Sleep(10000);

            //tracking sms
            TrackingSms(userName, password, smsId);

            Console.ReadKey();
        }

        private static void TrackingSms(string userName, string password, string smsid)
        {
            var client = new RestClient("http://br.noahsms.com/");
            var request = new RestRequest("api/SendSms/TrackingSms");

            request.Method = Method.POST;
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.Parameters.Clear();

            var body = new
            {
                UserName = userName,
                Password = Md5Hash(password, string.Empty),
                ClientId = smsid
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);
            var content = response.Content; // raw content as string

            if (!string.IsNullOrEmpty(content))
            {
                var objRs = JsonConvert.DeserializeObject<ObjRsChecking>(content);

                if (objRs.code.Equals("1"))
                {
                    foreach (var item in objRs.data)
                    {
                        Console.WriteLine($"Phone: {item.phone} - Telco: {item.telco} - Status: {item.status} - TimeResponse: {item.time_response}");
                    }
                }
                else
                {
                    Console.WriteLine("Checking Sms fail - " + objRs.description);
                }
            }
            else
            {
                Console.WriteLine("Tracking Sms fail");
            }
        }

        private static void SendSmsCSKH(string userName, string password, string brandName, string smsContent, string phone, string smsId)
        {
            var client = new RestClient("http://br.noahsms.com/");
            var request = new RestRequest("api/SendSms/SendSmsCskhWithoutChecksum");

            request.Method = Method.POST;
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.Parameters.Clear();

            var body = new
            {
                UserName = userName,
                Password = Md5Hash(password, string.Empty),
                BrandName = brandName,
                SmsContent = smsContent,
                TimeSend = "",
                Phones = phone,
                ClientId = smsId,
                CheckSum = "",
                IsUnicode = false
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);
            var content = response.Content; // raw content as string

            if (!string.IsNullOrEmpty(content))
            {
                var objRs = JsonConvert.DeserializeObject<ObjRsSendSms>(content);

                if (objRs.code.Equals("1"))
                {
                    Console.WriteLine("Send Sms successfully");
                }
                else
                {
                    Console.WriteLine("Send Sms fail - " + objRs.description);
                }
            }
            else
            {
                Console.WriteLine("Send Sms fail");
            }
        }

        private static string Md5Hash(string data, string salt)
        {
            var MD5Code = new MD5CryptoServiceProvider();
            byte[] byteDizisi;
            if (string.IsNullOrEmpty(salt))
                byteDizisi = Encoding.UTF8.GetBytes(data);
            else
                byteDizisi = Encoding.UTF8.GetBytes(salt + data);

            byteDizisi = MD5Code.ComputeHash(byteDizisi);
            StringBuilder sb = new StringBuilder();
            foreach (byte ba in byteDizisi)
            {
                sb.Append(ba.ToString("x2").ToLower());
            }
            return sb.ToString();
        }
    }

    public class ObjRsSendSms
    {
        public string code { get; set; }
        public string description { get; set; }
    }

    public class ObjRsChecking
    {
        public string code { get; set; }
        public string description { get; set; }
        public ObjRsDetail[] data { get; set; }
    }

    public class ObjRsDetail
    {
        public string phone { get; set; }
        public string telco { get; set; }
        public string sms_content { get; set; }
        public string status { get; set; }
        public string time_response { get; set; }
    }
}