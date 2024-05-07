# Essentials

https://github.com/SmartStandards/Essentials

[![Build status](https://dev.azure.com/SmartOpenSource/Smart%20Standards%20(Allgemein)/_apis/build/status/SmartStandards.Essentials)](https://dev.azure.com/SmartOpenSource/Smart%20Standards%20(Allgemein)/_build/latest?definitionId=15)

## Enclosed Tuple

### Why?

The common tuple style "HelloWorld;Foo;Bar;Hello" has some limitations:

- You can not use simple **full text search** to find a tuple element
  - indexOf("Hello") would return 0 which is wrong (the correct elemt starts at 27)
  - indexOf("World") would return 5 which is wrong (there is no element like "World") 

- You can not distinguish between an **empty tuple** and a tuple with one **empty element**
  - If you use string.Split("") you will get an array containing one empty string element - so it's impossible to ever get an empty array.

- You can not reflect **null tuples** or **null elements**

Solution: Instead of putting the separator only **before** an element (except the first), you also put the separator at the beginning and the end of a tuple - so each tuple element will be enclosed by the separator. When searching for a tuple element, you also enclose the element name (e.g.";Hello;")

### Examples (C#, using SmartStandards Essentials Library Extensions)

    tupleBuilder = new StringBuilder(80);
    tupleBuilder.AppendToEnclosedTuple("HelloWorld").AppendToEnclosedTuple("Foo").AppendToEnclosedTuple("Bar").AppendToEnclosedTuple("Hello")

Will result in: "#HelloWorld#Foo#Bar#Hello#" (we use hash as default separator in order not to conflict with classical tuple style)
You could now use indexOf("#Hello#") (or in SQL: "SELECT * FROM Entities WHERE Tags LIKE '%#Hello#%'"").
If you need more convenience, plus null handling and escaping, you can use SmartStandards Essentials Library Extensions:

    string tuple =  @"#HelloWorld#Foo#Bar#Hello#"
    string[] tupleElements = tuple.SplitEnclosedTuple()

Will result in: ["HelloWorld", "Foo", "Bar", "Hello"]

### Syntax Definition For Enclosed Tuples

|Situation|Tuple Representation|Code Representation|Remarks|
|------------------------------------|------------------------|------------------------|------------|
|Collection itself is null           |"\0"                    |null                    |            |
|Collection is empty                 |""                      |{}                      |            |
|Collection contains one null element|"#\0#"                  |{null}                  |            |
|Collection contains one empty string|"##"                    |{""}                    |            |
|Collection contains one element     |"#Foo#"                 |{"Foo"}                 |            |
|Collection contains two elements    |"#Foo#Bar#"             |{"Foo","Bar"}           |            |
|Value contains escape char          |"#C:\\Temp#D:\\Temp#"   |{"C:\Temp","D:\Temp"}   |            |
|Value contains delimiter            |"#Mambo\#\Five#HeyJude#"|{"Mambo#Five","HeyJude"}|read below**|

*) Hashtag is used as delimiter, to avoid looking similar to a "classic" comma or semicolon tuple.

**) The delimiter ('#') needs to be escaped symmetrically, otherwise it would be "#Mambo\#Five#HeyJude#" which contains "#Five#" which looks like a tuple element.
