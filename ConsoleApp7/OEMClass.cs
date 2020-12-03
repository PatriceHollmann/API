using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp7
{
    class OEMClass
    {
        public void Request(string method, string path, string body)
        {
            var date = DateTime.Now.ToString();

            var contentMD5 = GetMd5Hash(body);
            string part1 = "APIAuth-HMAC-SHA256";
            string part2 = "112233"; //TODO заменить
            string contentType = "application/json"; 
            string part3 = $"{method},{contentType},{contentMD5},{path},{date}";
            var secretKey = "112233"; //TODO заменить
            string part3Encoded = CreateToken(part3, secretKey);
            var authHeader = $"{part1}{part2}:{part3Encoded}";
            var host = "https://scc.suse.com"; //TODO заменить
            // Create a request using a URL that can receive a post.
            WebRequest request = WebRequest.Create($"{host}{path}");
            // Set the Method property of the request to POST.
            request.Method = method;
            request.Headers["Date"] = date;
            request.Headers["Content-MD5"] = contentMD5;
            request.Headers["Authorization"] = authHeader;
            // Create POST data and convert it to a byte array.

            byte[] byteArray = Encoding.UTF8.GetBytes(body);

            // Set the ContentType property of the WebRequest.
            request.ContentType = contentType;
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();

            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine(responseFromServer);
            }

            // Close the response.
            response.Close();
        }
        static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
        private string CreateToken(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}

