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




public static class Moogle
{
    public static SearchResult Query(string query)
    {

        string[] files = FilesMethods.ReadFolder();
        int TotalFiles = files.Length;

        // WordsDocs[i][j] son las repeticiones de la palabra j en el documento i
        List<List<info>> WordsDocs = new List<List<info>>();
        // Ver que palabras hay entre todos los documentos
        Dictionary<int, int> DocsW = new Dictionary<int, int>();
        // Lista de las palabras que hay en el documento
        List<pair> WordsOfDocs = new List<pair>();

        // Redimensionar WordsDocs a la cantidad de archivos
        for(int doc = 0; doc < TotalFiles; doc ++)  
            WordsDocs.Add(new List<info>());

        for (int doc = 0; doc < TotalFiles; doc ++) {
            FilesMethods.ReadContentFile(files[doc], doc, ref DocsW, ref WordsDocs, ref WordsOfDocs);
        }

        // Redimensionar todos las listas a su maximo
        for(int doc = 0; doc < TotalFiles; doc ++) {
            AuxiliarMethods.Resize(ref WordsDocs, WordsDocs[TotalFiles - 1].Count - WordsDocs[doc].Count, doc);
            // System.Console.WriteLine(WordsDocs[doc].Count);
        }

        string[] WordsQuery = AuxiliarMethods.GetWordsOfSentence(query);



        // Matrix de peso de cada termino en cada documento

        int maxl = info.MaxFreq(ref WordsDocs);


        float[,] weigthDocs = new float[TotalFiles, WordsOfDocs.Count];
        for(int doc = 0; doc < TotalFiles; doc ++) {
            for(int w = 0; w < WordsOfDocs.Count; w ++) {
                weigthDocs[doc, w] = info.TFIDF(doc, w, ref WordsDocs);
            }
        }

        float[] weigthQuery = new float[WordsOfDocs.Count];
        for(int i = 0; i < WordsQuery.Length; i ++) {
            int w = 0;
            for(int j = 0; j < WordsDocs.Count; j ++) 
                if(WordsDocs[j].)
        }













        SearchItem[] items = { 
            new SearchItem("Titulo", "holaaa", 0.5f),
            new SearchItem("Titulo#2", "HELLO", 0.2f)
        };

        return new SearchResult(items, query);
    }
}


