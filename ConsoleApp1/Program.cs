using System;
using System.Collections.Generic;
using System.IO;

struct Csomag
{
    public string Azonosito;
    public double Suly;
    public double Terfogat;
}

struct CsaladiCsomagok
{
    public string CsaladId;
    public double OsszSuly;
    public double OsszTerfogat;
    public List<Csomag> Csomagok;
}

struct Kontener
{
    public int Azonosito;
    public int Erokar;
    public double Suly;
    public double Terfogat;
    public List<CsaladiCsomagok> Csaladok;
}

class Program
{
    static Kontener[] InitKontenerek()
    {
        Kontener[] k = new Kontener[5];
        int[] erokarok = { -2, -1, 0, 1, 2 };
        for (int i = 0; i < 5; i++)
        {
            k[i].Azonosito = i + 1;
            k[i].Erokar = erokarok[i];
            k[i].Suly = 0;
            k[i].Terfogat = 0;
            k[i].Csaladok = new List<CsaladiCsomagok>();
        }
        return k;
    }

    static bool Belefere(Kontener k, CsaladiCsomagok cs)
    {
        return k.Suly + cs.OsszSuly <= 1500 && k.Terfogat + cs.OsszTerfogat <= 6.0;
    }

    static double SzamolCG(Kontener[] kontenerek)
    {
        double sulyOssz = 0, szorzat = 0;
        for (int i = 0; i < 5; i++)
        {
            sulyOssz += kontenerek[i].Suly;
            szorzat += kontenerek[i].Suly * kontenerek[i].Erokar;
        }
        return sulyOssz == 0 ? 0 : szorzat / sulyOssz;
    }

    static Csomag KadikLegnehezebb(Dictionary<string, CsaladiCsomagok> szotár, int k)
    {
        List<Csomag> osszes = new List<Csomag>();
        foreach (var kv in szotár)
            foreach (Csomag c in kv.Value.Csomagok)
                osszes.Add(c);

        for (int i = 0; i < osszes.Count - 1; i++)
            for (int j = i + 1; j < osszes.Count; j++)
                if (osszes[j].Suly > osszes[i].Suly)
                {
                    Csomag tmp = osszes[i];
                    osszes[i] = osszes[j];
                    osszes[j] = tmp;
                }

        return osszes[k - 1];
    }

    static void Main()
    {

        var szotár = new Dictionary<string, CsaladiCsomagok>();
        string[] sorok = File.ReadAllLines("csomagok.csv");

        for (int i = 0; i < sorok.Length; i++)
        {
            string[] r = sorok[i].Split(';');
            Csomag c = new Csomag();
            c.Azonosito = r[0];
            c.Suly = double.Parse(r[2].Replace('.', ','));
            c.Terfogat = double.Parse(r[3].Replace('.', ','));

            string csId = r[1];
            if (!szotár.ContainsKey(csId))
            {
                CsaladiCsomagok uj = new CsaladiCsomagok();
                uj.CsaladId = csId;
                uj.OsszSuly = 0;
                uj.OsszTerfogat = 0;
                uj.Csomagok = new List<Csomag>();
                szotár[csId] = uj;
            }

            CsaladiCsomagok cs = szotár[csId];
            cs.Csomagok.Add(c);
            cs.OsszSuly += c.Suly;
            cs.OsszTerfogat += c.Terfogat;
            szotár[csId] = cs;
        }

        Kontener[] kontenerek = InitKontenerek();

        Csomag harmadik = KadikLegnehezebb(szotár, 3);
        Console.WriteLine($"3. legnehezebb: {harmadik.Azonosito} - {harmadik.Suly} kg");

        List<CsaladiCsomagok> csaladok = new List<CsaladiCsomagok>();
        foreach (var kv in szotár)
            csaladok.Add(kv.Value);

        for (int i = 0; i < csaladok.Count - 1; i++)
            for (int j = i + 1; j < csaladok.Count; j++)
                if (csaladok[j].OsszSuly > csaladok[i].OsszSuly)
                {
                    CsaladiCsomagok tmp = csaladok[i];
                    csaladok[i] = csaladok[j];
                    csaladok[j] = tmp;
                }

        for (int f = 0; f < csaladok.Count; f++)
        {
            CsaladiCsomagok cs = csaladok[f];
            int legjobb = -1;
            double legjobbCG = double.MaxValue;

            for (int i = 0; i < 5; i++)
            {
                if (Belefere(kontenerek[i], cs))
                {
                    kontenerek[i].Suly += cs.OsszSuly;
                    kontenerek[i].Terfogat += cs.OsszTerfogat;
                    double cg = Math.Abs(SzamolCG(kontenerek));
                    kontenerek[i].Suly -= cs.OsszSuly;
                    kontenerek[i].Terfogat -= cs.OsszTerfogat;

                    if (cg < legjobbCG)
                    {
                        legjobbCG = cg;
                        legjobb = i;
                    }
                }
            }

            if (legjobb == -1)
            {
                Console.WriteLine($"{cs.CsaladId} nem fért be!");
            }
            else
            {
                kontenerek[legjobb].Suly += cs.OsszSuly;
                kontenerek[legjobb].Terfogat += cs.OsszTerfogat;
                kontenerek[legjobb].Csaladok.Add(cs);
            }
        }

        Console.WriteLine("--- LUFTHANSA JÁRAT RAKODÁSI TERV ---");
        for (int i = 0; i < 5; i++)
            Console.WriteLine($"Konténer {kontenerek[i].Azonosito} (Erőkar: {kontenerek[i].Erokar}): " +
                $"{kontenerek[i].Suly:F2} kg / {kontenerek[i].Terfogat:F2} m^3 - {kontenerek[i].Csaladok.Count} család");

        double cgFinal = SzamolCG(kontenerek);
        Console.WriteLine($"\nA repülőgép VÉGSŐ súlypontja (CG): {cgFinal:F4}");
        if (Math.Abs(cgFinal) < 0.5)
            Console.WriteLine("A gép tökéletes egyensúlyban van. Felszállás engedélyezve!");
        else
            Console.WriteLine("FIGYELEM: Túl nagy az eltérés!");
    }
}

//A tényleges ok: a két kód buborék-rendezése ties esetén más sorrendet ad, ami dominószerűen más elosztást eredményez a greedy algoritmusban