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
            words.Add(sentence.Substring(i, endWord - i + 1));
            i = endWord;
        }

        return words.ToArray();
    }

}