namespace MoogleEngine;
public class WordInfo{

    public struct WordContext {
        int[] left = new int[0];
        int[] right = new int[0];
        
        public WordContext(){}
        public WordContext(int[] left, int[] right){
            Array.Resize(ref this.left, left.Length);
            Array.Resize(ref this.right, right.Length);
            Array.Copy(left, this.left, left.Length);
            Array.Copy(right, this.right, right.Length);
        }
    
        public int[] Context{
            get{
                int[] context = new int[left.Length + right.Length + 1];
                for(int i = 0; i < left.Length; i ++) context[i] = left[i];
                context[left.Length] = -1;
                for(int i = 0, k = left.Length + 1; i < right.Length; i ++, k ++)
                    context[k] = right[i];

                return context;
            }
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    public struct info{
        int numberLine = 0; // Linea del documento en la que esta
        int numberWord = 0; // posicion la linea en la que esta
        WordContext context = new WordContext();

        public info(int numberLine, int numberWord, WordContext context){
            this.numberLine = numberLine;
            this.numberWord = numberWord;
            this.context = context;
        }
    
        public int[] Context {
            get{
                return context.Context;
            }
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


    public void AddAppearance(int  idFile, int numLine, int numWord, int[] LeftContext, int[] RightContext) {
        WordContext currContext = new WordContext(LeftContext, RightContext);
        InfosWord[idFile].Add(new info(numLine, numWord, currContext));
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