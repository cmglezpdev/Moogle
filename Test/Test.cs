using MoogleEngine;
using System.Diagnostics;

class Test{

    //* Para los test en donde se lea los ficheros hay que cambiar la ruta de lectura en el metodo
    //* de "../Content/" por "Content/"

    static void Main() {

        // string[] files = FilesMethods.ReadFolder();


        // TestGetWordsOfSentence();
        // TestGetWordStartIn();
        // TestGetWordEndIn();

        // TestReadFolder();   
        // TestGetNameFile();
        // TestGetLeftContext();
        // TestGetRightContext();
        // TestGetOperators();
        // BuildStepsForPorter();
        // TestLemmantization();

        // TestSynonymsDB();
        // TestBinarySearch();

        // System.Console.WriteLine(WorkingOperators.ValidOperators("~^**^^*^^**^^^^*****"));


        //? Print if all is OK
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("It's OK!😁");
    }




    // static void Assert(bool condition, string message) {
    //     if(!condition){
    //         throw new System.Exception(message);
    //     }
    // }

    // #region Test AuxiliarMethods
    // static void TestGetWordsOfSentence() {
    //     string sentence = "Hola, com!o est42an todos===,;hot";
    //     string[] solution = {"Hola", "com", "o", "est42an", "todos", "hot"};
    //     string[] result = AuxiliarMethods.GetWordsOfSentence(sentence);
    //     Assert(solution.Length == result.Length, "Test: TestGetWordsOfSentence() WRONG");
    //     bool ok = true;
    //     for(int i = 0; ok && i < solution.Length; i ++)
    //         ok = (bool)(solution[i] == result[i]);
    
    //     Assert(ok, "Test: TestGetWordsOfSentence() WRONG");
    // }
    // static void TestGetWordStartIn() {

    //     string sentence = "hol@ geT, Como est4n todos toda!!y";   
    //     Assert(AuxiliarMethods.GetWordStartIn(sentence, 8) == "", "Test: TestGetWordStartIn() WRONG");
    //     Assert(AuxiliarMethods.GetWordStartIn(sentence, 21) == "todos", "Test: TestGetWordStartIn() WRONG");
    //     Assert(AuxiliarMethods.GetWordStartIn(sentence, 28) == "oda", "Test: TestGetWordStartIn() WRONG");
    //     Assert(AuxiliarMethods.GetWordStartIn(sentence, 33) == "y", "Test: TestGetWordStartIn() WRONG");
    // }
    // static void TestGetWordEndIn() {
    //     string sentence = "hol@ geT, Como est4n todos toda!!y";   
    //     Assert(AuxiliarMethods.GetWordEndIn(sentence, 8) == "", "Test: TestGetWordStartIn() WRONG");
    //     Assert(AuxiliarMethods.GetWordEndIn(sentence, 25) == "todos", "Test: TestGetWordStartIn() WRONG");
    //     Assert(AuxiliarMethods.GetWordEndIn(sentence, 28) == "to", "Test: TestGetWordStartIn() WRONG");
    //     Assert(AuxiliarMethods.GetWordEndIn(sentence, 33) == "y", "Test: TestGetWordStartIn() WRONG");
    // }
    

    // #endregion

    // #region Test FilesMethods
    // static void TestReadFolder() {
    //     string[] files = FilesMethods.ReadFolder();
    //     int n = FilesMethods.GetTotalFiles();

    //     Assert(n == files.Length, "Test: TestReadFolder() WRONG");
    // }
    // static void TestGetNameFile() {
    //     string file = "Content/Nueva Carpeta/Otra/nombre archivo 021.txt";
    //     Assert(FilesMethods.GetNameFile(file) == "nombre archivo 021", "Test: TestGetNameFile() WRONG");
    // }
    // static void TestGetLeftContext() {
    //     // Tanto numLine como numWord empieza desde cero
    //     string result1 = FilesMethods.GetLeftContext(0, 51, 1, 5, true);
    //     Assert(result1 == "sea capaz de recuperar los fundamentales", "Test1 TestGetLeftContext() WRONG");

    //     string result2 = FilesMethods.GetLeftContext(0, 40, 0, 3, true);
    //     Assert(result2 == "04.03.2009                                                        ***          El", "Test2 TestGetLeftContext() WRONG");
        
    //     string result3 = FilesMethods.GetLeftContext(0, 140, 2, 9, true);
    //     Assert(result3 == "supervivencia de la vida en el planeta.          Los temas tratados", "Test3 TestGetLeftContext() WRONG");   
    
    //     string result4 = FilesMethods.GetLeftContext(0, 0, 1, 4, true);
    //     Assert(result4 == "                                                                            Polis, Revista", "Test4 TestGetLeftContext() WRONG");   
    // }
    // static void TestGetRightContext() {
    //     // Tanto numLine como numWord empieza desde cero
    //     string result1 = FilesMethods.GetRightContext(0, 51, 1, 5, true);
    //     Assert(result1 == "fundamentales aportes marxianos para la comprensión", "Test1 TestGetRightContext() WRONG");
        
    //     string result2 = FilesMethods.GetRightContext(0, 35, 4, 3, true);
    //     Assert(result2 == "Aceptado: 04.03.2009", "Test2 TestGetRightContext() WRONG");
        
    //     string result3 = FilesMethods.GetRightContext(0, 136, 11, 12, true);
    //     Assert(result3 == "es la posibilidad de supervivencia de la vida en el planeta.          Los temas", "Test3 TestGetRightContext() WRONG");   
    
