using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher2
{
    class Program
    {
        // Le patern a remplacer
        const string PATTERN = @"http://%s.patch.battle.net:1119/%s/";

        // La chaine qui va remplacer le pattern
        private const string REPLACEMENT = @"wow-version.appspot.com/19865/";

        // Fonctions basées sur http://stackoverflow.com/a/283648
        public static int Locate(IList<byte> self, IList<byte> candidate, int startIndex = 0)
        {
            if (IsEmptyLocate(self, candidate))
                return -1;

            for (var i = startIndex; i < self.Count; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                return i;
            }

            return -1;
        }

        static bool IsMatch(IList<byte> array, int position, IList<byte> candidate)
        {
            if (candidate.Count > (array.Count - position))
                return false;

            return !candidate.Where((t, i) => array[position + i] != t).Any();
        }

        static bool IsEmptyLocate(ICollection<byte> array, ICollection<byte> candidate)
        {
            return array == null
                || candidate == null
                || array.Count == 0
                || candidate.Count == 0
                || candidate.Count > array.Count;
        }

        static int EndOfStringIndex(IList<byte> array, int startIndex = 0)
        {
            var i = 0;
            while (array[startIndex + i] != 0)
            {
                i += 1;
            }
            return i;
        }

        static void Main(string[] args)
        {
            var wowPath = args.Length > 0 ? args[0] : "Wow.exe";
            //charge les données de l'executable 
            var programBytes = File.ReadAllBytes(wowPath);

            var patternByte = Encoding.ASCII.GetBytes(PATTERN);

            //recherche le pattern dans les bytes du programme
            var location = Locate(programBytes, patternByte);

            if (location == -1)
            {
                Console.WriteLine("Impossible de patcher, le pattern na pas été trouvé");
                return;
            }

            var replacement = args.Length > 1 ? args[1] : REPLACEMENT;
            var replacementByte = Encoding.ASCII.GetBytes(replacement);

            if (replacementByte.Length > patternByte.Length)
            {
                Console.WriteLine("Impossible de patcher, chaine de remplacement trop longue.");
                return;
            }

            while (location > - 1)
            {
                var end = EndOfStringIndex(programBytes, location);
                var remainingByte = end - patternByte.Length;
                //remplacement du pattern
                Array.Copy(replacementByte, 0, programBytes, location, replacementByte.Length);
                //deplacement des bytes restants
                Array.Copy(programBytes, location + patternByte.Length, programBytes, location + replacementByte.Length, remainingByte);

                //taille totalle de la nouvelle chaine
                var replacementLength = remainingByte + replacementByte.Length;

                //Met des "\0" a la fin
                for (var i = 0; i < patternByte.Length - replacementByte.Length; i++)
                {
                    programBytes[location + replacementLength + i] = 0;
                }
                // Cherche à nouveau le pattern
                location = Locate(programBytes, patternByte, location + replacementLength);
            }

            //Enregistre le nouvel executable
            File.WriteAllBytes("WowP2.exe", programBytes);
            Console.WriteLine("Patch ok !");
        }
    }
}
