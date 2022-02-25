using MoogleEngine;

public static class ShowDocument {

    public static string ShowDoc( string file ) {

        StreamReader reader = new StreamReader(file);

        string document = "";
        string line = "";
        
        do{
            line = reader.ReadLine()!;
            document += ( line + "<br>" );
        } while(line != null);


        return document;
    }
}
