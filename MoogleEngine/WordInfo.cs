namespace MoogleEngine;
public class WordInfo{
  
    public struct info{
        int numberLine = 0; // Linea del documento en la que esta
        int numberWord = 0; // posicion la linea en la que esta
        public info(int numberLine, int numberWord){
            this.numberLine = numberLine;
            this.numberWord = numberWord;
        }

        public int NumberLine{
            get{return this.numberLine;}
        }
        public int NumberWord{
            get{return this.numberWord;}
        }
    
    }

    string Word = ""; // string que representa la palabra
    List<List<info>> InfosWord = new List<List<info>>(); // Info de la palabra en cada uno de los archivos
   
    public WordInfo(string Word) { 
        this.Word = Word;
        int n = FilesMethods.GetTotalFiles();
        for(int i = 0; i < n; i ++)
            this.InfosWord.Add(new List<info>());
    }


    public void AddAppearance(int  idFile, int numLine, int numWord) {
        InfosWord[idFile].Add(new info(numLine, numWord));
    }
    public void Clone(WordInfo other) {
        this.Word = other.Word;
        this.InfosWord = other.InfosWord;
    }
    public List<info> InfoWordInDoc(int idFile) {
        if(idFile >= InfosWord.Count) 
            throw new Exception("The File does't exists!");
        return InfosWord[idFile];
    }

    public string GetContext(int idFile, int radioLength) {
        Random rand = new Random();
        // Escoger una aparicion random en el archivo para mostrar su contexto
        int appaerance = rand.Next() % this.TF(idFile);
        
        info infoPos = this.InfosWord[idFile][appaerance];
    
        string context = FilesMethods.GetLeftContext(idFile, infoPos.NumberLine, infoPos.NumberWord, radioLength, false)
                      + FilesMethods.GetRightContext(idFile, infoPos.NumberLine, infoPos.NumberWord, radioLength, true);
        
        return context;
    }



    #region Calculo del TFIDF

    // Termination Frecuency
    private int TF(int idFile) {
        return this.InfosWord[idFile].Count;
    }
    // Document Frecuency
    private int DF{
        get{
            int cont = 0;
            foreach(List<info> i in InfosWord) {
                if(i.Count > 0) cont ++;
            }
            return cont;
        }
    }
    // Inverse Document Frecuency 
    private float IDF{
        get{
            int N = InfosWord.Count;
            return (float)(1.00 + Math.Log2((float)N / DF));
        }
    }
    
    public float IFIDF(int idFile) {
        int tf = this.TF(idFile);
        float idf = this.IDF;
        return (float)tf * idf;
    }
   
    #endregion


}