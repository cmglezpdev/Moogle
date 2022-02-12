using System.Text;
namespace MoogleEngine;


public static class FilesMethods {

    public static string GetNameFile(string file) {
        int StartName = file.Length - 1;
        for( ; StartName >= 0 && file[StartName] != '/'; StartName --); StartName ++;
        
        int idxExtention = file.Length - 1;
        while(file[idxExtention --] != '.'); idxExtention ++;

        return file.Substring(StartName, file.Length - StartName - (file.Length - idxExtention));
    }
    public static string[] ReadFolder() {
        // Leer todos los archivos .txt de la carpeta Content
        string[] files = Directory.GetFiles(@"../Content/", "*.txt", SearchOption.AllDirectories);
        return files;
    }
    public static int GetTotalFiles() {
        return ReadFolder().Length;
    } 
    public static void ReadContentFile(string file, int idFile, ref Dictionary<string, int> IdxWords, ref List<List<info>> PosInDocs ) {
        
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
                string word =  words[i].ToLower();
                       word = Lemmatization.Stemmer(word);
                       word = AuxiliarMethods.NormalizeWord(word);

                // Si la palabra ya existe de los ficheros anteriores 
                if(IdxWords.ContainsKey(word)) {
                    // Anadimos una nueva aparicion de la palabra en IdFile y en
                    // la posicion reservada que tiene esa palabra en idFile
                    PosInDocs[idFile][ IdxWords[word] ].AddAppearance(numLine, i);
                    continue;
                }
                // Sino creamos una nueva posicion con esa palabra en idfFile
                int newPos = PosInDocs[idFile].Count;
                PosInDocs[idFile].Add(new info());
                PosInDocs[idFile][ newPos ].AddAppearance(numLine, i);
                // El nuevo indice es la ultima posicion vacia de la lista de palabras
                IdxWords[word] = newPos;
            }
        }

        archive.Close();
    } 
    public static string GetFileByID(int idFile) {
        if(idFile < 0 || idFile >= Data.TotalFiles) 
            throw new Exception("The File does't exists!");
        return Data.files[idFile];
    }
    public static int GetAmountWordsInSentence(string line) {
        return AuxiliarMethods.GetWordsOfSentence(line).Length;
    }
    public static string GetLeftContext(int idFile, int numLine, int numWord, int length, bool addWord) { 
        //todo:: Tanto numLine como numWord empieza desde cero
        
        StringBuilder context = new StringBuilder();
        StreamReader reader = new StreamReader(GetFileByID(idFile));            

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

        // SI es la primera palabra no hay contexto derecho
        if(numLine == numWord && numWord == 0) return context.ToString();

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
        StreamReader reader = new StreamReader(GetFileByID(idFile));            

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

    public static List< Tuple<string, string> > GetOperators(string query) {
        List<Tuple<string, string>> o = new List<Tuple<string, string>>();
        int n = query.Length;

        // Guardar las palabras que tienen el operador de cercania para despues juntarlos todos en una sola tupla
        List<string> aux = new List<string>(); 

        for(int i = 0; i < query.Length; i ++) {
            if(!AuxiliarMethods.IsOperator(query[i])) continue;

            // Sacar los operadores delante de la palabra en la posicion i
            string op = AuxiliarMethods.GetOperators(query, i);
            int j = i;
            while(++j < n && AuxiliarMethods.Ignore(query[j]));
            if(j >= n) break;
            // Sacar la palabra que esta a la derecha de la posicion j
            string wo = AuxiliarMethods.NormalizeWord(AuxiliarMethods.GetWord(query, j, "right"));
            int auxLen = wo.Length;
                    wo = Lemmatization.Stemmer(wo);

            // Validar los operadores segun mi criterio
            string operators = AuxiliarMethods.ValidOperators(op);
            if(operators == "") continue; // Si los operadores no son validos
            
            // Si no es un operador de cercania puedo guardar la palabra con sus operadores
            if(operators[0] != '~') {
                 o.Add(new Tuple<string, string>(operators, wo));
                i = j + auxLen - 1;
                continue;
            }
            
            // Como es un operador de cercania entonces encontramos la palabra anterior a ella
            int k = i;
            while( --k >= 0 && AuxiliarMethods.Ignore(query[k]) );
            if(k < 0) continue;
            string prev_wo = AuxiliarMethods.NormalizeWord(AuxiliarMethods.GetWord(query, k, "left"));
                   prev_wo = Lemmatization.Stemmer(prev_wo); 
            if(prev_wo == "") { // Si la palabra no existe el operador queda invalidado por lo que no la agregamos
                 i = j + auxLen - 1;
                 continue;
            }
            
            // Si es la misma que la ultima que pusimos entonces significa que tiene operador, por lo tanto 
            // la quitamos de la lista y la anadimos junto a la palara nueva
            if(o.Count > 0) { // Si hay palabras guardadas
                // Si la el operador de la palabra anterior es el de cercania, tenemos que cojer la segunda palabra de las que estan juntas con ese operador
                string x = ( o.Last().Item1 != "~" ) ? o.Last().Item2 : AuxiliarMethods.GetWordsOfSentence(o.Last().Item2).Last();
                
                if(prev_wo == x) { // Si la palabras coinciden las tomamos y las unimos
                    
                    // Si la palabra es simple
                    if(o.Last().Item1 != "~") prev_wo = o.Last().Item1 + o.Last().Item2;
                    else prev_wo = o.Last().Item2;
                   
                    o.RemoveAt(o.Count - 1);
                }
            }

            o.Add(new Tuple<string, string>( "~", prev_wo + " " + operators.Substring(1, operators.Length - 1) + wo));
            i = j + auxLen - 1;
        }

        return o;
    }


    public static float GetScore(ref float[] iWDoc, ref float[] wQuery) {
        // Si la query no continene operadores
        return info.Sim(ref iWDoc, ref wQuery);
    }



}