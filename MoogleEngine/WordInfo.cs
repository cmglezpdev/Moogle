namespace MoogleEngine;

public struct info {
    
    List<int> numLine = new List<int> ();
    List<int> numWord = new List<int> ();

    public info(int nl, int nw) {
        numLine.Add(nl);
        numWord.Add(nw);
    }

    public info() {}

    public void AddAppearance(int nl, int nw) {
        this.numLine.Add(nl);
        this.numWord.Add(nw);
    }

    public int AmountAppareance{
        get{return this.numLine.Count;}
    }

    public List<int> NumbersOfLine {
        get{return numLine;}
    }

    public List<int> NumbersOfWord {
        get{return numWord;}
    }



    public static float TFIDF(int doc, int w, int MaxFreq, ref List<List<info>> DWordsDocs ) {
        float tfidf = 0.00f;

        float tf = (float)(DWordsDocs[doc][w].AmountAppareance / MaxFreq);
        

        int ni = 0;
        for(int i = 0; i < DWordsDocs.Count; i ++) 
            if(DWordsDocs[i][w].AmountAppareance != 0) ni ++;
        
        float idf = (float)(1.00f + Math.Log10(DWordsDocs.Count / ni));
        float alfa = 0.5f;
        tfidf = (float)(alfa + (1.0f - alfa) * idf);

        return tfidf;
    }

    public static float TFIDF2(int wHash, ref Dictionary<int, int> FreqWordsQuery) {
        float tfidf = 0.00f;
        
        int MaxFreqQuery = 0, WordAppareance = 0;
        foreach(KeyValuePair<int, int> freq in FreqWordsQuery) {
            if(freq.Key == wHash)
                WordAppareance = freq.Value;
            MaxFreqQuery = Math.Max(MaxFreqQuery, freq.Value);
        }


        float tf = (float)(WordAppareance / MaxFreqQuery);
        float alfa = 0.5f;
        tfidf = (alfa + (1.00f - alfa) * tf);
       
        return tfidf;
    }



    // RANKING DE LOS DOCUMENTOS
    public static float Sim(ref float[] d, ref float[] q) {
        float MultVectors = 0.00f;
        int n = d.Length;
        for(int i = 0; i < n; i ++)
            MultVectors += (float)(d[i] * q[i]);

        float NormD = 0.00f, NormQ = 0.00f;
        for(int i = 0; i < n; i ++) {
            NormD += (float)(d[i] * d[i]);
            NormQ += (float)(q[i] * q[i]);
        }
        NormD = (float)Math.Sqrt(NormD);
        NormQ = (float)Math.Sqrt(NormQ);

        if(NormD == 0 || NormQ == 0 || MultVectors == 0) 
            return 0.00f;
        return MultVectors / (NormD * NormQ);
    }

}