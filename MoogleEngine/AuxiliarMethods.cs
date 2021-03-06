using System.Text;
namespace MoogleEngine;

public static class AuxiliarMethods{

    //! Extraer todas las palabras de una oracion
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
    
    //! Formatear la Query convenientemente para trabajar mejor con ella
    public static string FormatQuery(string q) {
        string query = q.ToLower();

        string[] aux = query.Split(' ');
        string[] partsOfQuery = new string[aux.Length];
        
        int n = 0;
        // Eliminar las cadenas vacias que me deja el Split
        for(int i = 0; i < aux.Length; i ++) {
            if(aux[i] == "") continue;
            partsOfQuery[n ++] = aux[i];
        }   
        Array.Resize(ref partsOfQuery, n);

        StringBuilder format = new StringBuilder(partsOfQuery[0]);
        // Insertar y eliminar los espacios en blanco donde van
        for(int i = 1; i < n; i ++) {
            int lastPos = partsOfQuery[i - 1].Length - 1;

            if( (Char.IsLetterOrDigit(partsOfQuery[i - 1][lastPos]) && Char.IsLetterOrDigit(partsOfQuery[i][0])) ||
              (Char.IsLetterOrDigit(partsOfQuery[i - 1][lastPos]) && WorkingOperators.IsOperator(partsOfQuery[i][0])) ||
              (WorkingOperators.IsOperator(partsOfQuery[i - 1][lastPos]) && Char.IsLetterOrDigit(partsOfQuery[i][0])) ) {
                format.Append(" " + partsOfQuery[i]);
                continue;
            }

            format.Append(partsOfQuery[i]);
        }

        // cambiar los signos de puntuacion por un espacio e insetar espacion en donde los necesite
        for(int i = 1; i < format.Length; i ++) {
            if(Char.IsPunctuation(format[i]) && !WorkingOperators.IsOperator(format[i])) {
                format.Remove(i, 1);
                format.Insert(i, ' ');
            }

            if(Char.IsLetterOrDigit(format[i - 1]) && WorkingOperators.IsOperator(format[i])) {
                format.Insert(i, ' ');
            }
            if(WorkingOperators.IsOperator(format[i - 1]) && Char.IsLetterOrDigit(format[i])) {
                format.Insert(i, ' ');
            }

        }

        // Remplazar los operadores por su equivalente ya validado
        partsOfQuery = format.ToString().Split(' ');
        n = partsOfQuery.Length;

        for(int i = 0; i < n; i ++) {
            string part = partsOfQuery[i];
            if(!IsLineOperators(part)) continue;
            format.Replace(part, WorkingOperators.ValidOperators(part));
        }


        // Eliminar si hay dos espacios en blanco adyacentes
        for(int i = 1; i < format.Length; i ++) {
            if(Char.IsWhiteSpace(format[i]) && Char.IsWhiteSpace(format[i - 1])) {
                format.Remove(i, 1);
                i --;
            }
        }
        if(format[ format.Length - 1 ] == ' ') format.Remove(format.Length - 1, 1);

        return format.ToString();
    }

    //! True si el string es una cadena de operadores
    public static bool IsLineOperators(string l) {
        if(l == "") return false;

        foreach(char c in l) {
            if(!WorkingOperators.IsOperator(c))
                return false;
        }
        return true;
    }


    //! True si el caracter no pertenece a una palabra
    public static bool Ignore(char x) {
        return !Char.IsLetterOrDigit(x);
    }
   
    //! Comprobar que una linea del fichero es o no una linea en blanco
    public static bool IsLineWhite(string line) {
        for(int i = 0; i < line.Length; i ++)
            if(!Char.IsWhiteSpace(line[i])) 
                return false;
        return true;
    }
   
    //! Llevar una palabra a minusculas y sin tildes
    public static string NormalizeWord(string word) {
        string w = word.ToLower();
        string newWord = "";
        foreach(char caracter in w) {
            switch( caracter ) {
                case '??':
                    newWord += 'a';
                    break;
                case '??':
                    newWord += 'e';
                    break;
                case '??':
                    newWord += 'i';
                    break;
                case '??':
                    newWord += 'o';
                    break;
                case '??':
                    newWord += 'u';
                    break;         
                default: 
                    newWord += caracter;
                    break;

            }
        }

        return newWord;
    }
   
   //! La palabra que empieza a partir de esa posicion
    public static string GetWordStartIn(string sentence, int start) {
        if(start >= sentence.Length)
            throw new Exception("Posicion no valida");
        if(Ignore(sentence[start]))
                return ""; // No es una posicion valida para comenzar una palabra
        int end;
        for(end = start; end < sentence.Length && !Ignore(sentence[end]); end ++);
        
        return sentence.Substring(start, end - start);
    }
    
    //! La palabra que finaliza en una posicion
    public static string GetWordEndIn(string sentence, int end) {
         if(end >= sentence.Length)
            throw new Exception("Position not valid!!");
        if(Ignore(sentence[end]))
                return ""; // No es una posicion valida para terminar una palabra
        int start;
        for(start = end; start >= 0 && !Ignore(sentence[start]); start --);
        
        return sentence.Substring(start + 1, end - start);
    }

    // !La acantidad de documentos en el que aparece la palabra
    public static int AmountAppareanceOfWordBetweenAllFiles(string word) {
        int count = 0;
        for(int i = 0; i < Data.TotalFiles; i ++) {
            if(Data.PosInDocs[i].ContainsKey(word)) count ++;
        }
        
        return count;   
    }

    //! True si la palabra esta entre los documentos
    public static bool IsWordInDocs(string word) {
        for(int i = 0; i < Data.TotalFiles; i ++) {
            if(Data.PosInDocs[i].ContainsKey(word))
                return true;
        }
        return false;
    }
    
    //! Algoritmo de Levenshtein
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