    //     string result4 = FilesMethods.GetRightContext(0, 187, 7, 5, true);
    //     Assert(result4 == "esfer@speedy.com.ar ", "Test3 TestGetRightContext() WRONG");   
    // }

    // #endregion


    // static void TestGetOperators() {
    //     // string query = "Lo **mas ~ ^importante !es mi ~ *&mamolshito ";
    //     // string query = "!cuba ~ amor! ~ !esperanza, xq l@ viD@ es *#52mvcs!#";
    //     string query = "leon ~ zorro ~ muerto cuba ~~universidad **tigre ^familiares";

    //     List< Tuple<string, string> > x = FilesMethods.GetOperators(query);

    //     foreach (Tuple<string, string> item in x) {

    //         System.Console.WriteLine(item.ToString());   
    //     }  

    // }


    static void TestLemmantization() {
        
        Stopwatch crono = new Stopwatch();

        StreamReader Dicc = new StreamReader(@"D:\MATCOM\Programacion\Proyecto Primer Semestre\moogle-2021\Test\Vocabulary-lemantization.txt");
        StreamWriter errores =new StreamWriter("errors.txt");

        string t = Dicc.ReadToEnd();
        string[] text = t.Split('\n');
        Dicc.Close();
        int errors = 0;

        crono.Start();

        for(int i = 0; i < text.Length; i++) {
            // if(text[i] == "") continue;
            string[] x = AuxiliarMethods.GetWordsOfSentence(text[i]);
            if(AuxiliarMethods.IsLineWhite(text[i])) continue;
            string word = x[0];
            string lemman = x[1];

            string result = Lemmatization.Stemmer(word);
            if(result != lemman) {
                errors ++;
                // errores.WriteLine("Error con {0}. Se esperaba {1} y de devolvio {2}.", word, lemman, result);
                Console.WriteLine("Error con {0}. Se esperaba {1} y de devolvio {2}.", word, lemman, result);
            }
        }

        crono.Stop();
        System.Console.WriteLine("Time: {0}", crono.ElapsedMilliseconds );

      System.Console.WriteLine(errors);
        if(errors == 0) {
            Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("PAST!!");
        }
    
    }

    // static void TestSynonymsDB() {
    //     string path = @"D:\MATCOM\Programacion\Proyecto Primer Semestre\moogle-2021\SynonymsDB\synonyms_db.json";

    //     WorkingSynonyms synonyms = new WorkingSynonyms( path );
    //     System.Console.WriteLine(synonyms.GetLengthDB());
    
    
    // }

    // static void TestBinarySearch() {
    //     List<string> a = new List<string>();
    //     a.Add("amigo");
    //     a.Add("codeforces");
    //     a.Add("cubano");
    //     a.Add("culito");
    //     a.Add("manana");
    //     a.Add("pase");
    
    //     a.Sort();

    //     System.Console.WriteLine(AuxiliarMethods.BinarySearch(a, "gg"));
    
    // }

    static void BuildStepsForPorter() {
        string[] a = new string[20];
        a[0] = "á   é   í   ó   ú   ü   ñ";
        a[1] = "a   e   i   o   u   á   é   í   ó   ú   ü";
        a[2] = "me   se   sela   selo   selas   selos   la   le   lo   las   les   los   nos";
        a[3] = "yendo iéndo   ándo   ár   ér   ír ando   iendo   ar   er   ir";
        a[4] = "anza   anzas   ico   ica   icos   icas   ismo   ismos   able   ables   ible   ibles   ista   istas   oso   osa   osos   osas   amiento   amientos   imiento   imientos";
        a[5] = "adora   ador   ación   adoras   adores   aciones   ante   antes   ancia   ancias";
        a[6] = "logía   logías";
        a[7] = "ución   uciones";
        a[8] = "encia   encias";
        a[9] = "amante";
        a[10] = "mente";
        a[11] = "idad   idades";
        a[12] = "iva   ivo   ivas   ivos";
        a[13] = "ya   ye   yan   yen   yeron   yendo   yo   yó   yas   yes   yais   yamos";
        a[14] = "en   es   éis   emos";
        a[15] = "arían   arías   arán   arás   aríais   aría   aréis   aríamos   aremos   ará   aré   erían   erías   erán   erás   eríais   ería   eréis   eríamos   eremos   erá   eré   irían   irías   irán   irás   iríais   iría   iréis   iríamos   iremos   irá   iré   aba   ada   ida   ía   ara   iera   ad   ed   id   ase   iese   aste   iste   an   aban   ían   aran   ieran   asen   iesen   aron   ieron   ado   ido   ando   iendo   ió   ar   er   ir   as   abas   adas   idas   ías   aras   ieras   ases   ieses   ís   áis   abais   íais   arais   ierais     aseis   ieseis   asteis   isteis   ados   idos   amos   ábamos   íamos   imos   áramos   iéramos   iésemos   ásemos";
        a[16] = "os   a   o   á   í   ó";
        a[17] = "e   é";

        StreamWriter r = new StreamWriter("parts.txt");        
        for(int sec = 15; sec < 18; sec ++) {
            
            string[] parts = a[sec].Split(' ');
            string secuence = "";

            foreach(string v in parts) {
                if(v == "") continue;
                secuence += ( $"\"{v}\", " );
            }

            r.WriteLine(secuence);
            // System.Console.WriteLine(secuence);
        }







    }



} 
