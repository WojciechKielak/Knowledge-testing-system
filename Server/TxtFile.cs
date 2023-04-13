using System.Text.RegularExpressions;

namespace Server
{
    class TxtFile : File
    {
        public override void Load(int katNumber, int number, string? path)
        {
            path += @"/../../../Pytania/";
            bool directoryExists = Directory.Exists(path);
            if (directoryExists)
            {
                string[] files = Directory.GetFiles(path);
                if (files.Length != 0)
                {
                    Regex regex = new Regex($".*{(Kategoria) katNumber}[a-z]*[0-9]*.txt");
                    foreach (string item in files)
                    {
                        Match match = regex.Match(item);
                        if (match.Success)
                        {
                            var groups = Regex.Match(item, @"([0-9]*).([a-z]*$)");
                            var x1 = groups.Groups[1].Value;
                            if (number == int.Parse(x1))
                            {
                                string?[] filetxt = System.IO.File.ReadAllLines(item);
                                FileQues = filetxt.Where((_, idx) => idx != filetxt.Length - 1).ToArray();
                                FileAnsw = filetxt[filetxt.Length - 1];
                                FileMaxNumber = int.Parse(x1);
                                break;
                            }

                            if (FileMaxNumber < number)
                            {
                                FileMaxNumber = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Podany folder nie istnieje");
            }
        }
    }
}