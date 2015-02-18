using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

///@author Tobias Moser, Jan Plank, Stefan Sonntag

namespace FRMC_Kinect
{
    public class GenreFinder
    {

        /// <summary>
        /// Findet alle Genres die in allen übergebenen Listen vorhanden sind.
        /// </summary>
        /// <param name="genreLists"></param>
        /// <returns></returns>
        public static string FindMatch(List<List<string>> genreLists)
        {
            string match = "Kein Match gefunden";
            int amountUsers = genreLists.Count;

            //Wenn die genreLists keinen Inhalt haben, dann wird null zurück gegeben.
            if (amountUsers >= 1)
            {          
                List<string> matchingGenres = null;
            
                //alle genre Listen in eine Liste packen um GroupBy anzuwenden
                List<string> allGenres = JoinLists(genreLists);

                //slektiert nur die elemente die so oft vorkommen wie es user gibt.
                //also wenn es 2 user gibt muss ein genre mindestens 2 mal vorkommen und so weiter.
                var duplicateGenres = allGenres.GroupBy(g => g)
                            .Where(group => group.Count() >= amountUsers)
                            .Select(group => group.Key);

                //erhaltene Genres von IEnumerable umwandeln in eine liste
                matchingGenres = duplicateGenres.ToList();

                Random rnd = new Random();
          
                if(matchingGenres.Count > 1) {
                    //Wenn es mehrere passende genres gibt muss ein der passenden ausgewählt werden
                    int randomIndex = rnd.Next(0, matchingGenres.Count);
                    match = matchingGenres.ElementAt(randomIndex);
                
                } else {
                    //Wenn es keine passende genre gibt, oder wenn es nur einen user gibt muss irgendein zufalls genre herausgefunden worden.
                    int randomIndex = rnd.Next(allGenres.Count);
                    match = allGenres.ElementAt(randomIndex);
                }
            }

            return match;
        }


        /// <summary>
        /// Fügt alle Listen zu einer zusammen
        /// </summary>
        /// <param name="listsToJoin"></param>
        /// <returns></returns>
        private static List<string> JoinLists(List<List<string>> listsToJoin)
        {
            List<string> allJoined = new List<string>();

            foreach (List<string> list in listsToJoin)
            {
                allJoined.AddRange(list);
            }
            return allJoined;
        }

        //alte Version:
        //public List<string> findMatchOfTwoUsers(List<string> genresUser1, List<string> genresUser2)
        //{
        //    List<string> matchingGenres = new List<string>();
        //    if (genresUser1.Any() && genresUser2.Any())
        //    {
        //        foreach (string genre1 in genresUser1)
        //        {
        //            foreach (string genre2 in genresUser2)
        //            {
        //                if (genre1.Equals(genre2))
        //                {
        //                    matchingGenres.Add(genre1);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Genre muss für beide User gefüllt sein!");
        //    }

        //    return matchingGenres;
        //}
    }
}
