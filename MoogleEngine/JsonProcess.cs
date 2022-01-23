namespace MoogleEngine;

public class JsonProcess {
    
    // Leer las palabras que estan entre comillas
    public static string[] GetWordsOfLine_JSON(string line) {
        List<string> words = new List<string>();
        int n = line.Length;

        for(int i = 0; i < n; i ++) {
            if( line[i] != '"' ) continue;

            string w = "";
            int k = i + 1;
            while(k < n && line[k] != '"') w += line[k ++]; k --;

        }



        return words.ToArray();
    }


}