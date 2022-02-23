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
    


    public float TFIDF(string word, int doc, int MaxFreq) {
        float tfidf = 0.00f;

        if(MaxFreq == 0)  // El documento esta en blanco 
            return tfidf;

        float tf = (float)this.AmountAppareance / (float)MaxFreq;
        int ni = 0, n = Data.TotalFiles;

        for(int i = 0; i < n; i ++) {
           if(Data.PosInDocs[doc].ContainsKey(word)) ni ++;
        }
        
        double division = (double)n / (double)ni;
        float idf = (float)Math.Log10(division);
        tfidf = tf * idf;
        return tfidf;
    }


    // RANKING DE LOS DOCUMENTOS
    public static float Sim(ref float[] d, ref float[] q) {
        float MultVectors = 0.00f;
        int n = d.Length;
        for(int i = 0; i < n; i ++)
            MultVectors += (d[i] * q[i]);

        float NormD = 0.00f, NormQ = 0.00f;
        for(int i = 0; i < n; i ++) {
            NormD += (float)(d[i] * d[i]);
            NormQ += (float)(q[i] * q[i]);
        }
        NormD = (float)Math.Sqrt((double)NormD);
        NormQ = (float)Math.Sqrt((double)NormQ);

        if(NormD == 0.00f || NormQ == 0.00f || MultVectors == 0.00f) 
            return 0.00f;

            // System.Console.WriteLine(MultVectors / (NormD * NormQ));
        return MultVectors / (NormD * NormQ);
    }

}