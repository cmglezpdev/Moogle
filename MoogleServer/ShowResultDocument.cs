using MoogleEngine;

public static class ShowDocument {

    public static string ShowDoc( string file ) {

        StreamReader reader = new StreamReader(file);

        string document = "";
        int TotalPrintLines = 100;
        int currentLine = 0;
        string line = "";
        
        do{
            line = reader.ReadLine()!;
            document += ( line + "<br>" );
            currentLine ++;
        } while(currentLine < TotalPrintLines && line != null);


        return document;
    }
}
