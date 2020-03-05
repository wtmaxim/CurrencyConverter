using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CurrencyConverter
{
    public class LuccaDevises
    {
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                string parms = String.Join(" ", args);

                if (File.Exists(parms))
                {
                    Map map = new Map();
                    List<TauxChange> neighbordsToCheck = new List<TauxChange>();

                    using (StreamReader sr = new StreamReader(parms))
                    {
                        string line = sr.ReadLine();
                        string[] strList = line.Split(';');
                        string secondeLine = sr.ReadLine();

                        TauxChange tauxChangeToConvert = BindTauxChangeToConvert(strList);                       

                        for (int i = 0; i < int.Parse(secondeLine); i++)
                        {
                            string myLine = sr.ReadLine();
                            string[] myStrList = myLine.Split(';');

                            BindMap(map, myStrList);                            
                        }

                        TauxChange firstTauxChange = new TauxChange(tauxChangeToConvert.DeviseDepart, tauxChangeToConvert.DeviseDepart, -1, tauxChangeToConvert.Resultat);
                        neighbordsToCheck.Add(firstTauxChange);

                        TauxChange match = FindShortestPath(map, neighbordsToCheck, tauxChangeToConvert.DeviseArrive);

                        Console.WriteLine("Le résultat de la conversion de " + tauxChangeToConvert.Resultat + " " + tauxChangeToConvert.DeviseDepart.Nom + " en " + tauxChangeToConvert.DeviseArrive.Nom + " est de " + Math.Ceiling(match.Resultat));
                    }
                }
                else
                {
                    Console.WriteLine("Le fichier est introuvable.");
                }
            }

            Console.ReadKey();
        }

        private static TauxChange BindTauxChangeToConvert(string[] strList)
        {
            Devise deviseDepart = new Devise(strList[0]);
            Devise deviseResultat = new Devise(strList[2]);
            decimal montant = decimal.Parse(strList[1], CultureInfo.InvariantCulture);

            TauxChange tauxChange = new TauxChange(deviseDepart, deviseResultat);
            tauxChange.Resultat = montant;

            return tauxChange;
        }

        private static TauxChange FindShortestPath(Map map, List<TauxChange> neighbordsToCheck, Devise deviseToFound)
        {
            TauxChange match;

            while (true)
            {
                neighbordsToCheck = map.GetNeighbors(neighbordsToCheck);

                match = neighbordsToCheck.FirstOrDefault(x => x.DeviseArrive.Nom.Contains(deviseToFound.Nom));
                if (match != null)
                    break;
            }

            return match;
        }

        private static void BindMap(Map map, string[] myStrList)
        {
            Devise deviseFrom = new Devise(myStrList[0]);
            Devise deviseTo = new Devise(myStrList[1]);
            decimal taux = decimal.Parse(myStrList[2], CultureInfo.InvariantCulture);

            TauxChange myTaux = new TauxChange(deviseFrom, deviseTo, taux);

            taux = decimal.Parse(myStrList[2], CultureInfo.InvariantCulture);

            TauxChange inversTaux = new TauxChange(deviseTo, deviseFrom, 1 / taux);
            map.tauxChanges.Add(myTaux);
            map.tauxChanges.Add(inversTaux);
        }
    }

    public class Devise
    {
        public string Nom { get; set; }

        public Devise(string _nom)
        {
            Nom = _nom;
        }
    }

    public class TauxChange
    {
        public Devise DeviseDepart { get; set; }
        public Devise DeviseArrive { get; set; }
        public decimal Taux { get; set; }
        public bool IsCheck { get; set; }
        public decimal Resultat { get; set; }

        public TauxChange(Devise _deviseDepart, Devise _deviseArrive, decimal _taux = -1, decimal _resultat = -1)
        {
            DeviseDepart = _deviseDepart;
            DeviseArrive = _deviseArrive;
            Taux = _taux;
            Resultat = _resultat;
        }

        public override string ToString()
        {
            return "Depart : " + DeviseDepart + " Arrivé " + DeviseArrive + " Montant " + Taux;
        }
    }

    public class Map
    {
        public List<TauxChange> tauxChanges;

        public Map()
        {
            tauxChanges = new List<TauxChange>();
        }

        public List<TauxChange> GetNeighbors(List<TauxChange> currentsTauxChanges)
        {
            List<TauxChange> neighbors = new List<TauxChange>();

            foreach (TauxChange currentTauxChange in currentsTauxChanges)
            {
                foreach (TauxChange tauxChange in tauxChanges)
                {
                    if (!tauxChange.Equals(currentTauxChange) && tauxChange.IsCheck == false)
                    {
                        if (tauxChange.DeviseDepart.Nom.Equals(currentTauxChange.DeviseArrive.Nom))
                        {
                            tauxChange.IsCheck = true;
                            tauxChange.Resultat = currentTauxChange.Resultat * tauxChange.Taux;
                            neighbors.Add(tauxChange);
                        }
                    }
                }
            }

            return neighbors;
        }
    }
}
