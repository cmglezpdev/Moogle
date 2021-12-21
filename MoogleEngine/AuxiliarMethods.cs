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

}