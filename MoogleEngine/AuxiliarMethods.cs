// using System.Text;
namespace MoogleEngine;

public class AuxiliarMethods{


    // Extraer todas las palabras de una oracion
    public static string[] GetWordsOfSentence(string sentence) {

        List<string> words = new List<string>();

        int n = sentence.Length;
        for(int i = 0; i < n; i ++) {
            if(Ignore(sentence[i])) continue;
            int endWord = i;
            while(++ endWord < n && !Ignore(sentence[endWord]));
            words.Add(sentence.Substring(i, endWord - i));
            i = endWord - 1;
        }

        return words.ToArray();
    }

    // Ignorar los espacios en blanco y los signos de puntuacion
    public static bool Ignore(char x) {
        return Char.IsWhiteSpace(x) || Char.IsWhiteSpace(x);
    }

    // Comprobar que una linea del fichero es o no una linea en blanco
    public static bool IsLineWhite(string line) {
        for(int i = 0; i < line.Length; i ++)
            if(!Char.IsWhiteSpace(line[i])) 
                return false;
        return true;
    }

    // La palabra que empieza a partir de esa posicion
    public static string GetWordStartIn(string sentence, int start) {
        if(start >= sentence.Length)
            throw new Exception("Posicion no valida");
        if(Ignore(sentence[start]))
                return ""; // No es una posicion valida para comenzar una palabra
        int end;
        for(end = start; end < sentence.Length && !Ignore(sentence[end]); end ++);
        
        return sentence.Substring(start, end - start);
    }
    // La palabra que finaliza en una posicion
    public static string GetWordEndIn(string sentence, int end) {
         if(end >= sentence.Length)
            throw new Exception("Posicion no valida");
        if(Ignore(sentence[end]))
                return ""; // No es una posicion valida para terminar una palabra
        int start;
        for(start = end; start >= 0 && !Ignore(sentence[start]); start --);
        
        return sentence.Substring(start + 1, end - start);
    }


    public static void Resize(ref List< List<info> > aux, int idFile, int newLength) {
        for(int i = 0; i < newLength; i ++)
            aux[idFile].Add(new info());
    }

    public static int Equival(float l, float r) {
        float eps = 0.000000001f;
        if( l - r <= eps ) return 0; // Son iguales
        if( r - l > eps ) return 1; // l < r
        return -1; // r < l
    }

    public static int GetHashCode(string w) {
        int MOD = 1000000007; // 10^9 + 7
        return w.GetHashCode() % MOD;
    }


}