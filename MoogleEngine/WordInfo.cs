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
    


    public static float TFIDF(int w, int MaxFreq, int wFreq, ref List<List<info>> PosInDocs) {
        float tfidf = 0.00f;

        if(MaxFreq == 0)  // El documento esta en blanco 
            return tfidf;

        float tf = (float)(wFreq / MaxFreq);
        int ni = 0;
        for(int i = 0; i < PosInDocs.Count; i ++) 
            if(PosInDocs[i][w].AmountAppareance > 0) ni ++;
        
        float idf = (float)Math.Log(PosInDocs.Count / ni);
        tfidf = tf * idf; 

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
        return (float)(MultVectors / (NormD * NormQ));
    }

}