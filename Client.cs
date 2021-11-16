using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Client
{

    // Klasa do tworzenia uzytkownika 
    class GamblingPlayer
    {
        public string name;
        public int betAmount;
        public int winAmount;
        public int boxerNumber;


        public GamblingPlayer(string country, int betAmount, int boxerNumber )
        {
            this.name = country;
            this.betAmount = betAmount;
            this.winAmount = 0;
            this.boxerNumber = boxerNumber;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            using (WebSocket ws = new WebSocket("ws://127.0.0.1:1900/Unique"))
            {

                ws.OnOpen += Ws1_OnOpen;

            }


            using (WebSocket ws = new WebSocket("ws://127.0.0.1:1900/EchoAll"))
            {
                ws.OnMessage += Ws_OnMessage;
                ws.Connect();


                Console.Write("type your name: ");
                string name = Console.ReadLine();
                Console.Write("type name of your bet amount: ");
                string betAmountString = Console.ReadLine();
                Console.Write("type name your boxer number: ");
                string boxerNumberString = Console.ReadLine();

                int betAmount;
                Int32.TryParse(betAmountString, out betAmount);

                int boxerNumber;
                Int32.TryParse(boxerNumberString, out boxerNumber);


                GamblingPlayer gamblingPlayer = new GamblingPlayer(name, betAmount, boxerNumber);
                string json = JsonConvert.SerializeObject(gamblingPlayer);

                ws.Send(json);

                //ws.Send("msg from client");
                Console.ReadKey();


            }

        }



        private static void Ws1_OnOpen(object sender, EventArgs e)
        {
        }

        private static void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            switch (e.Data.ToString())
            {
                case "Clear":
                    Console.Clear();
                    break;


                default:
                    Console.WriteLine(e.Data);
                    break;
            }


        }
    }
}
