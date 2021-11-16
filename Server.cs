using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Server
{

 

    public class Boxer
    {
        public int id;
        public string name;
        public int betCourse;
 
        public int wins, loses, points;

        public Boxer(int id,string name, int betCourse)
        {
            this.name = name;
            this.betCourse = betCourse;
            this.id = id;
            wins = 0;
            loses = 0;
            points = 0;
        }

        public void summary()
        {
            Random r = new Random();
            int randomReward = r.Next(0, 3);
            this.points = this.wins * 3 - loses + randomReward;
        }
    }

    public class User
    {

        public string ses_id { get; set; }

        public string name { get; set; }
        public int betAmount { get; set; }
        public int boxerNumber { get; set; }

        public int winAmount { get; set; }
    }


    public class Unique : WebSocketBehavior
    {
        static int sess_count = 0;

        protected override void OnOpen()
        {
            sess_count++;
            Console.WriteLine("this is unique connection number " + sess_count + " id = " + ID);

            Sessions.SendTo("Connection established ", ID);

        }

    }


    public class EchoALL : WebSocketBehavior
    {

        public static List<User> users = new List<User>();
        public static List<Boxer> boxers = new List<Boxer>();

        static Boxer boxer1 = new Boxer(0,"Boxer 1", 4);
        static Boxer boxer2 = new Boxer(1,"Boxer 2", 2);
        static Boxer boxer3 = new Boxer(2,"Boxer 3", 7);
        static Boxer boxer4 = new Boxer(3,"Boxer 4", 3);

       

        protected override void OnMessage(MessageEventArgs e)
        {

            boxers.Add(boxer1);
            boxers.Add(boxer2);
            boxers.Add(boxer3);
            boxers.Add(boxer4);

            // W ilu watkach bariera sie zwalnia i pozwala wykonywać kod tylko dwie walki 
            Barrier barrier1 = new Barrier(2, (b) =>
            {}
);
            Barrier barrier2 = new Barrier(2, (b) =>
            {});
            Barrier barrier3 = new Barrier(2, (b) =>
            {
                results(users);
            });

           
            User user = new User();
            user.ses_id = ID;

            User u = JsonConvert.DeserializeObject<User>(e.Data);

            user.name = u.name;
            user.betAmount = u.betAmount;
            user.boxerNumber = u.boxerNumber;




           users.Add(user);

           Console.WriteLine("Nowy obstawienie: ( " + user.name + " ) na boksera" + boxers[u.boxerNumber].name);
           Console.WriteLine("Pula obstawienia" + user.betAmount + " z kursem "+ boxers[u.boxerNumber].betCourse);



            if (users.Count() > 4)
            {
                Task.Delay(10000).ContinueWith(t => count_before_start());
            }

          

            void count_before_start()
            {


                for (int i = 5; i > 0; i--)
                {
                    for (int j = 0; j < users.Count(); j++)
                    {
                        Sessions.SendTo("Fights will start in " + i + "seconds", users[j].ses_id);
                    }


                    System.Threading.Thread.Sleep(500);
                }
                start_game();

            }

            void fight(Boxer b1, Boxer b2, Boxer b3, Boxer b4, Boxer b5, Boxer b6 )
            {

                simulateFight(b1, b2);

                barrier1.SignalAndWait();

                simulateFight(b3, b4);

                barrier2.SignalAndWait();

                simulateFight(b5, b6);

                barrier3.SignalAndWait();

            }



            void start_game()
            {
                for (int j = 0; j < users.Count(); j++)
                {
                    Sessions.SendTo("Clear", users[j].ses_id);
                    Sessions.SendTo("Obstawiony bokser -> " + boxers[users[j].boxerNumber].name + " z kursem: "+ boxers[users[j].boxerNumber].betCourse, users[j].ses_id);
                }

                Sessions.Broadcast("Walki się zaczynają ");
              
                Thread thread = new Thread(() => fight(boxers[0], boxers[1], boxers[0], boxers[2], boxers[0], boxers[3]));
                thread.Start();
            
                Thread thread1 = new Thread(() => fight(boxers[3], boxers[2], boxers[3], boxers[1], boxers[1], boxers[2]));
                thread1.Start();


            }

            void simulateFight(Boxer boxer1, Boxer boxer2)
            {

                int b1hits = 0;
                int b2hits = 0;

                Console.WriteLine("Fight between " + boxer1.name + " VS " + boxer2.name);
                Sessions.Broadcast("Fight between " + boxer1.name + " VS " + boxer2.name);

                Random r = new Random();
                int randomHit = r.Next(0, 2);

                // po 5 rund
                for (int i = 0; i < 5; i++)
                {
                   
                    Sessions.Broadcast("Runda numer" + i);
                    // po 3 minuty 
                    for (int j = 0; j < 4; j++)
                    {
                        Console.WriteLine("Minuta " + j);
                        Sessions.Broadcast("Minuta " + j);
                        if (randomHit == 0)
                        {
                            Console.WriteLine( boxer1.name + " Uderza");
                            Sessions.Broadcast(boxer1.name + " Uderza");
                            b1hits++;
                        }
                        else
                        {
                            Console.WriteLine(boxer2.name + " Uderza");
                            Sessions.Broadcast(boxer2.name + " Uderza");
                            b2hits++;
                        }
                    }
                }

                if (b1hits > b2hits)
                {
                    boxer1.wins++;
                    boxer2.loses++;
                }
                else if (b1hits < b2hits)
                {
                    boxer1.loses++;
                    boxer2.wins++;
                }

            }

            void results(List<User> users)
            {
                    int winnerId = 0;
                    
                    for (int j = 0; j < boxers.Count(); j++)
                    {
                        boxers[j].summary();
                    }

                    for (int j = 0; j < boxers.Count(); j++)
                    {
                        if (boxers[winnerId].points < boxers[j].points)
                        {
                            winnerId = boxers[j].id;
                        }
                    }   

                    for (int j = 0; j < users.Count(); j++)
                     {
                            if (users[j].boxerNumber==winnerId)
                            {
                             Console.WriteLine("Ustawiam wygraną ");
                                int winningSum = 0;
                                winningSum = users[j].betAmount * boxers[users[j].boxerNumber].betCourse;
                        users[j].winAmount = winningSum;
                            }
                      }


                Console.WriteLine();
                Console.WriteLine("Boxer | Wins | Loses | Points");
                Console.WriteLine(boxer1.name + " "+ boxer1.wins + " " + boxer1.loses + " "+ boxer1.points);
                Console.WriteLine(boxer2.name + " " + boxer2.wins + " " + boxer2.loses + " " + boxer2.points);
                Console.WriteLine(boxer3.name + " " + boxer3.wins + " " + boxer3.loses + " " + boxer3.points);
                Console.WriteLine(boxer4.name + " " + boxer4.wins + " " + boxer4.loses + " " + boxer4.points);

                Console.WriteLine("Wygrał: "+ boxers[winnerId].name);


                Sessions.Broadcast("");
                Sessions.Broadcast("Boxer | Wins | Loses | Points");
                Sessions.Broadcast(boxer1.name + " " + boxer1.wins + " " + boxer1.loses + " " + boxer1.points);
                Sessions.Broadcast(boxer2.name + " " + boxer2.wins + " " + boxer2.loses + " " + boxer2.points);
                Sessions.Broadcast(boxer3.name + " " + boxer3.wins + " " + boxer3.loses + " " + boxer3.points);
                Sessions.Broadcast(boxer4.name + " " + boxer4.wins + " " + boxer4.loses + " " + boxer4.points);

                Sessions.Broadcast("Wygrał: " + boxers[winnerId].name);

                for (int j = 0; j < users.Count(); j++)
                {
                    if (users[j].winAmount > 0)
                    {
                        Sessions.SendTo("Twój szczęśliwy dzień, wygrałeś:" + users[j].winAmount, users[j].ses_id);
                    }
                    else
                    {
                        Sessions.SendTo("Niestety przegrałeś" , users[j].ses_id);
                    }
                  

                }



            }



        }

   
    }


    class Program
    {
        static void Main(string[] args)
        {
            //127.0.0.1:1900
            WebSocketServer serv = new WebSocketServer("ws://127.0.0.1:1900");


            serv.AddWebSocketService<EchoALL>("/EchoAll");
            serv.AddWebSocketService<Unique>("/Unique");
            serv.Start();
            Console.WriteLine("server ws://127.0.0.1:1900 started");
            Console.WriteLine("to stop press any key...");
            Console.ReadKey();
            serv.Stop();




        }


    }


}
