namespace MoogleEngine;
using System.Diagnostics;

// public class BuildDictionary {

//     private Dictionary<string, WordInfo> DocsInfos = new Dictionary<string, WordInfo>();
//     private List<int> WordsOfDocs = new List<int>();
//     public BuildDictionary(bool build) {
//         if(!build) return;
//         string[] files = FilesMethods.ReadFolder();
//         // Sacar la informacion de cada documento
//         for(int i = 0; i < files.Length; i ++) 
//             FilesMethods.ReadContentFile(files[i], i, ref this.DocsInfos, ref this.WordsOfDocs);
//     }

//     public BuildDictionary() {}

//     public Dictionary<string, WordInfo> Infos{
//         get{return this.DocsInfos;}
//     }

// }

    public struct pair{
        string w;
        int idx, hash;
        public pair(string word, int idx) {
            this.w = word;
            this.idx = idx;
            this.hash = AuxiliarMethods.GetHashCode(word);
        }
        public string Word{
            get{return this.w;}
        }
        public int Index{
            get{return this.idx;}
        }
        public int Hash{
            get{return this.hash;}
        }
    }

    public struct two{
        int x, y;
        public two(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public int X{
            get{return this.x;}
        }
        public int Y{
            get{return this.y;}
        }
    }



public static class Moogle
{
    public static SearchResult Query(string query)
    {

        string[] files = FilesMethods.ReadFolder();
        int TotalFiles = files.Length;

        // WordsDocs[i][j] son las repeticiones de la palabra j en el documento i
        List<List<info>> DWordsDocs = new List<List<info>>();
        // Ver que palabras hay entre todos los documentos
        Dictionary<int, int> DocsW = new Dictionary<int, int>();
        // Lista de las palabras que hay en el documento
        List<pair> LWordsOfDocs = new List<pair>();

        // Redimensionar WordsDocs a la cantidad de archivos
        for(int doc = 0; doc < TotalFiles; doc ++)  
            DWordsDocs.Add(new List<info>());

        // Guardar todas las palabras en una lista y ...
        // crear un diccionario con todas las posiciones de las palabras en cada documento
        for (int doc = 0; doc < TotalFiles; doc ++)
            FilesMethods.ReadContentFile(files[doc], doc, ref DocsW, ref DWordsDocs, ref LWordsOfDocs);     
        int TotalWords = LWordsOfDocs.Count;

        // Redimensionar todos las listas a su maximo
        for(int doc = 0; doc < TotalFiles; doc ++) 
            AuxiliarMethods.Resize(ref DWordsDocs, DWordsDocs[TotalFiles - 1].Count - DWordsDocs[doc].Count, doc);





        








        // Matrix de peso de cada documento
        float[,] weigthDocs = new float[TotalFiles, TotalWords];
        for(int doc = 0; doc < TotalFiles; doc ++) {          
            int MaxFreq = 0;
            for(int i = 0; i < TotalWords; i ++ )
                 MaxFreq = Math.Max(MaxFreq, DWordsDocs[doc][i].AmountAppareance);
            
            for(int w = 0; w < TotalWords; w ++) {
                weigthDocs[doc, w] = info.TFIDF(doc, w, MaxFreq, ref DWordsDocs);
            }
        }

        // Frecuencia de cada palabra de la query
        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);
        Dictionary<int, int> FreqWordsQuery = new Dictionary<int, int> ();
        foreach(string w in WordsQuery) {
            if(!FreqWordsQuery.ContainsKey(AuxiliarMethods.GetHashCode(w)))
                FreqWordsQuery[ AuxiliarMethods.GetHashCode(w) ] = 0;
            FreqWordsQuery[ AuxiliarMethods.GetHashCode(w) ] ++;
        }


        // Calcular el vector peso que representa la Query
        float[] weigthQuery = new float[TotalWords];
        for(int wd = 0; wd < TotalWords; wd ++) {
            for(int wq = 0; wq < WordsQuery.Length; wq ++) {
                // Si la palabra de la query coincide con alguna
                if(AuxiliarMethods.GetHashCode(WordsQuery[wq]) == LWordsOfDocs[wd].Hash)
                    weigthQuery[wd] = info.TFIDF2(LWordsOfDocs[wd].Hash, ref FreqWordsQuery);
            }
        }


        // Calcular el rank(similitud entre la query u los documentos) entre documentos
        float[] sim = new float[TotalFiles];
        for(int i = 0; i < TotalFiles; i ++) {
            float[] auxWeigthDocs = new float[TotalWords];
            for(int j = 0; j < TotalWords; j ++)
                auxWeigthDocs[j] = weigthDocs[i, j];

            sim[i] = info.Sim(ref auxWeigthDocs, ref weigthQuery);
        }


        
        // Lista de palabras de la query(sin repeticiones)
        int[] WQuery = new int[FreqWordsQuery.Count];
        int t = 0;
        foreach(KeyValuePair<int, int> wq in FreqWordsQuery) {
            WQuery[t ++] = wq.Key;
        }


        for(int doc = 0; doc < TotalFiles; doc ++) {
            if(sim[doc] == 0.00f) continue;

            Random rand = new Random();
            while( true ) {
                int pos = rand.Next() % WQuery.Length;
                // if(  )
            }
        }




















        SearchItem[] items = { 
            new SearchItem("Titulo", "holaaa", 0.5f),
            new SearchItem("Titulo#2", "HELLO", 0.2f)
        };

        return new SearchResult(items, query);
    }
}


