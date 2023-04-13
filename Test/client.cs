/*
 * Program wersja 7.0 - wszystko 31.01.2023
 * Aleksandra Szymaczak && Wojciech Kielak
 * WCY20IJ1S1
 * 
 */

using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Client
{
    class Program
    {
        private static Semaphore? _semafor;

        static void Main()
        {
            ExecuteClient();
        }

        public static void WyswietlOsobe(object obj)
        {
            Type objType = obj.GetType();
            var prop = objType.GetProperties();
            foreach (var p in prop)
            {
                var no = p.GetCustomAttribute<DisplayOsobaAttribute>();
                if (no != null) Console.WriteLine($"{no.DisplayOsoba}: {p.GetValue(obj)}");
            }
        }

        public static Task<string?> Process(Semaphore? semaphore)
        {
            string? a;
            a = Console.ReadLine();
            if (semaphore != null) semaphore.Release();
            return Task.FromResult(a);
        }

        static void ExecuteClient()
        {
            _semafor = new Semaphore(initialCount: 0, maximumCount: 1);
            string? imie;
            string? nazwisko;
            Console.WriteLine("Podaj imie: ");
            imie = Console.ReadLine();
            Console.WriteLine("Podaj nazwisko: ");
            nazwisko = Console.ReadLine();

            Osoba osoba = new Osoba();
            osoba.Imie = imie;
            osoba.Nazwisko = nazwisko;

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            Socket sender = new Socket(ipAddr.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(localEndPoint);

            Console.Clear();

            Console.WriteLine("Podlaczono do serwera");
            string? data;
            string? text;
            byte[] messageReceived = new Byte[1024];
            int byteRecv;
            byte[] messageSent;
            while (true)
            {
                byteRecv = sender.Receive(messageReceived);

                if (Encoding.ASCII.GetString(messageReceived,
                        0, byteRecv) == "-1")
                {
                    Console.WriteLine("Rozlaczono z serwerem");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    break;
                }

                string wiad = Encoding.ASCII.GetString(messageReceived,
                    0, byteRecv);

                string kom = wiad.Remove(0, wiad.Length - 2);
                wiad = wiad.Remove(wiad.Length - 2);
                /*
                 * dwa ostatnie znaki w wiadomosci to wiadomośc dla clienta
                 * "-2" - oznacza wyświetl wiadomość
                 * "-3" - oznacza wyswietl wiadomosc i uruchum readline ( uzywane do wyswietlania menu)
                 * "-4" - oznacza wyswietl wiadomosc, uruchom taksa w którym jest uruchamiany readline
                 *      aby osoba testujaca mogla wpisac pytanie a watek glowny rozmawia dalej z serverem.
                 *      Jezeli odpowiedz nie zostala udzielona to wysyla "nic" - server wie, ze wiadomosc nie jest
                 *      jeszcze udzielona. Watek glowny czeka na odpowiedz od serwera jessli ta widomosc to
                 *      "Czas na odpowiedz uplynal" to infromuje o tym osobe egzminujaca, a jesli nie to sprawdza czy
                 *      osoba egzaminowala wpisala juz odpowiedz
                 * "-5" - oznacza ze to koniec testu i wyswietlaja sie wyniki uzyskane przez kursanta
                 * 
                 */
                if (kom == "-2")
                {
                    Console.WriteLine(wiad);
                    if (wiad == "Poprawna odpowiedz") osoba.Punkty++;
                    messageSent = Encoding.ASCII.GetBytes("Slucham<EOF>");
                    sender.Send(messageSent);
                }

                if (kom == "-3")
                {
                    Console.WriteLine(wiad);
                    text = Console.ReadLine();
                    text = text?.ToUpper();
                    Console.Clear();
                    text += "<EOF>";
                    messageSent = Encoding.ASCII.GetBytes(text);
                    sender.Send(messageSent);
                    continue;
                }

                if (kom == "-4")
                {
                    osoba.IloscPytan++;
                    Console.WriteLine(wiad);
                    var finished = Task.Run(() => Process(_semafor));

                    while (true)
                    {
                        messageSent = Encoding.ASCII.GetBytes("nic<EOF>");
                        sender.Send(messageSent);
                        byteRecv = sender.Receive(messageReceived);
                        wiad = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                        wiad = wiad.Remove(wiad.Length - 2);

                        if (wiad == "Czas na odpowiedz uplynal")
                        {
                            Console.WriteLine(wiad);
                            Console.WriteLine("Wcisnji enter aby kontynuowac");
                            _semafor.WaitOne();
                            Console.Clear();
                            messageSent = Encoding.ASCII.GetBytes("Slucham<EOF>");
                            sender.Send(messageSent);
                            break;
                        }

                        if (finished.IsCompleted)
                        {
                            Console.Clear();
                            _semafor.WaitOne();
                            data = finished.Result;
                            data = data?.ToUpper();
                            data += "<EOF>";
                            messageSent = Encoding.ASCII.GetBytes(data);
                            sender.Send(messageSent);
                            break;
                        }
                    }
                }

                if (kom == "-5")
                {
                    Console.Clear();
                    WyswietlOsobe(osoba);
                    osoba.IloscPytan = 0;
                    osoba.Punkty = 0;
                    messageSent = Encoding.ASCII.GetBytes("Slucham<EOF>");
                    sender.Send(messageSent);
                }
            }
        }
    }
}