
Caracteres a ignorar:
! ¿ ? ( ) { } [ ] " ' : ; . , / ` | \ - _


1. Arreglar el suggestion para que imprima correctamente los caracteres

2. En la cercania
    .: Los documentos con menos palabras del conjunto de las cercanas tiene que tener menos score que las que tienen mas palabras

3.  Cambiar el metodo para calcular la distancia minima en la cercania entre varias palabras xq es demasiado lento. Normalmente pra 4 palabras ya es super lento dependiendo de la frecuencia de las palabras


4.  Arreglar el suggestion
5.  MOdifcar el score de las palabras mal escritas y las buscadas del diccionario para que salgan con menos score
6.  Eliminar las cosas raras que hice para llamar al NormalizeWord en los diferentes metodos
7.  Anadir al SaveData una lista con la cantidad de palabras por linea para el calculo despues de la cercania