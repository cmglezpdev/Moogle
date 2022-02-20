using System.Text;
namespace MoogleEngine;

public static class AuxiliarMethods{


    // Extraer todas las palabras de una oracion
    public static string[] GetWordsOfSentence(string sentence) {

        string[] aux = sentence.Split(' ');
        int n = aux.Length;
        List<string> words = new List<string> ();   

        for(int i = 0; i < n; i ++) {

            if(aux[i] == "") continue;

            int l = 0, r = aux[i].Length - 1;
            
            while( l < aux[i].Length && Ignore(aux[i][l]) ) l++;
            while( r >= 0 && Ignore(aux[i][r]) ) r--;

            if(l == aux[i].Length || r < 0) continue;

            words.Add(aux[i].Substring(l, r - l + 1));
        }

        return words.ToArray();
    }
    
      public static string FormatQuery(string q) {
        string query = q.ToLower();

        string[] partsOfQuery = query.Split(' ');   
        int n = partsOfQuery.Length;

        StringBuilder format = new StringBuilder(partsOfQuery[0]);
        for(int i = 1; i < n; i ++) {
            int len = partsOfQuery[i - 1].Length;

            if( (Char.IsLetterOrDigit(partsOfQuery[i - 1][len - 1]) && Char.IsLetterOrDigit(partsOfQuery[i][0])) ||
              (Char.IsLetterOrDigit(partsOfQuery[i - 1][len - 1]) && WorkingOperators.IsOperator(partsOfQuery[i][0])) ||
              (WorkingOperators.IsOperator(partsOfQuery[i - 1][len - 1]) && Char.IsLetterOrDigit(partsOfQuery[i][0])) ) {
                format.Append(" " + partsOfQuery[i]);
                continue;
            }

            format.Append(partsOfQuery[i]);
        }

        for(int i = 1; i < format.Length; i ++) {
            // Si es un signo de puntuacion lo elimino e inserto un espacio en blanco
            if(Char.IsPunctuation(format[i]) && !WorkingOperators.IsOperator(format[i])) {
                format.Remove(i, 1);
                format.Insert(i, ' ');
            }

            if(Char.IsLetterOrDigit(format[i - 1]) && WorkingOperators.IsOperator(format[i])) {
                format.Insert(i, ' ');
            }

        }

        // Remplazar los operadores por su equivalente ya validado
        partsOfQuery = format.ToString().Split(' ');
        n = partsOfQuery.Length;

        for(int i = 0; i < n; i ++) {
            string part = partsOfQuery[i];
            if(!IsLineOperators(part)) continue;
            format.Replace(part, ValidOperators(part));
        }


        for(int i = 1; i < format.Length; i ++) {
            // Eliminar si hay dos espacios en blanco adyacentes
            if(Char.IsWhiteSpace(format[i]) && Char.IsWhiteSpace(format[i - 1])) {
                format.Remove(i, 1);
                i --;
            }
        }


        return format.ToString();
    }
    
    public static bool IsWord(string l) {
        foreach(char c in l) {
            if(!Char.IsLetterOrDigit(c))
                return false;
        }
        return true;
    }

    public static bool IsLineOperators(string l) {
        if(l == "") return false;

        foreach(char c in l) {
            if(!WorkingOperators.IsOperator(c))
                return false;
        }
        return true;
    }


    public static bool Ignore(char x) {
        // return Char.IsPunctuation(x) || WorkingOperators.IsOperator(x) || Char.IsWhiteSpace(x);
        return !Char.IsLetterOrDigit(x);
    }

    // Comprobar que una linea del fichero es o no una linea en blanco
    public static bool IsLineWhite(string line) {
        for(int i = 0; i < line.Length; i ++)
            if(!Char.IsWhiteSpace(line[i])) 
                return false;
        return true;
    }
    public static string NormalizeWord(string word) {
        string w = word.ToLower();
        string newWord = "";
        foreach(char caracter in w) {
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
                case 'ü':
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

    public static void Swap(ref int a, ref int b) {
        int aux = a;
        a = b;
        b = aux;
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
        if(op == "") return "";

        // Sort String
        char[] aux = new char[op.Length];
        for(int i = 0; i < op.Length; i ++)
            aux[i] = op[i];
        Array.Sort(aux);

        op = "";
        for(int i = 0; i < aux.Length; i ++)
            op += aux[i];
        // End Sort String

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