namespace MoogleEngine;
public class WordInfo{
  
    public struct info{
        int numberLine = 0; // Linea del documento en la que esta
        int numberWord = 0; // posicion la linea en la que esta
        public info(int numberLine, int numberWord){
            this.numberLine = numberLine;
            this.numberWord = numberWord;
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


    public List<info>[] InfoWord{
        get{return this.InfosWord;}
    }
}