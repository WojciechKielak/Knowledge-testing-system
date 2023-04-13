namespace Client
{
    public class Osoba
    {
        [DisplayOsoba("Imie")] public string? Imie{ get; set; }

        [DisplayOsoba("Nazwisko")] public string? Nazwisko{ get; set; }

        [DisplayOsoba("Liczba punktow")] public int Punkty { get; set; }
        [DisplayOsoba("Ilosc pytan")] public int IloscPytan { get; set; }
    }
}