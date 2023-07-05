# BsonToMySQL
Transforms MongoDB documents into SQL scripts, "normalizing" the document to SQL databases. It's a simple utility, created in a few hours of work that can be used to create a more robust tool for this purpose.

## How to run this utility quickly.

Linux x64:
```bash
$ wget https://github.com/rodrigobrito/bson-to-sql/releases/download/v1.0.0/BsonToMySQL-linux-x64.tar.gz
$ tar -xzvf BsonToMySQL-linux-x64.tar.gz
$ chmod 744 BsonToMySQL
$ ./BsonToMySQL -f ./your-bson-file -t prefixTablesName
```

OSX arm64:
```bash
$ curl -L https://github.com/rodrigobrito/bson-to-sql/releases/download/v1.0.0/BsonToMySQL-osx-arm64.tgz -o BsonToMySQL-osx-arm64.tgz
$ tar -xvf BsonToMySQL-osx-arm64.tgz
$ ./BsonToMySQL -f ./your-bson-file -t prefixTablesName
```

Windows x64
```powershell
$ Invoke-WebRequest -URI https://github.com/rodrigobrito/bson-to-sql/releases/download/v1.0.0/BsonToMySQL-win-x64.zip -OutFile BsonToMySQL-win-x64.zip
$ Expand-Archive -Path BsonToMySQL-win-x64.zip -DestinationPath .\
$ ./BsonToMySQL.exe -f ./your-bson-filen -t prefixTablesName
```

