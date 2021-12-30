using System.Text;
namespace MoogleEngine;


public class FilesMethods {

    string[] FILES = new string[0];

    public FilesMethods() {
        string[] aux = ReadFolder();
        Array.Resize(ref this.FILES, aux.Length);
        Array.Copy(aux, this.FILES, aux.Length);
    }
    public static string GetNameFile(string file) {
        int StartName = file.Length - 1;
        for( ; StartName >= 0 && file[StartName] != '/'; StartName --); StartName ++;

        return file.Substring(StartName, file.Length - StartName - 4);
    }
    public static string[] ReadFolder() {
        // Leer todos los archivos .txt de la carpeta Content
        string[] files = Directory.GetFiles(@"../Content/", "*.txt", SearchOption.AllDirectories);
        return files;
    }
    public static int GetTotalFiles() {
        return ReadFolder().Length;
    }
    public static void ReadContentFile(string file, int idFile, ref Dictionary<int, int> IdxWords, ref List<List<info>> PosInDocs ) {
        
        // Reservar las palabras que ya estan desde los ficheros pasados
        int n = PosInDocs[Math.Max(0, idFile - 1)].Count; // palabras hasta el fichero anterior
        for(int i = 0; i < n; i ++) 
            PosInDocs[idFile].Add(new info());


        StreamReader archive = new StreamReader(file);
        
        int numLine = 0;
        for(string line = archive.ReadLine(); line != null; line = archive.ReadLine(), numLine ++){
           
            if(AuxiliarMethods.IsLineWhite(line)) continue;
            
            string[] words = AuxiliarMethods.GetWordsOfSentence(line);
            for(int i = 0; i < words.Length; i ++) {
                int hash = AuxiliarMethods.GetHashCode(words[i].ToLower());
                // Si la palabra ya existe de los ficheros anteriores 
                if(IdxWords.ContainsKey(hash)) {
                    // Anadimos una nueva aparicion de la palabra en IdFile y en
                    // la posicion reservada que tiene esa palabra en idFile
                    PosInDocs[idFile][ IdxWords[hash] ].AddAppearance(numLine, i);
                    continue;
                }
                // Sino creamos una nueva posicion con esa palabra en idfFile
                int newPos = PosInDocs[idFile].Count;
                PosInDocs[idFile].Add(new info());
                PosInDocs[idFile][ newPos ].AddAppearance(numLine, i);
                // El nuevo indice es la ultima posicion vacia de la lista de palabras
                IdxWords[hash] = newPos;
            }
        }

        archive.Close();
    } 
    public string GetFileByID(int idFile) {
        if(idFile >= this.FILES.Length) 
            throw new Exception("The File does't exists!");
        return this.FILES[idFile];
    }
    public static int GetAmountWordsInSentence(string line) {
        return AuxiliarMethods.GetWordsOfSentence(line).Length;
    }
    public static string GetLeftContext(int idFile, int numLine, int numWord, int length, bool addWord) { 
        //todo:: Tanto numLine como numWord empieza desde cero
        
        StringBuilder context = new StringBuilder();
        FilesMethods files = new FilesMethods();
        StreamReader reader = new StreamReader(files.GetFileByID(idFile));            

        // SI es la primera palabra no hay contexto derecho
        if(numLine == numWord && numWord == 0) return context.ToString();

        // Tomar las lineas por encima de el
        List<string> AuxLines = new List<string>();
        for(int i = 0; i <= numLine; i ++) AuxLines.Add(reader.ReadLine());
        int n = AuxLines.Count;

        // Buscar la posicion en donde empieza la palabra
        int posWord = 0;
        for(int nw = -1; posWord < AuxLines[n - 1].Length; posWord ++) {
            if(AuxiliarMethods.Ignore(AuxLines[n - 1][posWord])) continue;
            if(++ nw == numWord) break;
            int l = AuxiliarMethods.GetWordStartIn(AuxLines[n - 1], posWord).Length;
            posWord += l - 1;
        } 

        // Anadimos la palabra al contexto
        if(addWord) context.Append(AuxiliarMethods.GetWordStartIn(AuxLines[n - 1], posWord));

        // Tomamos las palabras que necesitamos para conformar el contexto izquierdo
        int  nl = numLine;
        while(true) {
            string currLine = AuxLines[ nl ];
            n = (nl == numLine) ? posWord - 1 : currLine.Length - 1;

            for(int i = n; i >= 0 && length > 0; i --) {
                if(AuxiliarMethods.Ignore(currLine[i])){
                    context.Insert(0, currLine[i]);
                    continue;
                }
                string w = AuxiliarMethods.GetWordEndIn(currLine, i);
                length --;
                context.Insert(0, w);
                i-= (w.Length - 1);
            } 
            // nl --;
            if(-- nl >= 0 && length > 0)
                 context.Insert(0, " "); // Espacio en blanco representando el salto de linea
            else break;
        }

        return context.ToString();
    }
    public static string GetRightContext(int idFile, int numLine, int numWord, int length, bool addWord) { 
        //todo:: Tanto numLine como numWord empieza desde cero

        StringBuilder context = new StringBuilder();
        FilesMethods files = new FilesMethods();
        StreamReader reader = new StreamReader(files.GetFileByID(idFile));            

        // Tomar las lineas necesarias que esten por debajo de el
        List<string> AuxLines = new List<string>();
        for(int i = 0, k = 1; k < length; i ++) {
            string line = reader.ReadLine();
          
            if(line == null) break; // Si llego a la ultima linea
            if(i < numLine) continue; // Sui no ha llegado a la linea donde esta la palabra
            AuxLines.Add(line);

            if(i == numLine) continue; // Si es la linea donde esta la palabra
                
            // Anadiremos lineas hasta que se complete el length
            k += GetAmountWordsInSentence(line);
        }


        int n = AuxLines.Count;

        // Buscar la posicion en donde termina la palabra
        int posWord = 0;
        for(int nw = 0; nw <= numWord && posWord < AuxLines[0].Length; posWord ++) {
            if(AuxiliarMethods.Ignore(AuxLines[0][posWord])) continue;
            int l = AuxiliarMethods.GetWordStartIn(AuxLines[0], posWord).Length;
            posWord += l - 1;
            nw ++;
        }

        // Anadimos la palabra al contexto
        if(addWord) context.Append(AuxiliarMethods.GetWordEndIn(AuxLines[0], posWord - 1));

        // Tomamos las palabras que necesitamos para conformar el contexto derecho
        int  nl = 0;
        while(true) {
            string currLine = AuxLines[ nl ];
            n = (nl == 0) ? posWord : 0;

            for(int i = n; i < currLine.Length && length > 0; i ++) {
                if(AuxiliarMethods.Ignore(currLine[i])){
                    context.Append(currLine[i]);
                    continue;
                }
                string w = AuxiliarMethods.GetWordStartIn(currLine, i);
                length --;
                context.Append(w);
                i+= w.Length - 1;
            } 
            if( ++ nl < AuxLines.Count && length > 0)
                context.Append(" ");
            else break;
        }

        return context.ToString();
    }

    public static string GetContext(int idFile, int numLine, int numWord, int length) {
        return GetLeftContext(idFile, numLine, numWord, length, true) + 
               GetRightContext(idFile, numLine, numWord, length, false);
    }


    public static void Print(float[] x) {
        string path = "archive.txt";

        StreamWriter sw = new StreamWriter(path);
        string s = "";
        for(int i = 0; i < x.Length; i ++) {
            s += $"{x[i]} ";
        }
        sw.WriteLine(s);
        sw.Close();
    }


  




}