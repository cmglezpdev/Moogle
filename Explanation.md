

3. Bug al poner operadores de lante de un caracter que es omitido por el metodo que devuelve la palabra correspondiente a esos caracteres
4. Mientras sea posible, usar el hash en vez de string

Caracteres a ignorar:
! ¿ ? ( ) { } [ ] " ' : ; . , / ` | \ - _ 


5. Arreglar el suggestion para que imprima correctamente los caracteres

6. Disminuir el score de las palabras que estaban mal escritas
7. designar un umbral por si la cantidad de transformaciones que necesita una plabra es demasiado grande no la use

8. Implementar el Porter Algorithm con hashing

9. En la cercania
    .: Mientras mas cerca esten las palabras mas score le tenemos que dar
    .: Los documentos con menos palabras del conjunto de las cercanas tiene que tener menos score que las que tienen mas palabras

10. Cambiar el metodo para calcular la distancia minima en la cercania entre varias palabras xq es demasiado lento. Normalmente pra 4 palabras ya es super lento dependiendo de la frecuencia de las palabras