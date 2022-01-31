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
    


    public static float TFIDF(int IndexWord, int MaxFreq, int wFreq, ref List<List<info>> PosInDocs) {
        float tfidf = 0.00f;

        if(MaxFreq == 0)  // El documento esta en blanco 
            return tfidf;

        float tf = (float)wFreq / (float)MaxFreq;
        int ni = 0, n = PosInDocs.Count;

        for(int i = 0; i < n; i ++) 
           if(PosInDocs[i][IndexWord].AmountAppareance > 0) ni ++;
        
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
        return MultVectors / (NormD * NormQ);
    }


    private static double DistanceBetweenWords(int x1, int y1, int x2, int y2) {
        return Math.Sqrt( (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) );
    }


    public static void buildTree(int doc, List< List<info> > PosInDocs, Dictionary<string, int> IdxWords, List<string> words, List< Tuple<int, int> > Points, double distance, int kword, int x, int y) {

        if(kword == words.Count)
            return;



        System.Console.WriteLine("Estoy en el buildTree");


        info Appareances = PosInDocs[ doc ][ IdxWords[ words[kword + 1] ] ];
        int cntAppareances = Appareances.AmountAppareance;
        
        // Recorrer todas las apariciones de la palabra kword
        for(int i = 0; i < cntAppareances; i ++) {
            int x1, y1;
            (x1, y1) = Appareances.nthAppareance(i);
        
            // Anadir link entre la aparicion (x, y) de kword  con la aparicion iesima de (kowrd + 1)
            Points.Add(new Tuple<int, int>(x1, y1));
            double dist = DistanceBetweenWords(x, y, x1, y1);

            buildTree(doc, PosInDocs, IdxWords, words, Points, distance + dist, kword + 1, x1, y1);
        }
    }





}