Given a file `simple.bson`, in the following format:
```json
{
    "_id" : ObjectId("623232af986a0200435765aa"),
    "skuId" : NumberInt(1),
    "productId" : NumberInt(1),
    "isActive" : true,    
    "name" : "PRETO-48",
    "sku" : "100000289720UC9948",
    "attributes" : [
        {
            "attributeId" : NumberInt(4),
            "attributeValue" : "48",
            "attributeContent" : "54 EUROPA - 48 BRASIL",
            "attributeValueId" : NumberInt(40),
            "name" : "Tamanho"
        },
        {
            "attributeId" : NumberInt(3),
            "attributeValue" : "Preto",
            "attributeContent" : "foto",
            "attributeValueId" : NumberInt(11),
            "name" : "Cor"
        }
    ],
    "storeId" : ObjectId("61f92d9dde31fd001143bc16"),
    "storeCode" : "Grupo sample-account",
    "integrations" : [
          1,
          2
    ],
    "history" : [
          "history1",
          "history2",
    ],
    "createdAt" : ISODate("2022-03-16T18:55:43.476+0000"),
    "updatedAt" : ISODate("2023-06-05T20:37:23.724+0000"),
    "medias" :
    {
            "sortOrder" : NumberInt(0),
            "fileId" : "1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b",
            "text" : "100000289720UC9948-CAMISETA_1.JPG",
            "mediaType" : "photo",
            "name" : "CAMISETA",
            "label" : "CAMISETA",
            "url" : "https://media.ifcshop.com.br/contexts/catalog/company/masculino/roupas/camisetas-e-polos/camiseta-borgonuovo-de-algodao-48-preto?id=1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b",            
            "metadata" : {
                "contentType" : "image/jpeg",
                "height" : NumberInt(2702),
                "width" : NumberInt(1920)
            }
    },
    "contentMedia" : "yes",
    "activationDate" : null,
    "inactivationDate" : null,
    "giftPackaging" : false
},
{
    "_id" : ObjectId("623232af986a0200435765ab"),
    "skuId" : NumberInt(1),
    "productId" : NumberInt(1),
    "isActive" : true,    
    "name" : "PRETO-48",
    "sku" : "100000289720UC9948",
    "attributes" : [
        {
            "attributeId" : NumberInt(4),
            "attributeValue" : "48",
            "attributeContent" : "54 EUROPA - 48 BRASIL",
            "attributeValueId" : NumberInt(40),
            "name" : "Tamanho"
        },
        {
            "attributeId" : NumberInt(3),
            "attributeValue" : "Preto",
            "attributeContent" : "foto",
            "attributeValueId" : NumberInt(11),
            "name" : "Cor"
        }
    ],
    "storeId" : ObjectId("61f92d9dde31fd001143bc16"),
    "storeCode" : "Grupo sample-account",
    "integrations" : [
          1,
          2
    ],
    "history" : [
          "history1",
          "history2",
    ],
    "createdAt" : ISODate("2022-03-16T18:55:43.476+0000"),
    "updatedAt" : ISODate("2023-06-05T20:37:23.724+0000"),
    "medias" :
    {
            "sortOrder" : NumberInt(0),
            "fileId" : "1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b",
            "text" : "100000289720UC9948-CAMISETA_1.JPG",
            "mediaType" : "photo",
            "name" : "CAMISETA",
            "label" : "CAMISETA",
            "url" : "https://media.ifcshop.com.br/contexts/catalog/company/masculino/roupas/camisetas-e-polos/camiseta-borgonuovo-de-algodao-48-preto?id=1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b",            
            "metadata" : {
                "contentType" : "image/jpeg",
                "height" : NumberInt(2702),
                "width" : NumberInt(1920)
            }
    },
    "contentMedia" : "yes",
    "activationDate" : null,
    "inactivationDate" : null,
    "giftPackaging" : false
}
```
The output will be following after running this command: `./BsonToMySQL -f ./simple.bson -t simple`:
```sql
CREATE TABLE IF NOT EXISTS simple (
   _id VARCHAR(24) 
  ,skuId BIGINT DEFAULT NULL
  ,productId BIGINT DEFAULT NULL
  ,isActive TINYINT DEFAULT NULL
  ,name VARCHAR(8) DEFAULT NULL
  ,sku VARCHAR(18) DEFAULT NULL
  ,storeId VARCHAR(24) DEFAULT NULL
  ,storeCode VARCHAR(20) DEFAULT NULL
  ,createdAt VARCHAR(20) DEFAULT NULL
  ,updatedAt VARCHAR(20) DEFAULT NULL
  ,contentMedia VARCHAR(3) DEFAULT NULL
  ,activationDate VARCHAR(3) DEFAULT NULL
  ,inactivationDate VARCHAR(3) DEFAULT NULL
  ,giftPackaging TINYINT DEFAULT NULL
);
ALTER TABLE simple ADD INDEX idx_0f7d0d088b6ea936fb25b477722d734706fe8b40_id(_id); 

CREATE TABLE IF NOT EXISTS simple_attributes (
   _id VARCHAR(24) 
  ,attributeId BIGINT DEFAULT NULL
  ,attributeValue VARCHAR(5) DEFAULT NULL
  ,attributeContent VARCHAR(21) DEFAULT NULL
  ,attributeValueId BIGINT DEFAULT NULL
  ,name VARCHAR(7) DEFAULT NULL
);
ALTER TABLE simple_attributes ADD INDEX idx_2cb6a4d15f998b4a7bcd21ef7835292bbdbe174f_id(_id); 

CREATE TABLE IF NOT EXISTS simple_integrations (
   _id VARCHAR(24) 
  ,simple_integrations BIGINT DEFAULT NULL
);
ALTER TABLE simple_integrations ADD INDEX idx_6a7541027cdd0594841e366d337e55ab5d153bcf_id(_id); 

CREATE TABLE IF NOT EXISTS simple_history (
   _id VARCHAR(24) 
  ,simple_history VARCHAR(8) DEFAULT NULL
);
ALTER TABLE simple_history ADD INDEX idx_a5b5843cfe72594fa59e7294e2795b15350eb9e6_id(_id); 

CREATE TABLE IF NOT EXISTS simple_medias (
   _id VARCHAR(24) 
  ,sortOrder BIGINT DEFAULT NULL
  ,fileId VARCHAR(36) DEFAULT NULL
  ,text VARCHAR(33) DEFAULT NULL
  ,mediaType VARCHAR(5) DEFAULT NULL
  ,name VARCHAR(8) DEFAULT NULL
  ,label VARCHAR(8) DEFAULT NULL
  ,url VARCHAR(168) DEFAULT NULL
);
ALTER TABLE simple_medias ADD INDEX idx_9f9461aff2db8ffcf82957abdc1d745ca43148de_id(_id); 

CREATE TABLE IF NOT EXISTS simple_medias_metadata (
   _id VARCHAR(24) 
  ,contentType VARCHAR(10) DEFAULT NULL
  ,height BIGINT DEFAULT NULL
  ,width BIGINT DEFAULT NULL
);
ALTER TABLE simple_medias_metadata ADD INDEX idx_77519ac993b0db70cbd04f2ae66ad41a4c107e6b_id(_id); 


INSERT INTO simple (_id, skuid, productid, isactive, name, sku, storeid, storecode, createdat, updatedat, contentmedia, activationdate, inactivationdate, giftpackaging) VALUES 
(  '623232af986a0200435765aa', 1, 1, True, 'PRETO-48', '100000289720UC9948', '61f92d9dde31fd001143bc16', 'Grupo sample-account', '2022-03-16T18:55:43Z', '2023-06-05T20:37:23Z', 'yes', NULL, NULL, False);


INSERT INTO simple_attributes (_id, attributeid, attributevalue, attributecontent, attributevalueid, name) VALUES 
(  '623232af986a0200435765aa', 4, '48', '54 EUROPA - 48 BRASIL', 40, 'Tamanho');

INSERT INTO simple_attributes (_id, attributeid, attributevalue, attributecontent, attributevalueid, name) VALUES 
(  '623232af986a0200435765aa', 3, 'Preto', 'foto', 11, 'Cor');


INSERT INTO simple_integrations (_id, simple_integrations) VALUES 
(  '623232af986a0200435765aa', 1);

INSERT INTO simple_integrations (_id, simple_integrations) VALUES 
(  '623232af986a0200435765aa', 2);


INSERT INTO simple_history (_id, simple_history) VALUES 
(  '623232af986a0200435765aa', 'history1');

INSERT INTO simple_history (_id, simple_history) VALUES 
(  '623232af986a0200435765aa', 'history2');


INSERT INTO simple_medias (_id, sortorder, fileid, text, mediatype, name, label, url) VALUES 
(  '623232af986a0200435765aa', 0, '1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b', '100000289720UC9948-CAMISETA_1.JPG', 'photo', 'CAMISETA', 'CAMISETA', 'https://media.ifcshop.com.br/contexts/catalog/company/masculino/roupas/camisetas-e-polos/camiseta-borgonuovo-de-algodao-48-preto?id=1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b');


INSERT INTO simple_medias_metadata (_id, contenttype, height, width) VALUES 
(  '623232af986a0200435765aa', 'image/jpeg', 2702, 1920);


INSERT INTO simple (_id, skuid, productid, isactive, name, sku, storeid, storecode, createdat, updatedat, contentmedia, activationdate, inactivationdate, giftpackaging) VALUES 
(  '623232af986a0200435765ab', 1, 1, True, 'PRETO-48', '100000289720UC9948', '61f92d9dde31fd001143bc16', 'Grupo sample-account', '2022-03-16T18:55:43Z', '2023-06-05T20:37:23Z', 'yes', NULL, NULL, False);


INSERT INTO simple_attributes (_id, attributeid, attributevalue, attributecontent, attributevalueid, name) VALUES 
(  '623232af986a0200435765ab', 4, '48', '54 EUROPA - 48 BRASIL', 40, 'Tamanho');

INSERT INTO simple_attributes (_id, attributeid, attributevalue, attributecontent, attributevalueid, name) VALUES 
(  '623232af986a0200435765ab', 3, 'Preto', 'foto', 11, 'Cor');


INSERT INTO simple_integrations (_id, simple_integrations) VALUES 
(  '623232af986a0200435765ab', 1);

INSERT INTO simple_integrations (_id, simple_integrations) VALUES 
(  '623232af986a0200435765ab', 2);


INSERT INTO simple_history (_id, simple_history) VALUES 
(  '623232af986a0200435765ab', 'history1');

INSERT INTO simple_history (_id, simple_history) VALUES 
(  '623232af986a0200435765ab', 'history2');


INSERT INTO simple_medias (_id, sortorder, fileid, text, mediatype, name, label, url) VALUES 
(  '623232af986a0200435765ab', 0, '1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b', '100000289720UC9948-CAMISETA_1.JPG', 'photo', 'CAMISETA', 'CAMISETA', 'https://media.ifcshop.com.br/contexts/catalog/company/masculino/roupas/camisetas-e-polos/camiseta-borgonuovo-de-algodao-48-preto?id=1ebda2f8-85a6-4f39-ad3f-29b5e0979f0b');


INSERT INTO simple_medias_metadata (_id, contenttype, height, width) VALUES 
(  '623232af986a0200435765ab', 'image/jpeg', 2702, 1920);
```

Use `-f ` To specify the bson file that will be transformed into the sql script.

Use `-t`  To specify the name prefix for the tables that will be created in the DDL.
```bash
./BsonToMySQL -f ./your-bson-file -t prefixTablesName
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
