// using System.Text;
namespace MoogleEngine;

public class AuxiliarMethods{

    public static string[] getWordsOfSentence(string sentence) {

        List<string> words = new List<string>();

        int n = sentence.Length;
        for(int i = 0; i < n; ) {
            if(sentence[i] == ' ') continue;
            int endWord = i;
            while(sentence[endWord ++] != ' '); endWord --;
            if(endWord - i + 1 == 1) continue; // No procesar las palabras de tamano menor a 3
            
            words.Add(sentence.Substring(i, endWord - i + 1));
            i = endWord;
        }

        return words.ToArray();
    }
    private static int CountWordsInName(string name, string[] words) {
        string[] wordsName = getWordsOfSentence(name);
        int count = 0;
        foreach(string i in wordsName)
            foreach(string j in words)
                if(i == j) count ++;
        return count;
    }
    public static void MergeSort(ref Files[] files, ref string[] words) {
         files = Sort(files, words, 0, files.Length - 1);
    }
    private static Files[] Sort(Files[] files, string[] words, int l, int r) {
        if(l == r) {
            Files[] sol = new Files[1];
            sol[0] = new Files(files[l].Name, files[l].Path);
            return sol;
        }
        
        // Dividimos el array en dos y ordenamos recursivamente
        int middle = (l + r) / 2;
        // La primera mitad del array
        Files[] LeftFiles = Sort(files, words, l, middle);
        // La segunda mitad del array
        Files[] RightFiles = Sort(files, words, middle, r);
        // Mezclamos los dos Arrays
        return Merge(LeftFiles, RightFiles, words);
    }
    private static Files[] Merge(Files[] LeftFiles, Files[] RightFiles, string[] words) {
       
        int n = LeftFiles.Length, m = RightFiles.Length;
        Files[] union = new Files[n + m];
        int l = 0, r = 0, u = 0;

        while(l < n || r < m) {
            if(l == n) {
                union[u ++] = LeftFiles[l ++];
                continue;
            }
            if(r == m) {
                union[u ++] = RightFiles[r ++];
                continue;
            }
            
            if(CountWordsInName(LeftFiles[l].Name, words) <= CountWordsInName(RightFiles[r].Name, words))
                union[u ++] = LeftFiles[l ++];
            else union[u ++] = RightFiles[r ++];
        }
        return union;
    }

}