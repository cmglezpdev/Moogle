// using System.Text;
namespace MoogleEngine;

public static class AuxiliarMethods{


    // Extraer todas las palabras de una oracion
    public static string[] GetWordsOfSentence(string sentence) {

        List<string> words = new List<string>();

        int n = sentence.Length;
        for(int i = 0; i < n; i ++) {
            if(Ignore(sentence[i])) continue;
            int endWord = i;
            while(++ endWord < n && !Ignore(sentence[endWord]));
            words.Add( NormalizeWord(sentence.Substring(i, endWord - i)));
            i = endWord - 1;
        }

        return words.ToArray();
    }
    public static bool Ignore(char x) {
        return Char.IsPunctuation(x) || IsOperator(x) || Char.IsWhiteSpace(x);
    }

    // Comprobar que una linea del fichero es o no una linea en blanco
    public static bool IsLineWhite(string line) {
        for(int i = 0; i < line.Length; i ++)
            if(!Char.IsWhiteSpace(line[i])) 
                return false;
        return true;
    }
    public static string NormalizeWord(string word) {
        word.ToLower();
        
        // Falta quitar las tildes
        string newWord = "";
        foreach(char caracter in word) {
            switch( caracter ) {
                case 'á':
                    newWord += 'a';
                    break;
                case 'é':
                    newWord += 'e';
                    break;
                case 'í':
                    newWord += 'i';
                    break;
                case 'ó':
                    newWord += 'o';
                    break;
                case 'ú':
                    newWord += 'u';
                    break;
                
                default: 
                    newWord += caracter;
                    break;

            }
        }

        return newWord;
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
            throw new Exception("Position not valid!!");
        if(Ignore(sentence[end]))
                return ""; // No es una posicion valida para terminar una palabra
        int start;
        for(start = end; start >= 0 && !Ignore(sentence[start]); start --);
        
        return sentence.Substring(start + 1, end - start);
    }
   public static void Resize(List< List<info> > aux, int idFile, int newLength) {
        for(int i = 0; i < newLength; i ++)
            aux[idFile].Add(new info());
    }
    public static string GetWord(string sentence, int pos, string direction) {
        string word = "";
        int n = sentence.Length;

        switch( direction ) {
            case "right":
                for(int i = pos; i < n && !AuxiliarMethods.Ignore(sentence[i]); i ++)
                    word += sentence[i];
                break;
            
            case "left":
                for(int i = pos; i >= 0 && !AuxiliarMethods.Ignore(sentence[i]); i --)
                    word = sentence[i] + word;
                break;
        }

        return word;
    }
    // Devuelve vacio si no es valida, y en otro caso simplifica la expresion
    public static string ValidOperators(string op) {
        // Si son operadores simples
        if(op == "!") return op;
        if(op == "^") return op;
        if(op == "*") return op;
        if(op == "~") return op;

        // Si todos son iguales 
        bool allEquival = true;
        for(int i = 1; i < op.Length; i ++)
            if(op[i] != op[i - 1]) {
                allEquival = false;
                break;
            }
            
        if(allEquival == true) {
            if(op[0] != '*') return op[0].ToString();
            return op;
        }

        // Si entre los operadores aparece ~ entoces no es valido
        for(int i = 1; i < op.Length; i ++) 
            if(op[i] == '~') return "";
        
        // Si aparece ! entoces los demas operadores no importan
        for(int i = 0; i < op.Length && op[0] != '~'; i ++)
            if(op[i] == '!') return "!";

        return op;
    }
    public static int LevenshteinDistance(string a, string b) {

        int n = a.Length,
            m = b.Length;

        int[,] dp = new int[n + 1, m + 1];

        // El coste de convertir una palabra vacia a una de i caracteres tiene un coste de i pasos
        for(int i = 0; i <= n; i ++) dp[i, 0] = i;
        for(int i = 0; i <= m; i ++) dp[0, i] = i;
    

        for(int i = 1; i <= n; i ++) {
            for(int j = 1; j <= m; j ++) {
                // Si los caracteres son iguales entonces no hay coste
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min( dp[i - 1, j] + 1, // Eliminacion
                            Math.Min(dp[i, j - 1] + 1, // Insersion
                            dp[i - 1, j - 1] + cost) );  // Sustitucion
            }
        }

        return dp[n, m];
    }

}