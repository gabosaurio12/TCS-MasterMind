# Notas

## Servicio de filtrado (FilterService)

- El diccionario de palabras castigables vive en la base de datos
- En la vista estática debe ser representado
    - Qué interfaz expone
- En la vista de secuencia debe verse el flujo paso a paso

```
Player sends messsage
        ↓
Server receives the message
        ↓
Server validates the message with the FilterService
        ↓
FilterService obtains the ForbiddenWordsDictionary from the DB
        ↓
FilterService reviews the message looking for words in the dictionary
        ↓
If the FilterService finds a coincidense returns false
        ↓
The Server receives the false and sends the username to the ReportService
        ↓
The ReportService adds a report to the player with that username, if it's his fifth report it will send back a message to the server telling him that
        ↓
The server receives that message and sends the username to the BanService
        ↓
The BanService will ban the player and not allow him to join to any match for 24 hours.
        ↓
The Server will send a message to the players explaining what happened and the message will appear as "censured message".
```

- En deployment se debe ver donde corre
    - De preferencia en el server