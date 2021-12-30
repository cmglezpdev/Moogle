namespace MoogleEngine;

public struct info {
    
    List<int> numLine = new List<int> ();
    List<int> numWord = new List<int> ();

    public info(int nl, int nw) {
        this.numLine.Add(nl);
        this.numWord.Add(nw);
    }

    public info() {}

    public void AddAppearance(int nl, int nw) {
        this.numLine.Add(nl);
        this.numWord.Add(nw);
    }

    public int AmountAppareance{
        get{return this.numLine.Count;}
    }

    public (int, int) nthAppareance(int n) {
        if(n >= this.AmountAppareance) 
            throw new Exception("No existe esa aparicion en el documento");
        return(this.numLine[n], this.numWord[n]);
    }
    



    public static float TFIDF(int doc, int w, int MaxFreq, ref List<List<info>> PosInDocs ) {
        float tfidf = 0.00f;

        if(MaxFreq == 0)  // El documento esta en blanco 
            return tfidf;

        float tf = (float)(PosInDocs[doc][w].AmountAppareance / MaxFreq);
        int ni = 0;
        for(int i = 0; i < PosInDocs.Count; i ++) 
            if(PosInDocs[i][w].AmountAppareance != 0.00f) ni ++;
        
        float idf = (float)(1.00f + Math.Log10(PosInDocs.Count / ni));
        float alfa = 0.5f;
        tfidf = (float)(alfa + (1.0f - alfa) * tf) * idf;

        return tfidf;
    }

    public static float TFIDF(int wFreq, int MaxFreq) {
        float tfidf = 0.00f;

        if(MaxFreq == 0) // El documento esta en blanco
            return tfidf;

        float tf = (float)(wFreq / MaxFreq);
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