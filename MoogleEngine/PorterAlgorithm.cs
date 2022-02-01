namespace MoogleEngine;

public static class Lemmatization {

    public static string Stemmer(string word) {
        
        Tuple<int, int, int> t = Get_R1_R2_RV(word);
        
        return t.ToString();
    }


    private static Tuple<int, int, int> Get_R1_R2_RV(string word) {
        int r1, r2, rv;
        int n = word.Length;
        r1 = r2 = rv = n;

            // Calcular R1: El substring que esta despues de la primera no-vocal precedida por una vocal
        for(int i  = 1; i < n; i ++) 
            if( !IsVocal(word[i])  && IsVocal(word[i - 1])) {
                r1 = n - (i + 1); // tamano 
                break;
            }
        // Calcular R2: Lo mismo que R1 pero aplicado al propio R1
        for(int i = n - r1; i < n; i ++) 
                if( !IsVocal(word[i])  && IsVocal(word[i - 1])) {
                r2 = n - (i + 1); // tamano 
                break;
            }     
        
        // Calcular RV: 
        if(word.Length >=3) {

            if( IsConsonant(word[1]) ) {
                int i = 2;
                while(i < n && !IsVocal(word[i])) i ++;
                i ++;
                if(i < n) rv = n - i;
            }
            else 
            if( IsVocal(word[0]) && IsVocal(word[1]) ) {
                int i = 2;
                while(i < n && !IsConsonant(word[i])) i ++;
                i ++;
                if(i < n) rv = n - i;
            }
            else
            if( IsConsonant(word[0]) && IsVocal(word[1]) ) {
                rv = n - 3;
            }
        }


        return new Tuple<int, int, int> (r1, r2, rv);
    }





    private static bool IsVocal(char x) {
        return (x == 'a' || x == 'e' ||  x == 'i' ||  x == 'o' ||  x == 'u');
    }
    private static bool IsConsonant(char x) {
        return  x >= 'a' && x <= 'z' && 
              ( x != 'a' && x != 'e' &&  x != 'i' &&  x != 'o' &&  x != 'u');
    }



#region DataForPorterAlgoritm
    
    public static string[] Step0 = { "me", "se", "sela", "selo", "selas", "selos", "la", "le", "lo", "las", "les", "los", "nos" }; 

    public static string[] StepAfter0 = { "iéndo", "ándo", "ár", "ér", "ír", "ando", "iendo", "ar", "er", "ir", "yendo" };

    public static string[] Step1 = { "ito","itos","ita","itas","anza" ,"anzas","ico", "ica", "icos", "icas", "ismo", "ismos", "capaz", "ables", "ible", "ibles", "ista" ,"istas", "oso", "osa", "osos", "osas", "amiento", "amientos", "imiento", "imientos"
        ,"adora" ,"ador","ación" ,"adoras" ,"adores" ,"aciones" ,"ante" ,"antes" ,"ancia" ,"ancias"
        ,"encia", "encias"
        ,"ativo","ativa","or","edor"
        ,"amente","mente"
        ,"idad", "idades"
        ,"iva", "ivo", "ivas", "ivos" };

    public static string[] Step2a = {"yeron", "yendo", "yamos", "yais", "yan", "yen", "yas", "yes", "ya", "ye", "yo", "yó" };

    public static string[] Step2b1 = {"en", "es", "éis", "emos"};

    public static string[] Step2b2 = { "arían", "arías", "arán", "arás", "aríais", "aría", "aréis", "aríamos", "aremos", "ará",
            "aré", "erían", "erías", "erán", "erás", "eríais", "ería", "eréis", "eríamos", "eremos",
            "erá", "eré", "irían", "irías", "irán", "irás", "iríais", "iría", "iréis", "iríamos",
            "iremos", "irá", "iré", "aba", "ada", "ida", "ía", "ara", "iera", "ad", "ed", "id", "ase",
            "iese", "aste", "iste", "an", "aban", "ían", "aran", "ieran", "asen", "iesen", "aron",
            "ieron", "ado", "ido", "ando", "iendo", "ió", "ar", "er", "ir", "as", "abas", "adas",
            "idas", "ías", "aras", "ieras", "ases", "ieses", "ís", "áis", "abais", "íais", "arais",
            "ierais", "aseis", "ieseis", "asteis", "isteis", "ados", "idos", "amos", "ábamos", "íamos",
            "imos", "áramos", "iéramos", "iésemos", "ásemos" };

    public static string[] Step3a = {"os", "a", "o", "á", "í", "ó"};

    public static string[] Step3b = {"e", "é"};

#endregion



}