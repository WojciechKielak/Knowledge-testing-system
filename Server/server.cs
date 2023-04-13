/*
 * Program wersja 7.0 - wszystko 31.01.2023
 * Aleksandra Szymaczak && Wojciech Kielak
 * WCY20IJ1S1
 * 
 */


using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        public delegate void WysDelegate(string[] array);

        private static WysDelegate? _wysDelegate;

        public delegate int NrDelegate(string? text);

        private static NrDelegate? _nrDelegate;

        public static File? File;

        public static string? Path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        public static void Wyswietl<T>(T[]? array)
        {
            foreach (T item in array!)
            {
                Console.WriteLine(item);
            }
        }

        public static async Task<string> Process(CancellationToken cancelToken)
        {
            try
            {
                for (var j = 0; j <= 30; j++)
                {
                    Console.Clear();
                    if (File != null) Wyswietl(File.FileQues);
                    Console.WriteLine("\t \t \t \t \t \t" + j);
                    await Task.Delay(1000, cancelToken);
                }

                return "nic";
            }
            catch (TaskCanceledException)
            {
                return "";
            }
        }

        private static void Main()
        {
            ExecuteServer();
        }

        public static void FileOperations(int a, int nr)
        {
            if (File != null) File.Load(nr, a, Path);
        }

        public static void MenuKategori()
        {
            var array = new[] {@"[0] Test A ", "[1] Test B", "[2] Test T"};
            object[] covariant = array;
            Console.WriteLine(string.Join("\n", covariant));
        }

        public static void ExecuteServer()
        {
            _wysDelegate = delegate(string[] array) { Console.WriteLine(string.Join("\n", array)); };
            _nrDelegate = delegate(string? text)
            {
                return text switch
                {
                    "0" => 0,
                    "1" => 1,
                    _ => 2
                };
            };

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            Socket listener = new Socket(ipAddr.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            File = new TxtFile();
            listener.Bind(localEndPoint);

            listener.Listen(10);

            Console.WriteLine("Waiting connection ... ");

            Socket clientSocket = listener.Accept();
            Console.Clear();
            int start = 0;
            byte[] bytes = new Byte[1024];
            byte[] message;
            string? data = null;
            int numByte;
            int a = 1;
            int nr = 0;
            var array = new[] {"Wybierz dzialanie:", "[1] Rozpocznij test", "[2] Wyjscie"};
            var array2 = new[] {"Wybierz kategorie:", "[0] Test A ", "[1] Test B", "[2] Test T"};
            int q;
            while (true)
            {
                if (start == 0)
                {
                    _wysDelegate(array);
                    data = "";
                    for (q = 0; q < array.Length; q++)
                    {
                        if (q == array.Length - 1) message = Encoding.ASCII.GetBytes(array[q] + "-3");
                        else message = Encoding.ASCII.GetBytes(array[q] + "-2");
                        clientSocket.Send(message);
                        data = "";
                        while (true)
                        {
                            numByte = clientSocket.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes,
                                0, numByte);

                            if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                                break;
                        }
                    }

                    data = data.Replace("<EOF>", "");
                    if (data == "2")
                    {
                        message = Encoding.ASCII.GetBytes("-1");
                        clientSocket.Send(message);
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        break;
                    }

                    start++;

                    Console.Clear();
                    MenuKategori();
                    data = "";

                    for (q = 0; q < array2.Length; q++)
                    {
                        if (q == array2.Length - 1) message = Encoding.ASCII.GetBytes(array2[q] + "-3");
                        else message = Encoding.ASCII.GetBytes(array2[q] + "-2");
                        clientSocket.Send(message);
                        data = "";
                        while (true)
                        {
                            numByte = clientSocket.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes,
                                0, numByte);

                            if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                                break;
                        }
                    }

                    data = data.Replace("<EOF>", "");
                    nr = _nrDelegate(data);
                    message = Encoding.ASCII.GetBytes($"Uruchomiony test kategorii: {(Kategoria) nr}-2");
                    clientSocket.Send(message);
                    while (true)
                    {
                        numByte = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes,
                            0, numByte);

                        if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                            break;
                    }
                }

                Console.Clear();
                FileOperations(a, nr);
                if (File.FileMaxNumber == 0)
                {
                    a = 1;
                    start = 0;
                    message = Encoding.ASCII.GetBytes("-5");
                    clientSocket.Send(message);
                    while (true)
                    {
                        numByte = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes,
                            0, numByte);

                        if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                            break;
                    }

                    Console.Clear();
                    continue;
                }

                a++;
                if (File.FileQues != null)
                    for (q = 0; q < File.FileQues.Length; q++)
                    {
                        if (q == File.FileQues.Length - 1) message = Encoding.ASCII.GetBytes(File.FileQues[q] + "-4");
                        else message = Encoding.ASCII.GetBytes(File.FileQues[q] + "-2");
                        clientSocket.Send(message);
                        data = "";
                        if (q == File.FileQues.Length - 1) continue;
                        while (true)
                        {
                            numByte = clientSocket.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes,
                                0, numByte);

                            if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                                break;
                        }
                    }

                var cancelSource = new CancellationTokenSource();
                Task<string> finished = Task.Run(() => Process(cancelSource.Token));

                while (true)
                {
                    data = "";
                    numByte = clientSocket.Receive(bytes);

                    data += Encoding.ASCII.GetString(bytes,
                        0, numByte);
                    data = data.Replace("<EOF>", "");


                    if (data != "nic")
                    {
                        cancelSource.Cancel();
                        cancelSource.Dispose();
                        break;
                    }

                    if (finished.IsCompleted)
                    {
                        data = finished.Result;
                        cancelSource.Dispose();
                        break;
                    }

                    message = Encoding.ASCII.GetBytes("nic");
                    clientSocket.Send(message);
                }

                data = data.Replace("<EOF>", "");


                if (data == File.FileAnsw)
                {
                    message = Encoding.ASCII.GetBytes("Poprawna odpowiedz-2");
                }
                else if (data == "nic")
                {
                    message = Encoding.ASCII.GetBytes("Czas na odpowiedz uplynal-2");
                }
                else
                {
                    message = Encoding.ASCII.GetBytes("Bledna odpowiedz-2");
                }


                clientSocket.Send(message);
                while (true)
                {
                    numByte = clientSocket.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes,
                        0, numByte);

                    if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
                        break;
                }
            }
        }

        public enum Kategoria
        {
            A,
            B,
            T
        }
    }
}