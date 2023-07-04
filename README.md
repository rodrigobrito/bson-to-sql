# BsonToMySQL
Transforms MongoDB documents into SQL scripts, "normalizing" the document to SQL databases. It's a simple utility, created in a few hours of work that can be used to create a more robust tool for this purpose.

## how to run the utility



Linux:
```bash
$ wget https://github.com/rodrigobrito/bson-to-sql/releases/download/v1.0.0/BsonToMySQL-linux-x64.tar.gz
$ tar -xzvf BsonToMySQL-linux-x64.tar.gz
$ chmod 744 BsonToMySQL
$ ./BsonToMySQL -f ./your-bson-filename -t prefixTablesName
```



## how to build
Build to Linux:
```bash
$ dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true
```

Build to MacOS Apple silicon:
```bash
dotnet publish  -c Release -r osx-arm64 -p:PublishSingleFile=true -o arm64 --self-contained true
```

Build to Windows:
```bash
$ dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true
```