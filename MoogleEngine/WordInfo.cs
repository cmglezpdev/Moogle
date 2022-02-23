namespace MoogleEngine;

public class info {
    
    private List<int> numLine = new List<int> ();
    private List<int> numWord = new List<int> ();
    private float weight = 0;

    public info() {
        this.numLine = new List<int>();
        this.numWord = new List<int>();
        this.weight = 0f;
    }

    public void AddAppearance(int nl, int nw) {
        this.numLine.Add(nl);
        this.numWord.Add(nw);
    }

    public int AmountAppareance{
        get{return this.numLine.Count;}
    }

    public (int, int) nthAppareance(int n) {
        if(n >= this.AmountAppareance) 
            throw new Exception("No existe esa aparicion en el documento.");
        return(this.numLine[n], this.numWord[n]);
    }
    
    public float WeigthWord {
        get{return this.weight;}
        set{this.weight = value;}
    }


    public static float TFIDF(string word, int MaxFreq, int AmountAppareance) {
        float tfidf = 0.00f;

        if(MaxFreq == 0)  // El documento esta en blanco 
            return tfidf;

        float tf = (float)AmountAppareance / (float)MaxFreq;
        int ni = 0, n = Data.TotalFiles;

        for(int i = 0; i < n; i ++) {
           if(Data.PosInDocs[i].ContainsKey(word)) ni ++;
        }
        
        double division = (double)n / (double)ni;
        float idf = (float)Math.Log10(division);
        tfidf = tf * idf;
        return tfidf;
    }


    // RANKING DE LOS DOCUMENTOS
    public static float Sim(int doc, Dictionary<string, Tuple<int, float>> wquery) {
        float MultVectors = 0.00f;
        foreach(var i in wquery) {
            if( Data.PosInDocs[doc].ContainsKey(i.Key) )
                MultVectors += ( i.Value.Item2 * Data.PosInDocs[doc][i.Key].WeigthWord );
        }     

        float NormD = 0.00f, NormQ = 0.00f;
        foreach(var i in wquery)
            NormQ += (i.Value.Item2 * i.Value.Item2);
        foreach(var i in Data.PosInDocs[doc]) 
            NormD += ( Data.PosInDocs[doc][i.Key].WeigthWord * Data.PosInDocs[doc][i.Key].WeigthWord);

        NormD = (float)Math.Sqrt((double)NormD);
        NormQ = (float)Math.Sqrt((double)NormQ);

        if(NormD == 0.00f || NormQ == 0.00f || MultVectors == 0.00f) 
            return 0.00f;

        return MultVectors / (NormD * NormQ);
    }

}