// using System.Text;
namespace MoogleEngine;

public class AuxiliarMethods{


    // Extraer todas las palabras de una oracion
    public static string[] getWordsOfSentence(string sentence) {

        List<string> words = new List<string>();

        int n = sentence.Length;
        for(int i = 0; i < n; ) {
            if(Ignore(sentence[i])) continue;
            int endWord = i;
            while(sentence[endWord ++] != ' '); endWord --;
            words.Add(sentence.Substring(i, endWord - i + 1));
            i = endWord;
        }

        return words.ToArray();
    }

    // Ignorar todos los caracteres que no sean letras o numeros
    private static bool Ignore(char x) {
        return !Char.IsLetterOrDigit(x);
    }


    // Comprobar que una linea del fichero es o no una linea en blanco
    public static bool IsLineWhite(string line) {
        for(int i = 0; i < line.Length; i ++)
            if(!Char.IsWhiteSpace(line[i])) 
                return false;
        return true;
    }


    // Devuelve el hash de una palabra
    public static int GetHash(string word) {
        int MOD = 1000000007; // Primo para modular el hash
        return word.GetHashCode() % MOD;
    } 



}