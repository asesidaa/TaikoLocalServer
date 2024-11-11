# Server

This is the solution for server.  
Server is implemented with ASP.NET Core 6. ORM is Entity Framework Core 6. Database is SQLite for easier setup.  
As the game uses protobuf, `protobuf-net` is used for serializing and deserializing the data.  

## Datatable documentation

The **verupNo** field is the version number of your list. You'll have to increment it each time you make an edit for the game tu pull the new version (otherwise it'll use an old cached copy !)

### event_folder_data

```json
[
    {
        // Touhou Project Special
        // A collection of special songs!
        "folderId": 1,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // The Idolmaster Special
        // A collection of special songs!
        "folderId": 2,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Highly Recommended Songs
        // Why don’t you start with these popular songs?
        "folderId": 3,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // ?
        "folderId": 4,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Studio Ghibli Feature
        // A collection of special songs!
        "folderId": 5,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Yokai Watch Special
        // A collection of special songs!
        "folderId": 6,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // UUUM Creator Feature
        // A collection of special songs!
        "folderId": 7,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Soshina's Playlist
        // A collection of songs produced by Soshina!
        "folderId": 8,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Recommended Playlist
        // Soshina's handpicked songs to battle to!
        "folderId": 9,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Championship Songs
        // Assigned songs for the World Championship
        "folderId": 10,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // [Bonus] 2023 Championship Songs
        // We gathered up some songs from the online 2023 World's Online Championship Match [Bonus] !
        "folderId": 11,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // #Compass Special
        // Here is a collection of songs from #Compass!
        "folderId": 12,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Winter Seasonal Songs Pack
        // A collection of songs to play in the winter!
        "folderId": 13,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // World Popular Songs
        // We've collected some of the world's most popular songs!
        "folderId": 14,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    },
    {
        // Taiko no Tatsujin 20th Anniversary Songs
        // A collection of “Taiko no Tatsujin” 20th Anniversary Songs!
        "folderId": 14,
        "verupNo": 1,
        "priority": 1,
        "songNo": []
    }
]
```
