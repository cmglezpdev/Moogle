// using MoogleEngine;
namespace MoogleEngine;

class Test{

    static void Main() {

        Files[] files = FilesMethods.ReadFolder();
        showSortFiles(files);


        //? Print if all is OK
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("It's OK!😁");
    }


    static void Assert(bool condition, string message) {
        if(!condition)
            throw new System.Exception(message);
    }

    // Arreglar el Sort que da overflow
    static void showSortFiles(Files[] files) {
        string[] words = {"JS"};
        Files[] aux = files;
        FilesMethods.Sort(ref aux, ref words);

        foreach(Files x in aux) 
            Console.WriteLine(x.Name);
    }

} 
