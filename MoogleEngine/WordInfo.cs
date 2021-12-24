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
    int HashCode = 0; // Guardar el valor hash de la palabra
    List<info>[] InfosWord = new List<info>[0]; // Info de la palabra en cada uno de los archivos
   
    public WordInfo(string Word) { 
        this.Word = Word;
        this.HashCode = AuxiliarMethods.GetHash(Word);
        Array.Resize(ref this.InfosWord, FilesMethods.GetTotalFiles());
    }


    public void AddAppearance(int  idFile, int numLine, int numWord) {
        InfosWord[idFile].Add(new info(numLine, numWord));
    }
    public void Clone(WordInfo other) {
        this.Word = other.Word;
        this.HashCode = other.HashCode;
        this.InfosWord = other.InfosWord;
    }
    public List<info> InfoWordInDoc(int idFile) {
        if(idFile > InfosWord.Length) 
            throw new Exception("The File does't exists!");
        return InfosWord[idFile];
    }

    public string GetContext(int idFile, int radioLength) {
        Random rand = new Random();
        // Escoger una aparicion random en el archivo para mostrar su contexto
        int appaerance = rand.Next() % this.TF(idFile);
        
        info infoPos = this.InfosWord[idFile][appaerance];
    
        string context = FilesMethods.getLeftContext(idFile, infoPos.NumberLine, infoPos.NumberWord, radioLength)
                      + FilesMethods.getRightContext(idFile, infoPos.NumberLine, infoPos.NumberWord, radioLength);
        
        return context;
    }



    #region Calculo del TFIDF

    // Termination Frecuency
    public int TF(int idFile) {
        return this.InfosWord[idFile].Count;
    }
    // Document Frecuency
    public int DF{
        get{
            int cont = 0;
            foreach(List<info> i in InfosWord) {
                if(i.Count > 0) cont ++;
            }
            return cont;
        }
    }
    // Inverse Document Frecuency 
    public float IDF{
        get{
            int N = InfosWord.Length;
            return (float)Math.Log(N / DF);
        }
    }
    
    public float IFIDF(int idFile) {
        float tf = this.TF(idFile);
        float idf = this.IDF;
        return tf * idf;
    }
   
    #endregion


}